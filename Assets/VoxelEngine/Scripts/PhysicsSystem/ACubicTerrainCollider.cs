using UnityEngine;
using System.Collections;

/// <summary>
/// Cubic terrain hit info.
/// PLACEHOLDER
/// </summary>
public class CubicTerrainHitInfo
{
	public ACubicTerrainCollider otherCollider;
}

/// <summary>
/// A cubic terrain collider.
/// Superclass for all cubicworld collider implementations.
/// </summary>
public abstract class ACubicTerrainCollider : MonoBehaviour
{
	/// <summary>
	/// The collider.
	/// </summary>
	[HideInInspector]
	public new ACubicTerrainCollider collider;

	/// <summary>
	/// Do collision check against another collider.
	/// Implement all other collider's collision detection here.
	/// </summary>
	/// <returns><c>true</c>, if collision was checked, <c>false</c> otherwise.</returns>
	/// <param name="otherCollider">Other collider.</param>
	public abstract bool CheckCollision (ACubicTerrainCollider otherCollider, ref CubicTerrainHitInfo hitInfo);

	/// <summary>
	/// Gets called after a collision with another collider got detected.
	/// </summary>
	/// <param name="hitInfo">Hit info.</param>
	public void CollisionDetected(CubicTerrainHitInfo hitInfo)
	{
		this.gameObject.SendMessage ("CubicTerrainCollision", hitInfo, SendMessageOptions.DontRequireReceiver);
	}
}
