using UnityEngine;
using System.Collections;

/// <summary>
/// Flat world generator.
/// </summary>
public class FlatGenerator : ATerrainGenerator
{
	// Block Ids
	private const int grassId = 1;
	private const int dirtId = 2;
	private const int stoneId = 3;
	private const int bedrockId = 5;
	private const int treeId = 6;
	private const int leavesId = 7;

	private System.Random rand;
	
	protected override void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace)
	{
		for (int x = 0; x < terrainDataObject.width; x++)
		{
			for (int y = 0; y < terrainDataObject.height; y++)
			{
				for (int z = 0; z < terrainDataObject.depth; z++)
				{
					// Get absolute positions for noise generation
					float absX = (float) x + worldspace.x;
					float absZ = (float) z + worldspace.z;
					float absY = (float) y + worldspace.y;

					if (absY < 4)
						terrainDataObject.SetVoxel(x,y,z,2);
				}
			}
		}
	}
}
