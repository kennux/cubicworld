using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Cubic terrain data class.
/// 
/// This class is thread-safe!
/// </summary>
public class CubicTerrainData
{
	// Voxel data struct
	public class VoxelData
	{
		public short blockId;
		public bool transparent
		{
			get { return Blocks.GetBlock (this.blockId).transparentBlock; }
		}

		public byte rotation = 0;

		public VoxelData(short blockId)
		{
			this.blockId = blockId;
		}
	}

	/// <summary>
	/// The voxel data in an 3d-array (xyz)
	/// </summary>
	private VoxelData[][][] _voxelData;

	/// <summary>
	/// Gets the voxel data.
	/// </summary>
	/// <value>The voxel data.</value>
	public VoxelData[][][] voxelData
	{
		get { return this._voxelData; }
	}

	/// <summary>
	/// The voxel data lock object.
	/// </summary>
	private object voxelDataLockObject = new object();

	/// <summary>
	/// If this is set on true on the next update the mesh will get resynced to the voxel data.
	/// </summary>
	private bool _isDirty;

	public bool isDirty
	{
		get { return this._isDirty; }
		set { this._isDirty = value; }
	}

	// Dimensions
	private int _width;
	private int _height;
	private int _depth;

	/// <summary>
	/// Gets the terrain width.
	/// </summary>
	/// <value>The width.</value>
	public int width
	{
		get { return this._width; }
	}

	/// <summary>
	/// Gets the terrain height.
	/// </summary>
	/// <value>The height.</value>
	public int height
	{
		get { return this._height; }
	}

	/// <summary>
	/// Gets the terrain depth.
	/// </summary>
	/// <value>The depth.</value>
	public int depth
	{
		get { return this._depth; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CubicTerrainData"/> class.
	/// </summary>
	/// <param name="width">Width.</param>
	/// <param name="height">Height.</param>
	/// <param name="depth">Depth.</param>
	public CubicTerrainData(int width, int height, int depth)
	{
		// Save dimensions
		this._width = width;
		this._height = height;
		this._depth = depth;

		// Initialize voxel data array
		this._voxelData = new VoxelData[width][][];
		for (int i = 0; i < width; i++)
		{
			this._voxelData[i] = new VoxelData[height][];
			for (int j = 0; j < height; j++)
			{
				this._voxelData[i][j] = new VoxelData[depth];
			}
		}
	}


	/// <summary>
	/// Sets the voxel at x|y|z.
	/// 
	/// -1 as block id means no block.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="blockId">Block identifier. -1 means no block</param>
	public void SetVoxel(int x, int y, int z, short blockId)
	{
		lock (this.voxelDataLockObject)
		{
			// Write voxel data
			this._voxelData [x] [y] [z] = new VoxelData (blockId);
			this._isDirty = true;
		}
	}

	/// <summary>
	/// Gets the voxel data for the given coordinates.
	/// Returns null or voxeldata with blockid less than 0 for empty space.
	/// </summary>
	/// <returns>The voxel.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public VoxelData GetVoxel (int x, int y, int z)
	{
		if (x < 0 || x >= this.width || y < 0 || y >= this.height || z < 0 || z >= this.depth)
			return null;

		lock (this.voxelDataLockObject)
		{
			return this.voxelData[x][y][z];
		}
	}
	
	/// <summary>
	/// Determines whether this instance has voxel at the specified x y z.
	/// Returns also false if the voxel's block id is less than 0 (which means no block)
	/// </summary>
	/// <returns><c>true</c> if this instance has voxel at the specified x y z; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public bool HasVoxel(int x, int y, int z)
	{
		lock (this.voxelDataLockObject)
		{
			return this.voxelData [x] [y] [z] != null && this.voxelData [x] [y] [z].blockId >= 0;
		}
	}

	/// <summary>
	/// Returns the estimated size in bytes per chunk.
	/// </summary>
	/// <returns>The chunk bytes.</returns>
	/// <param name="width">Width.</param>
	/// <param name="height">Height.</param>
	/// <param name="depth">Depth.</param>
	public static int EstimatedChunkBytes(int width, int height, int depth)
	{
		return width * height * depth * 3; // * 2 for 1 short (block id)
	}

	/// <summary>
	/// Serializes the chunk data to the given stream.
	/// </summary>
	/// <param name="stream">Stream.</param>
	public void SerializeChunk(BufferedStream stream)
	{
		byte[] chunkData = new byte[EstimatedChunkBytes(this.width, this.height, this.depth)];

		lock (this.voxelDataLockObject)
		{
			// Write chunk data
			int counter = 0;
			for (int x = 0; x < this.width; x++)
			{   
				for (int y = 0; y < this.height; y++)
				{
					for (int z = 0; z < this.depth; z++)
					{
						short blockId = -1;
						if (this.voxelData[x][y][z] != null)
							blockId = this.voxelData[x][y][z].blockId;

						byte[] d = System.BitConverter.GetBytes(blockId);
						chunkData[counter]=d[0];
						chunkData[counter+1]=d[1];
						chunkData[counter+2]=d[1];
						counter += 3;
					}
				}
			}
		}

		// Write data
		stream.Write (chunkData, 0, chunkData.Length);
		stream.Flush ();
	}

	/// <summary>
	/// Deserializes the chunk.
	/// </summary>
	/// <param name="stream">Stream.</param>
	public void DeserializeChunk(BufferedStream stream)
	{
		lock (this.voxelDataLockObject)
		{
			// Read chunk data
			byte[] chunkData = new byte[EstimatedChunkBytes(this.width, this.height, this.depth)];
			stream.Read (chunkData, 0, EstimatedChunkBytes (this.width, this.height, this.depth));

			int counter = 0;

			// Parse chunk data
			for (int x = 0; x < this.width; x++)
			{   
				for (int y = 0; y < this.height; y++)
				{
					for (int z = 0; z < this.depth; z++)
					{
						short blockId = System.BitConverter.ToInt16(chunkData, counter);
						byte rotation = chunkData[counter+2];
						this.voxelData[x][y][z] = new VoxelData(blockId);
						this.voxelData[x][y][z].rotation = rotation;
						counter += 3;
					}
				}
			}
		}
	}

	/// <summary>
	/// Call this after data in voxelData was changed by the GetVoxel() function or direct access.
	/// </summary>
	public void DataUpdated()
	{
		this._isDirty = true;
	}

	/// <summary>
	/// Locks the voxel data.
	/// You MUST use this function if you are doing direct xxx.voxelData[x][y][z] to ensure thread-safety.
	/// </summary>
	public void LockData()
	{
		System.Threading.Monitor.Enter (this.voxelDataLockObject);
	}

	/// <summary>
	/// Unlocks the voxel data.
	/// You MUST use this function if you are doing direct xxx.voxelData[x][y][z] to ensure thread-safety.
	/// </summary>
	public void UnlockData()
	{
		System.Threading.Monitor.Exit (this.voxelDataLockObject);
	}
}


public enum BlockFace
{
	LEFT = 0,
	RIGHT = 1,
	TOP = 2,
	BOTTOM = 3,
	FRONT = 4,
	BACK = 5
}