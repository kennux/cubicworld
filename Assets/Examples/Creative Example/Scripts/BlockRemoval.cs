using UnityEngine;
using System.Collections;

/// <summary>
/// Block removal demo script.
/// Retrieves the clicks triangle index by raycasting.
/// After that it resolves the clicked block position in the click chunk.
/// Then it sets the block id to -1 which means no block.
/// </summary>
public class BlockRemoval : MonoBehaviour
{
	/// <summary>
	/// The player camera where the raycast originates.
	/// </summary>
	public Camera playerCamera;
	/// <summary>
	/// The detection mask.
	/// </summary>
	public LayerMask detectionMask;

	public void Update()
	{
		// Right mouse button.
		if (Input.GetMouseButtonDown(1))
		{
			// The unity physics way

			if (CubicTerrain.GetInstance().useMeshColliders)
			{
				// Get ready to perform the raycast.
				RaycastHit hitInfo = new RaycastHit();
				Ray cameraRay = this.playerCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
				Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 100.0f);

				// Perform the raycast
				if (Physics.Raycast(cameraRay, out hitInfo, 50, this.detectionMask.value))
				{
					if (hitInfo.collider == null)
						return;

					// get collider parent
					Transform chunkTransform = hitInfo.collider.transform.parent;

					if (chunkTransform != null)
					{
						// Chunk hit?
						CubicTerrainChunk chunk = chunkTransform.GetComponent<CubicTerrainChunk>();
						if (chunk != null && !chunk.isDirty)
						{
							// Yes, chunk hit!
							// Delete the clicked block
							Vector3 block = chunk.GetBlockPosition(hitInfo, -0.5f);

							chunk.chunkData.SetVoxel((int)block.x, (int) block.y, (int) block.z, -1);
						}
					}
				}
			}
			else
			{
				// Cubic World Physics way
				CubicRaycastHitInfo hitInfo = new CubicRaycastHitInfo();
				if (CubicPhysics.TerrainRaycastUnprecise(this.playerCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2)), 5.0f, out hitInfo))
				{
					// Debug.Log ("Hit: " + hitInfo.hitPoint + ", Block: " + hitInfo.blockHit + ", Face: " + hitInfo.faceHit);
					// Hit block
					CubicTerrain.GetInstance().SetBlock((int)hitInfo.blockHit.x, (int)hitInfo.blockHit.y, (int)hitInfo.blockHit.z, -1);
				}
			}
		}
	}
}
