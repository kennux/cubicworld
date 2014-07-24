using UnityEngine;
using System.Collections;

public class PerlinGenerator3D : ATerrainGenerator
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
			for (int y = 0; y < terrainDataObject.height; y++)
			{
				for (int z = 0; z < terrainDataObject.depth; z++)
				{
					// Get absolute positions for noise generation
					float absX = (float) x + worldspace.x;
					float absZ = (float) z + worldspace.z;
					float absY = (float) y;
					
					//float perlin = Mathf.PerlinNoise(absX * frequency, absZ * frequency);
					//int toY = (int)(perlin * ((float)terrainDataObject.height-1));
					float noise = Simplex.Generate(absX * this.frequency, absY * this.frequency, absZ * this.frequency);
					
					/*for (int y = 0; y < toY-3; y++)
					{
						terrainDataObject.SetVoxel(x,y,z,stoneId);
					}

					for (int y = toY-3; y < toY-1; y++)
					{
						terrainDataObject.SetVoxel(x,y,z,dirtId);
					}
					terrainDataObject.SetVoxel(x,toY-1,z,grassId);*/

					if (noise < 0.5f && noise > 0.25f)
						terrainDataObject.SetVoxel(x,y,z,2);
				}
			}
		}
	}
}
