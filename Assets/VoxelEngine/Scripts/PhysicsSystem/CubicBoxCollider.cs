using UnityEngine;
using System.Collections;

public class CubicBoxCollider : ACubicTerrainCollider
{
	public Vector3 size;

	public override bool CheckCollision (ACubicTerrainCollider otherCollider, ref CubicTerrainHitInfo hitInfo)
	{
		// Box collisions -> Box collisions
		if (otherCollider.GetType().IsSubclassOf(typeof(CubicBoxCollider)))
		{
			// Cast collider
			CubicBoxCollider boxCollider = (CubicBoxCollider)otherCollider;
		}
		return false;
	}
}
