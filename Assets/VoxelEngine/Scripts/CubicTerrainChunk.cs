using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Linq;

/// <summary>
/// Block hit info structure. Gets built from raycast hits in the .
/// </summary>
public struct BlockHitInfo
{
	/// <summary>
	/// The block that was hit.
	/// </summary>
	public Vector3 hitBlock;
	
	/// <summary>
	/// The face that was hit.
	/// </summary>
	public BlockFace hitFace;
}

/// <summary>
/// Cubic terrain chunk.
/// </summary>
public class CubicTerrainChunk : MonoBehaviour
{
	#region Static data
	private static Vector3[] leftSideVertices = new Vector3[]
	{
		new Vector3(0,0,1),
		new Vector3(0,0,0),
		new Vector3(0,1,0),
		new Vector3(0,1,1)
	};
	
	private static int[] leftSideIndices = new int[]
	{
		1,0,2,0,3,2
	};
	
	private static Vector3[] rightSideVertices = new Vector3[]
	{
		new Vector3(1,0,0),
		new Vector3(1,0,1),
		new Vector3(1,1,1),
		new Vector3(1,1,0),
	};
	
	private static int[] rightSideIndices = new int[]
	{
		1,0,2,0,3,2
	};
	
	private static Vector3[] topSideVertices = new Vector3[]
	{
		new Vector3(0,1,0),
		new Vector3(1,1,0),
		new Vector3(1,1,1),
		new Vector3(0,1,1),
	};
	
	private static int[] topSideIndices = new int[]
	{
		1,0,2,0,3,2
	};
	
	private static Vector3[] bottomSideVertices = new Vector3[]
	{
		new Vector3(0,0,0),
		new Vector3(1,0,0),
		new Vector3(1,0,1),
		new Vector3(0,0,1)
	};
	
	private static int[] bottomSideIndices = new int[]
	{
		1,2,0,2,3,0
	};
	
	private static Vector3[] backSideVertices = new Vector3[]
	{
		new Vector3(0,0,0),
		new Vector3(1,0,0),
		new Vector3(1,1,0),
		new Vector3(0,1,0)
	};
	
	private static int[] backSideIndices = new int[]
	{
		2,1,0,0,3,2
	};
	
	private static Vector3[] frontSideVertices = new Vector3[]
	{
		new Vector3(1,0,1),
		new Vector3(0,0,1),
		new Vector3(0,1,1),
		new Vector3(1,1,1)
	};
	
	private static int[] frontSideIndices = new int[]
	{
		2,1,0,0,3,2
	};

	/* Rotation face mappings
	*  Indices:
	*  0 - LEFT
	*  1 - RIGHT
	*  2 - TOP
	*  3 - BOTTOM
	*  4 - BACK
	*  5 - FRONT
	* 
	*  Sides mapped from initial rotation.
	*/
	private static BlockFace[][] rotationMappings = new BlockFace[][]
	{
		// Facing front
		new BlockFace[]
		{
			BlockFace.LEFT, BlockFace.RIGHT, BlockFace.TOP, BlockFace.BOTTOM, BlockFace.BACK, BlockFace.FRONT
		},
		// Facing right
		new BlockFace[]
		{
			BlockFace.FRONT, BlockFace.BACK, BlockFace.TOP, BlockFace.BOTTOM, BlockFace.LEFT, BlockFace.RIGHT
		},
		// Facing back
		new BlockFace[]
		{
			BlockFace.RIGHT, BlockFace.LEFT, BlockFace.TOP, BlockFace.BOTTOM, BlockFace.FRONT, BlockFace.BACK
		},
		// Facing left
		new BlockFace[]
		{
			BlockFace.BACK, BlockFace.FRONT, BlockFace.TOP, BlockFace.BOTTOM, BlockFace.RIGHT, BlockFace.LEFT
		}
	};
	
	#endregion

	#region Helper classes and functions

	// Mesh data class.
	// Used for sending mesh data from generation thread to the main thread.
	class MeshData
	{
		public Vector3[] vertices;
		public int[] triangles;
		public int[] transparentTriangles;
		public Vector2[] uvs;
	}

	// Triangle info.
	// Will identify triangle indices with
	public class TriangleBlockInfo
	{
		public int x, y, z;
		public BlockFace face;

		public TriangleBlockInfo(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}
	#endregion

	/// <summary>
	/// Gets or sets the chunk data.
	/// </summary>
	/// <value>The chunk data.</value>
	public CubicTerrainData chunkData
	{
		get { return this._chunkData; }
		set { this._chunkData = value; this._isDirty = true; }
	}

