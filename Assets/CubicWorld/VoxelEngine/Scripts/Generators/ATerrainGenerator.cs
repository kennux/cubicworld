using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract terrain generator class.
/// This class needs to get attached to a CubicTerrain Object!
/// It will get used for terrain generation.
/// </summary>
public abstract class ATerrainGenerator : MonoBehaviour
{
	public void GenerateChunk(CubicTerrainData terrainDataObject, Vector3 worldspace)
	{
		/*System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch ();
		stopWatch.Start ();*/
		this.GenerateTerrainData (terrainDataObject, worldspace);
		/*stopWatch.Stop ();
		Debug.Log ("Generating chunk at " + worldspace + " took " + stopWatch.Elapsed.TotalMilliseconds + "ms");*/
	}

	/// <summary>
	/// Generates the terrain data.
	/// Generate your world inside here!
    /// 
    /// IMPORTANT: Chunks consist of multiple smaller chunks on the y-axis, so this function will get called for every y-axis chunk with the same x and y coordiantes!
	/// </summary>
	/// <param name="terrainDataObject">Terrain data object.</param>
	protected abstract void GenerateTerrainData(CubicTerrainData terrainDataObject, Vector3 worldspace);
}
