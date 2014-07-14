using UnityEngine;
using System.Collections;

public class TestGenerator : ATerrainGenerator
{
	public float frequency2D = 0.05f;
	public float frequency3D = 0.05f;
	
	// Block Ids
	private const int grassId = 1;
	private const int dirtId = 2;
	private const int stoneId = 3;
	private const int bedrockId = 5;
	private const int treeId = 6;
	private const int leavesId = 7;

	private System.Random rand;
	
	public override void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace)
	{
		this.rand = new System.Random ();

		for (int x = 0; x < terrainDataObject.width; x++)
		{
			for (int z = 0; z < terrainDataObject.depth; z++)
			{
				// Get absolute positions for noise generation
				float absX = (float) x + worldspace.x;
				float absZ = (float) z + worldspace.z;
				
				float perlin = Mathf.PerlinNoise(absX * frequency2D, absZ * frequency2D);
				int toY = (int)(perlin * ((float)terrainDataObject.height-1));

				bool tree = false; // this.rand.NextDouble() > 0.99;
				for (int y = 0; y < toY; y++)
				{
					if (y < toY-4)
					{
						terrainDataObject.SetVoxel(x,y,z,stoneId);
					}
					else if (y < toY - 1)
					{
						terrainDataObject.SetVoxel(x,y,z,dirtId);
					}
					else
					{
						terrainDataObject.SetVoxel(x,toY-1,z,grassId);
					}

					// 3D-Noise
					float noise = Simplex.Generate(absX * this.frequency3D, y * this.frequency3D, absZ * this.frequency3D);

					if (noise < 0.5f && noise > 0.3f)
						terrainDataObject.SetVoxel(x,y,z,-1);
				}

				// Tree generation possible
				bool treeGenerationPossible = terrainDataObject.GetVoxel (x,toY-1,z).blockId == grassId &&
					x > 2 && x < terrainDataObject.width - 2 && 
					z > 2 && z < terrainDataObject.depth - 2;

				if (treeGenerationPossible && tree)
				{
					// Generate tree
					int treeHeight = this.rand.Next (5,6);

					if (toY + treeHeight < terrainDataObject.height)
					{
						for (int i = toY; i<toY+treeHeight; i++)
						{
							terrainDataObject.SetVoxel(x,i,z,treeId);
						}
						
						for (int lY = toY +treeHeight-1; lY < toY +treeHeight+3; lY++)
						{
							// Generate leaves
							for (int lX = x - 2; lX < x + 2; lX++)
							{
								for (int lZ = z - 2; lZ < z + 2; lZ++)
								{
									terrainDataObject.SetVoxel (lZ, lY, lZ, leavesId);
								}
							}
						}
					}
				}

				// Set bedrock
				terrainDataObject.SetVoxel(x,0,z,bedrockId);
			}
		}
	}
}