	public CubicTerrain master;

	/// <summary>
	/// The frame where an update occured	
	/// </summary>
	private static float lastUpdateFrame;

	/// <summary>
	/// The chunk data.
	/// </summary>
	private CubicTerrainData _chunkData;

	/// <summary>
	/// If this flag is set to true the chunk data will get rebuilt.
	/// </summary>
	private bool _isDirty;

	public bool isDirty
	{
		get { return (this._isDirty || (this.chunkData != null && this.chunkData.isDirty)); }
		set { this._isDirty = value; if (this.chunkData != null) { this.chunkData.isDirty = value; } }
	}

	/// <summary>
	/// The renderer.
	/// </summary>
	private Mesh[] meshes;

	public Vector3 chunkPosition;

	/// <summary>
	/// The new mesh.
	/// Generated from another thread. asnychronously!
	/// </summary>
	private MeshData[] newMeshData;

	private object meshDataLockObject = new object();

	private Thread meshGenerationThread;

	/// <summary>
	/// Updates the chunk.
	/// </summary>
	public void FixedUpdate()
	{
		if (this.isDirty)
		{
			this.meshGenerationThread = new Thread(this.GenerateMesh);
			this.meshGenerationThread.Start ();
			
			this.isDirty = false;
		}
		
		// Lag protection
        if (this.master != null && (!this.master.smoothChunkLoading || lastUpdateFrame < Time.frameCount - 5))
		{
			lock (this.meshDataLockObject)
			{
				if (this.newMeshData != null)
				{
					GameObject[] existingObjects = new GameObject[this.transform.childCount];
					int j = 0;

					// Reuse childs
					foreach (Transform t in this.transform)
					{
						// Destroy (t.gameObject);
						existingObjects[j]=t.gameObject;
						j++;
					}

					for (int i = 0; i < this.newMeshData.Length; i++)
					{
						MeshData meshData = this.newMeshData[i];
						// Generate new mesh object from raw data.
						Mesh newMesh = new Mesh();

						newMesh.vertices = meshData.vertices; // this.newMeshData.vertices;
						// newMesh.colors = this.newMeshData.colors;
						newMesh.uv = meshData.uvs; // this.newMeshData.uvs;
						// newMesh.triangles = meshData.triangles; // this.newMeshData.triangles;
						newMesh.subMeshCount = 2;
						newMesh.SetTriangles(meshData.triangles, 0);
						newMesh.SetTriangles(meshData.transparentTriangles, 1);
						newMesh.RecalculateBounds ();
						newMesh.RecalculateNormals ();
						newMesh.Optimize();

						GameObject meshObject = null;
						bool newObject = false;

						// Add new mesh object if there is none
						if (existingObjects.Length > i)
						{
							meshObject=existingObjects[i];
							existingObjects[i]=null;
						}
						else
						{
							meshObject = new GameObject();
							meshObject.transform.parent = this.transform;
							meshObject.transform.position=this.transform.position;
							meshObject.layer=this.gameObject.layer;
							meshObject.transform.name="Mesh_"+i;
							newObject=true;
						}

						MeshFilter filter = null;
						MeshRenderer renderer = null;
						MeshCollider collider = null;

						if (newObject)
						{
							filter = meshObject.AddComponent<MeshFilter>();
							renderer = meshObject.AddComponent<MeshRenderer>();
							collider = meshObject.AddComponent<MeshCollider>();
						}
						else
						{
							filter = meshObject.GetComponent<MeshFilter>();
							renderer = meshObject.GetComponent<MeshRenderer>();
							collider = meshObject.GetComponent<MeshCollider>();
						}

						filter.sharedMesh = newMesh;
						if (this.master.useMeshColliders)
							collider.sharedMesh = newMesh;
						renderer.materials = new Material[] { this.master.terrainMaterial, this.master.transparentTerrainMaterial };
                    }
                    
                    // Cleanup
					foreach (GameObject g in existingObjects)
					{
						if (g != null)
							Destroy (g);
					}
					this.newMeshData = null;

					// Kill thread if alive
					if (this.meshGenerationThread != null)
					{
						this.meshGenerationThread.Abort();
						this.meshGenerationThread = null;
					}
					
					lastUpdateFrame = Time.frameCount;
				}
			}
		}
	}

