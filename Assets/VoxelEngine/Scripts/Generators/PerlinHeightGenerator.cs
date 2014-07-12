using UnityEngine;
using System.Collections;
using SimplexNoise;

public class PerlinHeightGenerator : ATerrainGenerator
{
	public float frequency = 0.05f;

	public override void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace)
	{
		for (int x = 0; x < terrainDataObject.width; x++)
		{
			for (int z = 0; z < terrainDataObject.depth; z++)
			{
				// Get absolute positions for noise generation
				float absX = (float) x + worldspace.x;
				float absZ = (float) z + worldspace.z;

				float perlin = Mathf.PerlinNoise(absX * frequency, absZ * frequency);
				int toY = (int)(perlin * ((float)terrainDataObject.height-1));

				for (int y = 0; y < toY; y++)
				{
					terrainDataObject.SetVoxel(x,y,z,1);
				}
			}
		}
	}
}
