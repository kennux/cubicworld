using UnityEngine;
using System.Collections;

public class FlatGenerator : ATerrainGenerator
{
	protected override void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace)
	{
		for (int x = 0; x < terrainDataObject.width; x++)
		{
			for (int z = 0; z < terrainDataObject.depth; z++)
			{
				for (int y = 0; y < 3; y++)
				{
					terrainDataObject.SetVoxel(x,y,z,1);
				}
			}
		}
	}
}