	/// <summary>
	/// Generates the mesh from _chunkData.
	/// </summary>
	/// <returns>The mesh.</returns>
	private void GenerateMesh()
	{
		this._chunkData.LockData();
		CubicTerrainData.VoxelData[][][] voxelData = this._chunkData.voxelData;
		int indicesCounter = 0;
		int transparentIndicesCounter = 0;
		
		List<Vector3> vertices = new List<Vector3> ();
		List<int> indices = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		List<Color> colors = new List<Color> ();

		// Determine block visibilities
		for (int x = 0; x < this._chunkData.width; x++)
		{
			for (int y = 0; y < this._chunkData.height; y++)
			{
				for (int z = 0; z < this._chunkData.depth; z++)
				{
					// Voxel in my position?
					if (voxelData[x][y][z] == null || voxelData[x][y][z].blockId < 0)
						continue;

					BlockFace[] faceMappings = rotationMappings[voxelData[x][y][z].rotation];

					// Left side un-covered?
					if (x == 0 || (voxelData[x-1][y][z] == null || voxelData[x-1][y][z].blockId < 0 || voxelData[x-1][y][z].transparent))
                    {
                        // Un-Covered! Add mesh data!
						WriteSideData(vertices, indices, uvs, colors, leftSideVertices, leftSideIndices, indicesCounter, transparentIndicesCounter,x,y,z, Color.blue, voxelData[x][y][z].blockId, faceMappings[0]);
						if (voxelData[x][y][z].transparent)
							transparentIndicesCounter+=leftSideVertices.Length;
						indicesCounter += leftSideVertices.Length;
					}
					// Right side un-covered?
					if (x == this._chunkData.width -1 || ((voxelData[x+1][y][z] == null || voxelData[x+1][y][z].blockId < 0 || voxelData[x+1][y][z].transparent)))
					{
						// Un-Covered!
						WriteSideData(vertices, indices, uvs, colors, rightSideVertices, rightSideIndices, indicesCounter, transparentIndicesCounter,x,y,z, Color.black, voxelData[x][y][z].blockId, faceMappings[1]);
						if (voxelData[x][y][z].transparent)
							transparentIndicesCounter+=rightSideVertices.Length;
						indicesCounter += rightSideVertices.Length;
					}
					// Top side un-covered?
					if (y == this._chunkData.height-1 || ((voxelData[x][y+1][z] == null || voxelData[x][y+1][z].blockId < 0 || voxelData[x][y+1][z].transparent)))
					{
						// Un-Covered!
						WriteSideData(vertices, indices, uvs, colors, topSideVertices, topSideIndices, indicesCounter, transparentIndicesCounter,x,y,z, Color.gray, voxelData[x][y][z].blockId, faceMappings[2]);
						if (voxelData[x][y][z].transparent)
							transparentIndicesCounter+=topSideVertices.Length;
                       	indicesCounter += topSideVertices.Length;
					}
					// Bottom side un-covered?
					if (y == 0 || (voxelData[x][y-1][z] == null || voxelData[x][y-1][z].blockId < 0 || voxelData[x][y-1][z].transparent))
					{
						// Un-Covered!
						WriteSideData(vertices, indices, uvs, colors, bottomSideVertices, bottomSideIndices, indicesCounter, transparentIndicesCounter,x,y,z, Color.green, voxelData[x][y][z].blockId, faceMappings[3]);
						if (voxelData[x][y][z].transparent)
							transparentIndicesCounter+=bottomSideVertices.Length;
						indicesCounter += bottomSideVertices.Length;
					}
					// Back side un-covered?
					if (z == 0 || (voxelData[x][y][z-1] == null || voxelData[x][y][z-1].blockId < 0 || voxelData[x][y][z-1].transparent))
					{
						// Un-Covered!
						WriteSideData(vertices, indices, uvs, colors, backSideVertices, backSideIndices, indicesCounter, transparentIndicesCounter,x,y,z, Color.yellow, voxelData[x][y][z].blockId, faceMappings[4]);
						if (voxelData[x][y][z].transparent)
							transparentIndicesCounter+=backSideVertices.Length;
						indicesCounter += backSideVertices.Length;
                    }
                    // Front side un-covered?
					if (z == this._chunkData.depth-1 || ((voxelData[x][y][z+1] == null || voxelData[x][y][z+1].blockId < 0 || voxelData[x][y][z+1].transparent)))
					{
						// Un-Covered!
						WriteSideData(vertices, indices, uvs, colors, frontSideVertices, frontSideIndices, indicesCounter, transparentIndicesCounter,x,y,z, Color.red, voxelData[x][y][z].blockId, faceMappings[5]);
						if (voxelData[x][y][z].transparent)
							transparentIndicesCounter+=frontSideVertices.Length;
						indicesCounter += frontSideVertices.Length;
					}
				}
			}
		}

		this._chunkData.UnlockData();

		// Write mesh data update
		if (this.master.terrainFile != null)
			this.master.terrainFile.SetChunkData ((int)this.chunkPosition.x, (int)this.chunkPosition.z, this.chunkData);

		// Set mesh data
		lock (this.meshDataLockObject)
		{
			int verticeCount = vertices.Count;
			int verticesAlreadySelected = 0;
			int trianglesAlreadySelected = 0;
			List<MeshData> meshDataList = new List<MeshData>();
			while (verticeCount > 0)
			{
				MeshData meshData = new MeshData();
				// Get vertices
				int verticesToSelect = Mathf.Min (verticeCount, 65000);
				verticesToSelect = verticesToSelect - (verticesToSelect % 4);
				int trianglesToSelect = (verticesToSelect / 4) * 6;
				
				Vector3[] selectedVertices = vertices.Skip(verticesAlreadySelected).Take(verticesToSelect).ToArray();
				Vector2[] selectedUvs = uvs.Skip(verticesAlreadySelected).Take(verticesToSelect).ToArray();
				int[] selectedTriangles = indices.Skip(trianglesAlreadySelected).Take(trianglesToSelect).ToArray();

				List<int> transparentTriangles = new List<int>();
				List<int> nonTransparentTriangles = new List<int>();
				
				// Preprocess indices
				for(int i = 0; i < selectedTriangles.Length; i++)
				{
					if (selectedTriangles[i] < 0)
					{
						// Transparent triangle
						transparentTriangles.Add (Mathf.Abs(selectedTriangles[i])-verticesAlreadySelected);
					}
					else
					{
						// Non-transparent triangles
						nonTransparentTriangles.Add (selectedTriangles[i]-verticesAlreadySelected);
					}
				}
				
				trianglesAlreadySelected+=trianglesToSelect;
				verticesAlreadySelected+=verticesToSelect;

				meshData.vertices = selectedVertices;
				meshData.triangles = nonTransparentTriangles.ToArray();
				meshData.transparentTriangles = transparentTriangles.ToArray();
				meshData.uvs = selectedUvs;

				// this.newMeshData.colors = colors.ToArray ();
				meshDataList.Add(meshData);
				verticeCount-=verticesToSelect;
			}
			this.newMeshData = meshDataList.ToArray ();
		}
	}

