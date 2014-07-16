using UnityEngine;
using System.Collections;

/// <summary>
/// Cubic terrain hit info.
/// PLACEHOLDER
/// </summary>
public class CubicTerrainHitInfo
{

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
	public ACubicTerrainCollider collider;

	/// <summary>
	/// Do collision check against another collider.
	/// Implement all other collider's collision detection here.
	/// </summary>
	/// <returns><c>true</c>, if collision was checked, <c>false</c> otherwise.</returns>
	/// <param name="otherCollider">Other collider.</param>
	public abstract bool CheckCollision (ACubicTerrainCollider otherCollider, ref CubicTerrainHitInfo hitInfo);
}
