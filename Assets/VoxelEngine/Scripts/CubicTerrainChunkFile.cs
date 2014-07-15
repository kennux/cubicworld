using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Cubic terrain chunk file implementation.
/// 
/// This class is fully thread-safe.
/// </summary>
public class CubicTerrainChunkFile
{
	/// <summary>
	/// The chunk lookup table file path.
	/// </summary>
	private string chunkLookupTableFile;

	/// <summary>
	/// The chunk data file path.
	/// </summary>
	private string chunkDataFile;

	/// <summary>
	/// The chunk lookup table.
	/// </summary>
	private Dictionary<ChunkTuple, long> chunkLookupTable;

	private object chunkDataLockObject = new object ();
	private object chunkLookupLockObject = new object ();

	/// <summary>
	/// Initializes a new instance of the <see cref="CubicTerrainChunkFile"/> class.
	/// </summary>
	/// <param name="chunkLookupTableFile">Chunk lookup table file.</param>
	/// <param name="chunkDataFile">Chunk data file.</param>
	public CubicTerrainChunkFile(string chunkLookupTableFile, string chunkDataFile)
	{
		this.chunkLookupTableFile = chunkLookupTableFile;
		this.chunkDataFile = chunkDataFile;

		// Create files
		if (!File.Exists (this.chunkDataFile))
			File.Create (this.chunkDataFile).Close();

		if (!File.Exists (this.chunkLookupTableFile))
			File.Create (this.chunkLookupTableFile).Close ();

		this.chunkLookupTable = new Dictionary<ChunkTuple, long> ();
		this.LoadLookupTable ();
	}

	/// <summary>
	/// Loads the lookup table.
	/// </summary>
	private void LoadLookupTable()
	{
		lock (this.chunkLookupLockObject)
		{
			// Read all lookup entries
			byte[] lookupEntries = File.ReadAllBytes (this.chunkLookupTableFile);

			// 16 Bytes (int - x, int - y, long data position)
			for (int i = 0; i < lookupEntries.Length; i += 16)
			{
				int x = BitConverter.ToInt32(lookupEntries, i);
				int z = BitConverter.ToInt32(lookupEntries, i+4);
				long position = BitConverter.ToInt64(lookupEntries, i+8);

				this.chunkLookupTable.Add(new ChunkTuple(x,z), position);
			}

			int j = 0;
		}
	}

	/// <summary>
	/// Writes the lookup table.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="position">Position.</param>
	private void WriteLookupTable(int x, int z, long position)
	{
		lock (this.chunkLookupLockObject)
		{
			// Write bytes
			byte[] bytesX = BitConverter.GetBytes(x);
			byte[] bytesZ = BitConverter.GetBytes(z);
			byte[] bytesPosition = BitConverter.GetBytes(position);

			// Concatenate all byte arrays
			byte[] bytes = new byte[bytesX.Length+bytesZ.Length+bytesPosition.Length];
			Array.Copy (bytesX, 0, bytes, 0, 4);
			Array.Copy (bytesZ, 0, bytes, 4, 4);
			Array.Copy (bytesPosition, 0, bytes, 8, 8);

			using (FileStream stream = new FileStream(this.chunkLookupTableFile, FileMode.Append))
			{
				stream.Write(bytes, 0, bytes.Length);
				stream.Flush();
				stream.Close();
			}
		}
	}

	/// <summary>
	/// Determines whether this instance has chunk the specified x z.
	/// </summary>
	/// <returns><c>true</c> if this instance has chunk the specified x z; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public bool HasChunk(int x, int z)
	{
		return this.chunkLookupTable.ContainsKey(new ChunkTuple(x,z));
	}

	/// <summary>
	/// Gets the chunk data.
	/// </summary>
	/// <returns>The chunk data.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="width">Width.</param>
	/// <param name="height">Height.</param>
	/// <param name="depth">Depth.</param>
	public CubicTerrainData GetChunkData(int x, int z, int width, int height, int depth)
	{
		BufferedStream chunkDataStream = new BufferedStream (File.Open (this.chunkDataFile, FileMode.Open));
		CubicTerrainData terrainData = new CubicTerrainData (width, height, depth);

		// Get chunk starting position
		chunkDataStream.Position = this.chunkLookupTable [new ChunkTuple (x, z)];

		terrainData.DeserializeChunk (chunkDataStream);
		chunkDataStream.Close ();

		return terrainData;
	}

	/// <summary>
	/// Sets the chunk data.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="terrainData">Terrain data.</param>
	public void SetChunkData(int x, int z, CubicTerrainData terrainData)
	{
		lock (this.chunkDataLockObject)
		{
			BufferedStream chunkDataStream = new BufferedStream (File.Open (this.chunkDataFile, FileMode.Open));
			ChunkTuple tuple = new ChunkTuple (x, z);

			long position = chunkDataStream.Length;
			if (this.chunkLookupTable.ContainsKey(tuple))
			{
				// Key already available
				position = this.chunkLookupTable[tuple];
			}
			else
			{
				// Key not available
				// Update lookup table
				this.chunkLookupTable.Add (tuple, chunkDataStream.Length);
				this.WriteLookupTable (x,z, chunkDataStream.Length);
			}

			// Write chunk data
			chunkDataStream.Position = position;
			terrainData.SerializeChunk (chunkDataStream);

			chunkDataStream.Flush ();
			chunkDataStream.Close ();
		}
	}

	/// <summary>
	/// Closes this instance.
	/// </summary>
	public void Close()
	{
	}
}