	/// <summary>
	/// Writes the side data.
	/// </summary>
	/// <param name="vertices">Vertices.</param>
	/// <param name="indices">Indices.</param>
	/// <param name="uvs">Uvs.</param>
	/// <param name="colors">Colors.</param>
	/// <param name="sideVertices">Side vertices.</param>
	/// <param name="sideIndices">Side indices.</param>
	/// <param name="indicesCounter">Indices counter.</param>
	/// <param name="transparentIndicesCounter">Transparent indices counter.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="color">Color.</param>
	/// <param name="uv">Uv.</param>
	/// <param name="face">Face.</param>
	/// <param name="transparent">If set to <c>true</c> transparent.</param>
	private static void WriteSideData(List<Vector3> vertices, List<int> indices, List<Vector2> uvs, List<Color> colors, Vector3[] sideVertices, int[] sideIndices, int indicesCounter, int transparentIndicesCounter, int x, int y, int z, Color color, short blockId, BlockFace face)
	{
		Vector2[] uv = Blocks.GetBlock (blockId).GetUvsForFace (face);
		bool transparent = Blocks.GetBlock (blockId).transparentBlock;

		// Calculate absolute vertex index count.
		int[] absoluteIndices = new int[sideIndices.Length];
		for (int i = 0; i < sideIndices.Length; i++)
		{
			absoluteIndices[i] = indicesCounter+sideIndices[i];

			if (transparent)
				absoluteIndices[i]*=-1;
		}

		// Transform vertices based on the block's position.
		Vector3[] absoluteVertices = new Vector3[sideVertices.Length];
		for (int i = 0; i < sideVertices.Length; i++)
		{
			absoluteVertices[i] = sideVertices[i];
			absoluteVertices[i].x += (float) x;
			absoluteVertices[i].y += (float) y;
			absoluteVertices[i].z += (float) z;
			colors.Add (color);
		}

		// Add mesh data to the lists.
		vertices.AddRange (absoluteVertices);

		// Write indices
		indices.AddRange (absoluteIndices);

		uvs.AddRange (uv);
	}

