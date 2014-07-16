using UnityEngine;
using System.Collections;

public class CubicBoxCollider : ACubicTerrainCollider
{
	public Vector3 size;

	public override bool CheckCollision (ACubicTerrainCollider otherCollider, ref CubicTerrainHitInfo hitInfo)
	{
		return false;
	}
}
