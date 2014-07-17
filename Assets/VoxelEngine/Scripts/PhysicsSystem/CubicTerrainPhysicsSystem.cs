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
	/// The colliders list.
	/// These colliders are update in FixedUpdate()
	/// </summary>
	private static List<ACubicTerrainCollider> colliders;

	private static bool alreadyInitialized = false;

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	private static void Initialize()
	{
		if (colliders == null)
			colliders = new List<ACubicTerrainCollider> ();
	}

	/// <summary>
	/// Adds the collider to the colliders list.
	/// </summary>
	/// <param name="collider">Collider.</param>
	public static void AddCollider(ACubicTerrainCollider collider)
	{
		Initialize ();
	}

	/// <summary>
	/// Start this instance.
	/// Initializes the physics system instance.
	/// </summary>
	public void Start()
	{
		// Check if already initialized
		if (alreadyInitialized)
		{
			Debug.LogError ("Not allowed to have 2 CubicTerrainPhysicSystems in the scene! Disabling myself...");
			this.enabled = false;
			return;
		}

		// Initialized
		alreadyInitialized = true;
	}

	/// <summary>
	/// Fixeds the update.
	/// </summary>
	public void FixedUpdate()
	{

	}
}
