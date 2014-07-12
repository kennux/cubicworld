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
			// Get ready to perform the raycast.
			RaycastHit hitInfo = new RaycastHit();
			Ray cameraRay = this.playerCamera.camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
			Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 100.0f);

			// Perform the raycast
			if (Physics.Raycast(cameraRay, out hitInfo, 50, this.detectionMask.value))
			{
				// Chunk hit?
				CubicTerrainChunk chunk = hitInfo.collider.GetComponent<CubicTerrainChunk>();
				if (chunk != null)
				{
					// Yes, chunk hit!
					// Delete the clicked block
					Debug.Log ("Triangle clicked: " + hitInfo.triangleIndex);
					Vector3 block = chunk.triangleIndexToBlock(hitInfo.triangleIndex);
					Debug.Log ("Block clicked: " + block);

					chunk.chunkData.SetVoxel((int)block.x, (int) block.y, (int) block.z, -1);
				}
			}
		}
	}
}
