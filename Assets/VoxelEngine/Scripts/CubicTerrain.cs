using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.IO;

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

	/// <summary>
	/// If set to true then the chunk where the player stands on will get loaded first.
	/// </summary>
	public bool loadPlayerChunkFirst = false;

	// Generator
	private ATerrainGenerator terrainGenerator;

	// Lock objects
	private object generationLockObject = new object();

	/// <summary>
	/// The terrain material.
	/// </summary>
	public Material terrainMaterial;
	public Material transparentTerrainMaterial;

	/// <summary>
	/// If smooth chunk loading is activated, lag will get prevented by not loading all chunks at once.
	/// </summary>
	public bool smoothChunkLoading;

	/// <summary>
	/// If this is set to true chunks will get mesh colliders added to their game objects.
	/// If not the CubicWorld physics system will get used (NOT DONE YET).
	/// However, CubicWorld's physics system is NOT compatible with unity physics and VERY less precise (It is only a simple "block" physics system).
	/// </summary>
	public bool useMeshColliders = true;

	/// <summary>
	/// If this is turned on, the terrain will not load from the given terrain file
	/// </summary>
	public bool serializeTerrain;

	public string chunkFilesPath;

	[HideInInspector]
	public CubicTerrainFile terrainFile;
	
	/// <summary>
	/// The chunk generation thread.
	/// </summary>
	private Thread chunkGenerationThread;
	
	/// <summary>
	/// The chunk game objects.
	/// </summary>
	private Dictionary<ChunkTuple, GameObject> chunkObjects;

	/// <summary>
	/// The chunk data.
	/// </summary>
	private Dictionary<ChunkTuple, CubicTerrainData> chunkData;
	
	/// <summary>
	/// The generation jobs.
	/// </summary>
	private Dictionary<ChunkTuple, ChunkGenerationJob> generationJobs;

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

	#endregion

	#region Singleton
	private static CubicTerrain instance;

	public static CubicTerrain GetInstance()
	{
		return instance;
	}
	#endregion

	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Start()
	{
		// Singleton
		if (instance != null)
		{
			Debug.LogError ("2 CubicTerrain Script GameObject detected! Error! Disabling this instance.");
			this.enabled = false;
			return;
		}
		instance = this;

		// Terrain stream?
		if (this.serializeTerrain)
			this.terrainFile = new CubicTerrainFile(this.chunkFilesPath+"table.clt", this.chunkFilesPath+"data.cfd");

		this.terrainGenerator = this.GetComponent<ATerrainGenerator> ();
		this.chunkGenerationThread = new Thread (this.ChunkGenerationThread);
		this.chunkGenerationThread.Start ();

		// Initialize dictionaries
		this.chunkObjects = new Dictionary<ChunkTuple, GameObject> ();
		this.chunkData = new Dictionary<ChunkTuple, CubicTerrainData> ();
		this.generationJobs = new Dictionary<ChunkTuple, ChunkGenerationJob> ();

		// Init
		this.terrainMaterial.SetTexture ("_MainTex", Blocks.textureAtlas);
		this.transparentTerrainMaterial.SetTexture ("_MainTex", Blocks.textureAtlas);

		if (!this.loadPlayerChunkFirst)
			return;

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
				// TODO Do correct iteration!
				if (Vector3.Distance(this.GetChunkPosition(this.playerTransform.position), new Vector3(x,0,z)) < this.chunkPreloadRadius &&
				    ! this.chunkObjects.ContainsKey(new ChunkTuple(x,z)))
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

		CubicTerrainChunk terrainChunk = chunkObject.AddComponent<CubicTerrainChunk> ();
		this.chunkObjects.Add (new ChunkTuple (x, z), chunkObject);
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
						// 
						if (this.terrainFile != null && this.terrainFile.HasChunk(job.Key.x, job.Key.z))
						{
							job.Value.terrainChunkData = this.terrainFile.GetChunkData(job.Key.x, job.Key.z, this.chunkWidth, this.chunkHeight, this.chunkDepth);
						}
						else
						{
							this.terrainGenerator.GenerateTerrainData(job.Value.terrainChunkData, job.Value.worldspace);
						}
						this.chunkData.Add (job.Key, job.Value.terrainChunkData);
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
			if (Vector3.Distance(chunkVector, this.GetChunkPosition(this.playerTransform.position)) > this.chunkPreloadRadius)
			{
				chunksToDelete.Add (chunk.Key);
			}
		}
		
		// Delete chunks
		foreach (ChunkTuple t in chunksToDelete)
		{
			Destroy (this.chunkObjects[t]);
			this.chunkObjects.Remove (t);
			this.chunkData.Remove (t);
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
	
	/// <summary>
	/// Gets the block at position x|y|z.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public CubicTerrainData.VoxelData GetBlock(int x, int y, int z)
	{
		// Calculate chunk position for calculating relative position
		int chunkX = Mathf.FloorToInt (x / this.chunkWidth);
		int chunkZ = Mathf.FloorToInt (z / this.chunkDepth);
		
		// Calculate relative position
		x -= chunkX * this.chunkWidth;
		z -= chunkZ * this.chunkDepth;
		
		return this.chunkData[new ChunkTuple(chunkX, chunkZ)].GetVoxel(x,y,z);
	}
	
	/// <summary>
	/// Sets the block id at position x|y|z.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public void SetBlock(int x, int y, int z, short blockId)
	{
		// Calculate chunk position for calculating relative position
		int chunkX = Mathf.FloorToInt (x / this.chunkWidth);
		int chunkZ = Mathf.FloorToInt (z / this.chunkDepth);
		
		// Calculate relative position
		x -= chunkX * this.chunkWidth;
		z -= chunkZ * this.chunkDepth;
		
		this.chunkData[new ChunkTuple(chunkX, chunkZ)].SetVoxel(x,y,z,blockId);
	}

	/// <summary>
	/// Determines whether this instance has block the specified x y z.
	/// Returns also false if the blockid at the given position is less than 0 (which means no block, air)
	/// </summary>
	/// <returns><c>true</c> if this instance has block the specified x y z; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public bool HasBlock(int x, int y, int z)
	{
		// Calculate chunk position for calculating relative position
		int chunkX = Mathf.FloorToInt (x / this.chunkWidth);
		int chunkZ = Mathf.FloorToInt (z / this.chunkDepth);
		
		// Calculate relative position
		x -= chunkX * this.chunkWidth;
		z -= chunkZ * this.chunkDepth;
		
		return this.chunkData[new ChunkTuple(chunkX, chunkZ)].HasVoxel(x,y,z);
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	public void OnDestroy()
	{
		if (this.terrainFile != null)
			this.terrainFile.Close ();
	}
	
	/// <summary>
	/// Gets the center position of the block at the given position in worldspace.
	/// </summary>
	/// <returns>The absolute center position.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public Vector3 GetWorldspaceCenterPosition(int x, int y, int z)
	{
		return new Vector3
		(
			x + 0.5f,
			y + 0.5f,
			z + 0.5f
		);
	}

}


// Chunk tuple implementation
// Used to identify chunks in a Dictionary
public class ChunkTuple : System.Object
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
