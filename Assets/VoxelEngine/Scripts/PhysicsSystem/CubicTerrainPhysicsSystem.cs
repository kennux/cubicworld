using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cubic terrain physics system master class.
/// This class handles collision detections.
/// 
/// This class is only allowed to exist ONE TIME.
/// </summary>
public class CubicTerrainPhysicsSystem : MonoBehaviour
{
	/// <summary>
	/// The instance of the physics system.
	/// </summary>
	private static CubicTerrainPhysicsSystem instance;

	/// <summary>
	/// Gets the instance of the physics system which will get set in Awake().
	/// </summary>
	/// <returns>The instance.</returns>
	public static CubicTerrainPhysicsSystem GetInstance()
	{
		return instance;
	}

	/// <summary>
	/// The colliders list.
	/// </summary>
	private List<ACubicTerrainCollider> colliders;

	/// <summary>
	/// Awakes this instance.
	/// Initializes the singleton pattern.
	/// </summary>
	public void Awake()
	{
		this.colliders = new List<ACubicTerrainCollider> ();
		if (instance != null)
		{
			Debug.LogError ("Not allowed to have 2 CubicTerrainPhysicsSystem. Disabling this one...");
			this.enabled = false;
		}

		instance = this;
	}

	/// <summary>
	/// Does collision checks and forwards this messages:
	/// 
	/// - CubicCollision(CubicTerrainHitInfo hitInfo)
	/// </summary>
	public void FixedUpdate()
	{
		// Check any collider against any collider
		for (int i = 0; i < this.colliders.Count; i++)
		{
			for (int j = 0; j < this.colliders.Count; j++)
			{
				// Actually check collisions
				CubicTerrainHitInfo hitInfo = new CubicTerrainHitInfo();

				if (this.colliders[i].CheckCollision(this.colliders[j], ref hitInfo))
				{
					this.colliders[i].gameObject.SendMessage("CubicCollision", hitInfo, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}
}
