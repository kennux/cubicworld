using UnityEngine;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Cubic terrain.
/// 
/// IMPORTANT: ATerrainGeneration implementation must be added to the same gameobject this script is added to.
/// </summary>
public class CubicTerrain : MonoBehaviour
{
	/// <summary>
	/// The player transform.
	/// Used to generate the world near the player.
	/// </summary>
	public Transform playerTransform;

	/// <summary>
	/// The chunk preload radius.
	/// </summary>
	public int chunkPreloadRadius = 2;

	// Config
	public int chunkWidth=32;
	public int chunkHeight=16;
	public int chunkDepth=32;

	// Generator
	private ATerrainGenerator terrainGenerator;

	// Lock objects
	private object generationLockObject = new object();

	/// <summary>
	/// The terrain material.
	/// </summary>
	public Material terrainMaterial;

	#region Helper functions and classes
	
	/// <summary>
	/// Gets the chunk position for the given worldspace.
	/// </summary>
	/// <returns>The chunk position.</returns>
	/// <param name="worldspace">Worldspace.</param>
	public Vector3 GetChunkPosition(Vector3 worldspace)
	{
		int x = 0;
		int z = 0;
		float xF = ((worldspace.x - this.transform.position.x) / this.chunkWidth);
		float zF = ((worldspace.z - this.transform.position.z) / this.chunkDepth);
		
		if (x < 0)
			x = Mathf.CeilToInt(xF);
		else
			x = Mathf.FloorToInt(xF);
		
		if (z < 0)
			z = Mathf.CeilToInt(zF);
		else
			z = Mathf.FloorToInt(zF);
		
		return new Vector3
		(
			x,
			0,
			z
		);
	}

	// Chunk generation job
	// This class holds information which gets used in the generation thread for generating chunks.
	class ChunkGenerationJob
	{
		public CubicTerrainData terrainChunkData;
		public bool done = false;
		public Vector3 worldspace;

		public ChunkGenerationJob(CubicTerrainData terrainChunkData, Vector3 worldspace)
		{
			this.terrainChunkData = terrainChunkData;
			this.worldspace = worldspace;
		}
	}
	
	// Chunk tuple implementation
	// Used to identify chunks in a Dictionary
	class ChunkTuple : System.Object
	{
		public int x,z;
		
		public ChunkTuple(int x, int z)
		{
			this.x = x;
			this.z = z;
		}
		
		public override bool Equals(object other)
		{
			ChunkTuple otherTuple = (ChunkTuple) other;
			
			if (otherTuple != null)
			{
				return (otherTuple.x == this.x && otherTuple.z == this.z);
			}
			
			return false;
		}
		
		public static bool operator ==(ChunkTuple a, ChunkTuple b)
		{
			if (((object)a == null) || ((object)b == null))
				return false;
			
			return a.Equals(b);
		}
		
		public static bool operator !=(ChunkTuple a, ChunkTuple b)
		{
			return !(a == b);
		}
		
		public override string ToString()
		{
			return "ChunkTuple: " + this.x + "|" + this.z;
		}
		
		public override int GetHashCode()
		{
			return ((short)this.z >> 16) | this.x;
		}
	}

	#endregion

	/// <summary>
	/// The chunk generation thread.
	/// </summary>
	private Thread chunkGenerationThread;

	/// <summary>
	/// The chunk game objects.
	/// </summary>
	private Dictionary<ChunkTuple, GameObject> chunkObjects;

	/// <summary>
	/// The generation jobs.
	/// </summary>
	private Dictionary<ChunkTuple, ChunkGenerationJob> generationJobs;

	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Start()
	{
		this.terrainGenerator = this.GetComponent<ATerrainGenerator> ();
		this.chunkGenerationThread = new Thread (this.ChunkGenerationThread);
		this.chunkGenerationThread.Start ();

		// Initialize dictionaries
		this.chunkObjects = new Dictionary<ChunkTuple, GameObject> ();
		this.generationJobs = new Dictionary<ChunkTuple, ChunkGenerationJob> ();

		// Init
		this.terrainMaterial.SetTexture ("_MainTex", Blocks.textureAtlas);
		Vector3 chunkPosition = this.GetChunkPosition(this.playerTransform.position);
		this.GenerateChunk((int)chunkPosition.x,(int)chunkPosition.z);
	}

