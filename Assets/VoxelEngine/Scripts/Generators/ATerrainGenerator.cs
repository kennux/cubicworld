using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract terrain generator class.
/// This class needs to get attached to a CubicTerrain Object!
/// It will get used for terrain generation.
/// </summary>
public abstract class ATerrainGenerator : MonoBehaviour
{
	/// <summary>
	/// Generates the terrain data.
	/// Generate your world inside here!
	/// </summary>
	/// <param name="terrainDataObject">Terrain data object.</param>
	public abstract void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace);
}
