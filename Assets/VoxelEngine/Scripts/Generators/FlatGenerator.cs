using UnityEngine;
using System.Collections;

public class FlatGenerator : ATerrainGenerator
{
	public override void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace)
	{
		for (int x = 0; x < terrainDataObject.width; x++)
		{
			for (int z = 0; z < terrainDataObject.depth; z++)
			{
				terrainDataObject.SetVoxel(x,0,z,1);
			}
		}
	}
}
