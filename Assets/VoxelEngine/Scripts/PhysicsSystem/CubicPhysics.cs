using UnityEngine;
using System.Collections;

/// <summary>
/// Cubic raycast hit info structure.
/// Will get returned by the raycast function.
/// </summary>
public struct CubicRaycastHitInfo
{
	/// <summary>
	/// The point where the ray hit the block.
	/// </summary>
	public Vector3 hitPoint;
	/// <summary>
	/// The block which the ray hit.
	/// </summary>
	public Vector3 blockHit;

	/// <summary>
	/// The face hit by the ray.
	/// </summary>
	public BlockFace faceHit;
}

public class CubicPhysics
{
	/// <summary>
	/// Executes an precise raycast.
	/// This is ways slower than the unprecise function.
	/// 
	/// TODO: Extend the raycast implementation for getting precise info at lower calculation time.
	/// </summary>
	/// <returns><c>true</c>, if raycast unprecise was terrained, <c>false</c> otherwise.</returns>
	/// <param name="ray">Ray.</param>
	/// <param name="length">Length.</param>
	/// <param name="hitInfo">Hit info.</param>
	public static bool TerrainRaycastPrecise(Ray ray, float length, out CubicRaycastHitInfo hitInfo)
	{
		Vector3 startPoint = ray.origin;
		Vector3 direction = ray.direction;
		
		return TerrainRaycast (startPoint, direction, 0.1f, length, out hitInfo, true);
	}

	/// <summary>
	/// Executes an unprecise raycast.
	/// Only use this for getting the block position. if you need the face you should use the TerrainRaycastPrecise() function.
	/// </summary>
	/// <returns><c>true</c>, if raycast unprecise was terrained, <c>false</c> otherwise.</returns>
	/// <param name="ray">Ray.</param>
	/// <param name="length">Length.</param>
	/// <param name="hitInfo">Hit info.</param>
	public static bool TerrainRaycastUnprecise(Ray ray, float length, out CubicRaycastHitInfo hitInfo)
	{
		Vector3 startPoint = ray.origin;
		Vector3 direction = ray.direction;
		
		return TerrainRaycast (startPoint, direction, 0.49f, length, out hitInfo, false);
	}

	/// <summary>
	/// Terrain raycast overloaded function.
	/// </summary>
	/// <returns><c>true</c>, if raycast hit a block, <c>false</c> otherwise.</returns>
	/// <param name="ray">Ray.</param>
	/// <param name="stepWidth">Step width.</param>
	/// <param name="length">Length.</param>
	/// <param name="hitInfo">Hit info.</param>
	/// <param name="calculateHitFace">hitFace will only get set if this is true.</param>
	public static bool TerrainRaycast(Ray ray, float stepWidth, float length, out CubicRaycastHitInfo hitInfo, bool calculateHitFace)
	{
		Vector3 startPoint = ray.origin;
		Vector3 direction = ray.direction;

		return TerrainRaycast (startPoint, direction, stepWidth, length, out hitInfo, calculateHitFace);
	}

	/// <summary>
	/// Performs a raycast with the current terrain data.
	/// Raycasts are performed very simple, this function will move along your ray by adding direction*stepWidth every step.
	/// In every step it will check if it hit a block and if true it will stop and return the data.
	/// 
	/// TODO: Implement a better way (more precise!) for raycasting
	/// </summary>
	/// <returns><c>true</c>, if the raycast hit a block, <c>false</c> otherwise.</returns>
	/// <param name="worldspaceStartpoint">Worldspace startpoint.</param>
	/// <param name="direction">Direction.</param>
	/// <param name="stepWidth">The step width you want to use for raycasting. the lower this is the more precise the result and slower the calculation will be.</param>
	/// <param name="length">The length of the ray.</param>
	/// <param name="hitInfo">The raycast hit info.</param>
	/// <param name="calculateHitFace">hitFace will only get set if this is true.</param>
	public static bool TerrainRaycast(Vector3 worldspaceStartpoint, Vector3 direction, float stepWidth, float length, out CubicRaycastHitInfo hitInfo, bool calculateHitFace)
	{
		// Initialize
		bool blockHit = false;
		float distanceTraveled = 0;
		Vector3 currentPos = worldspaceStartpoint;
		hitInfo = new CubicRaycastHitInfo ();
		Vector3 blockPos = Vector3.zero;

		CubicTerrain terrain = CubicTerrain.GetInstance ();

		// Search for blocks on the ray
		while (!blockHit && distanceTraveled < length)
		{
			// Calculate current step data
			currentPos += direction * stepWidth;
			distanceTraveled+=stepWidth;
			blockPos = terrain.GetBlockPosition(currentPos);

			// Check if there is a block at this position
			if (terrain.HasBlock(blockPos))
			{
				blockHit = true;
				hitInfo.blockHit = blockPos;
				hitInfo.hitPoint = currentPos;

				if (calculateHitFace)
				{
					// Get hit face
					
					/*LEFT = 0,
					RIGHT = 1,
					TOP = 2,
					BOTTOM = 3,
					FRONT = 4,
					BACK = 5*/

					// Back and forward flipped
					Vector3[] faceNormals = new Vector3[]
					{
						Vector3.left,
						Vector3.right,
						Vector3.up,
						Vector3.down,
						Vector3.forward,
						Vector3.back
					};

					// Get block center position
					Vector3 blockCenterPosition = blockPos + new Vector3(0.5f, 0.5f, 0.5f);

					// Get shortest distance face.
					float shortestDistance = 1.01f;
					int shortestFace = -1;

					for (int i = 0; i < faceNormals.Length; i++)
					{
						// Get distance from hit point to the current normal + blockcenter
						Vector3 blockNormalPosition = blockCenterPosition + faceNormals[i];
						float distance = Vector3.Distance(currentPos, blockNormalPosition);

						if (shortestDistance > distance)
						{
							shortestDistance = distance;
							shortestFace = i;
						}
					}

					// Get face
					hitInfo.faceHit = (BlockFace)shortestFace;
				}
			}
		}

		return blockHit;
	}
}
