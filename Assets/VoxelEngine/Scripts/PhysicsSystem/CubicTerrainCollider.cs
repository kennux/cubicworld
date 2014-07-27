using UnityEngine;
using System.Collections;

/// <summary>
/// Cubic terrain collider.
/// </summary>
public class CubicTerrainCollider : ACubicTerrainCollider
{
	/// <summary>
	/// The terrain chunk reference.
	/// </summary>
	public CubicTerrainChunk terrainChunk;

	public override bool CheckCollision (ACubicTerrainCollider otherCollider, ref CubicTerrainHitInfo hitInfo)
	{

		return false;
	}
}
