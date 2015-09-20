using UnityEngine;
using System.Collections;

public class PerlinHeightGenerator : ATerrainGenerator
{
	public float frequency = 0.05f;

	// Block Ids
	private const int grassId = 1;
	private const int dirtId = 2;
	private const int stoneId = 3;
	
	protected override void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace)
	{
		for (int x = 0; x < terrainDataObject.width; x++)
		{
			for (int z = 0; z < terrainDataObject.depth; z++)
			{
				// Get absolute positions for noise generation
				float absX = (float) x + worldspace.x;
				float absZ = (float) z + worldspace.z;
				
				float perlin = Mathf.PerlinNoise(absX * frequency, absZ * frequency);
				int toY = (int)(perlin * ((float)(terrainDataObject.owner.chunkHeight * terrainDataObject.owner.chunksOnYAxis)-1));
				toY -= (int)terrainDataObject.owner.GetChunkPosition(worldspace).y * terrainDataObject.owner.chunkHeight;
				toY = Mathf.Min(toY, terrainDataObject.height);

				for (int y = 0; y < terrainDataObject.height; y++)
				{
					if (y > toY)
					{
						continue;
					}
					if (y < toY - 3)
					{
						terrainDataObject.SetVoxel(x,y,z,stoneId);
					}
					else if (y == toY)
					{
						terrainDataObject.SetVoxel(x,y,z,grassId);
					}
					else if (y > toY - 1)
					{
						terrainDataObject.SetVoxel(x,y,z,dirtId);
					}
					else if (y > toY - 3)
					{
						terrainDataObject.SetVoxel(x,y,z,dirtId);
					}
				}
			}
		}
	}
}
