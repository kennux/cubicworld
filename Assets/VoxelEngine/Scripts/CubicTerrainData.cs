using UnityEngine;
using System.Collections;

public class CubicTerrainData
{
	// Voxel data struct
	public class VoxelData
	{
		public short blockId;

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
		// Write voxel data
		this._voxelData [x] [y] [z] = new VoxelData (blockId);
		this._isDirty = true;
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
		return this.voxelData[x][y][z];
	}


	/// <summary>
	/// Determines whether this instance has voxel at the specified x y z.
	/// </summary>
	/// <returns><c>true</c> if this instance has voxel at the specified x y z; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public bool HasVoxel(int x, int y, int z)
	{
		return this.voxelData [x] [y] [z] != null && this.voxelData [x] [y] [z].blockId >= 0;
	}
}