	/// <summary>
	/// TODO: Find a correct name for this. lol.
	/// 
	/// Multiplicates the position of the hit by the raycasthit normal * direction.
	/// Use -0.5f as direction for detecting the block that was hit and 0.5f for detecting the block next to the block that was hit.
	/// </summary>
	/// <returns>The hitted block.</returns>
	/// <param name="raycastHit">Raycast hit.</param>
	/// <param name="direction">Direction.</param>
	public Vector3 GetBlockPosition(RaycastHit raycastHit, float direction)
	{
		Vector3 pos = raycastHit.point;
		pos += (raycastHit.normal * direction);

		pos -= this.transform.position;
		
		pos.x = Mathf.Floor (pos.x);
		pos.y = Mathf.Floor (pos.y);
		pos.z = Mathf.Floor (pos.z);

		return pos;
	}

	/// <summary>
	/// Gets the block hit info for the given raycast hit
	/// Block facingsare in "physical space", which means they are not affected by the rotation of the block.
	/// For getting the facing affected by rotation transform the facing by using the function <see cref="TransformFacing"/>TransformFacing()</see>.
	/// </summary>
	/// <returns>The block hit info.</returns>
	/// <param name="hitInfo">Hit info.</param>
	public BlockHitInfo GetBlockHitInfo(RaycastHit hitInfo)
	{
		// Get the hit towards block
		Vector3 towardsBlockHit = this.GetBlockPosition(hitInfo, 0.5f);

		// Get the hit block
		Vector3 hitBlock = this.GetBlockPosition(hitInfo, -0.5f);

		BlockFace f = new BlockFace();

		// Determine which face was clicked
		if (towardsBlockHit.x > hitBlock.x)
		{
			f = BlockFace.RIGHT;
		}
		else if (towardsBlockHit.x < hitBlock.x)
		{
			f = BlockFace.LEFT;
		}
		else if (towardsBlockHit.y > hitBlock.y)
		{
			f = BlockFace.TOP;
		}
		else if (towardsBlockHit.y < hitBlock.y)
		{
			f = BlockFace.BOTTOM;
		}
		else if (towardsBlockHit.z > hitBlock.z)
		{
			f = BlockFace.FRONT;
		}
		else if (towardsBlockHit.z < hitBlock.z)
		{
			f = BlockFace.BACK;
		}

		// Build the hitinfo
		BlockHitInfo blockHitInfo = new BlockHitInfo ();
		blockHitInfo.hitBlock = hitBlock;
		blockHitInfo.hitFace = f;

		return blockHitInfo;
	}

	/// <summary>
	/// Transforms the given facing of the block at the given position.
	/// </summary>
	/// <returns>The facing.</returns>
	public BlockFace TransformFacing(BlockFace facing, int x, int y, int z)
	{
		// Get the block's rotation and face mappings
		byte rotation = this._chunkData.GetVoxel (x, y, z).rotation;
		BlockFace [] faceMapping = rotationMappings [rotation];

		// Transform the facing accordingly to the block's rotation.
		switch (facing)
		{
			case BlockFace.LEFT: return faceMapping[0];
			case BlockFace.RIGHT: return faceMapping[1];
			case BlockFace.BACK: return faceMapping[4];
			case BlockFace.FRONT: return faceMapping[5];
		}

		return facing;
	}

	/// <summary>
	/// Gets the absolute position by passing the position of an object relative to this chunk's start.
	/// 
	/// All coordinates are in block-space!
	/// </summary>
	/// <returns>The absolute position.</returns>
	/// <param name="relativePosition">Relative position.</param>
	public Vector3 GetAbsolutePosition(Vector3 relativePosition)
	{
		return new Vector3
		(
			this.chunkPosition.x * this.master.chunkWidth + relativePosition.x,
			relativePosition.y,
			this.chunkPosition.z * this.master.chunkDepth + relativePosition.z
		);
	}

	/// <summary>
	/// Raises the destroy event.
	/// Frees memory for procedurally generated resources.
	/// </summary>
	public void OnDestroy()
	{
		// Free memory for all child meshes
		foreach (Transform t in this.transform)
		{
			Destroy (t.gameObject.GetComponent<MeshFilter> ().sharedMesh);
			MeshCollider c = t.gameObject.GetComponent<MeshCollider> ();

			if (c != null)
				Destroy (c.sharedMesh);
		}
	}
}