	/// <summary>
	/// Update this instance.
	/// Loads new chunk and deletes old chunks.
	/// </summary>
	public void Update()
	{
		// Load needed chunks
		Vector3 chunkPosition = this.GetChunkPosition(this.playerTransform.position);

		for (int x = (int)chunkPosition.x - this.chunkPreloadRadius; x <= (int)chunkPosition.x + this.chunkPreloadRadius; x++)
		{
			for (int z = (int)chunkPosition.z - this.chunkPreloadRadius; z <= (int)chunkPosition.z + this.chunkPreloadRadius; z++)
			{
				if (! this.chunkObjects.ContainsKey(new ChunkTuple(x,z)))
				{
					this.GenerateChunk(x,z);
				}
			}
		}

		this.UpdateGenerationData ();
		this.CollectGarbage();
	}

	/// <summary>
	/// Generates the chunk generation job for x|z
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	private void GenerateChunk(int x, int z)
	{
		// Create gameobject
		Vector3 chunkPosition = this.transform.position + new Vector3
		(
			x * this.chunkWidth,
			0,
			z * this.chunkDepth
		);
		
		GameObject chunkObject = new GameObject ("Chunk (" + x + "|" + z + ")");
		chunkObject.transform.position = chunkPosition;
		chunkObject.transform.parent = this.transform;
		chunkObject.layer = this.gameObject.layer;

		this.chunkObjects.Add (new ChunkTuple (x, z), chunkObject);
		CubicTerrainChunk terrainChunk = chunkObject.AddComponent<CubicTerrainChunk> ();
		terrainChunk.terrainMaterial = this.terrainMaterial;
		terrainChunk.chunkPosition = new Vector3 (x, 0, z);

		lock (this.generationLockObject)
		{
			this.generationJobs.Add (new ChunkTuple(x,z), new ChunkGenerationJob(new CubicTerrainData(this.chunkWidth, this.chunkHeight, this.chunkDepth), chunkPosition));
		}
	}

	/// <summary>
	/// Generates all chunks in chunkGenerationQuene
	/// </summary>
	private void ChunkGenerationThread()
	{
		while (true)
		{
			lock(this.generationLockObject)
			{
				foreach (KeyValuePair<ChunkTuple, ChunkGenerationJob> job in this.generationJobs)
				{
					if (! job.Value.done)
					{
						// Generate chunk
						this.terrainGenerator.GenerateTerrainData(job.Value.terrainChunkData, job.Value.worldspace);
						job.Value.done = true;
					}
				}
			}

			Thread.Sleep (100);
		}
	}

	/// <summary>
	/// Updates the generation data.
	/// If there are finished chunk generations their data will get added to the chunk object.
	/// </summary>
	private void UpdateGenerationData()
	{
		lock(this.generationLockObject)
		{
			List<ChunkTuple> jobsToDelete = new List<ChunkTuple>();
			foreach (KeyValuePair<ChunkTuple, ChunkGenerationJob> job in this.generationJobs)
			{
				if (job.Value.done)
				{
					// Set chunk data
					CubicTerrainChunk chunk = this.chunkObjects[job.Key].GetComponent<CubicTerrainChunk>();
					chunk.master=this;
					chunk.chunkData = job.Value.terrainChunkData;

					// Mark job for removal
					jobsToDelete.Add (job.Key);
				}
			}

			// Delete jobs marked for removal
			foreach(ChunkTuple t in jobsToDelete)
			{
				this.generationJobs.Remove (t);
			}
		}
	}

	/// <summary>
	/// Collects the garbage (not needed chunks).
	/// </summary>
	private void CollectGarbage()
	{
		List<ChunkTuple> chunksToDelete = new List<ChunkTuple>();
		
		// Get chunks to delete
		Vector3 chunkVector = Vector3.zero;
		foreach (KeyValuePair<ChunkTuple, GameObject> chunk in this.chunkObjects)
		{
			chunkVector = new Vector3(chunk.Key.x, 0, chunk.Key.z);
			if (Vector3.Distance(chunkVector, this.GetChunkPosition(this.playerTransform.position)) > this.chunkPreloadRadius * 2)
			{
				chunksToDelete.Add (chunk.Key);
			}
		}
		
		// Delete chunks
		foreach (ChunkTuple t in chunksToDelete)
		{
			Destroy (this.chunkObjects[t]);
			this.chunkObjects.Remove (t);
		}
	}

	/// <summary>
	/// Gets the chunk object for the given chunk positions.
	/// </summary>
	/// <returns>The chunk object.</returns>
	/// <param name="chunkX">Chunk x.</param>
	/// <param name="chunkZ">Chunk z.</param>
	public GameObject GetChunkObject(int chunkX, int chunkZ)
	{
		return this.chunkObjects [new ChunkTuple (chunkX, chunkZ)];
	}
}
