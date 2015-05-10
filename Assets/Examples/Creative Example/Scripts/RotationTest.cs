using UnityEngine;
using System.Collections;

public class RotationTest : MonoBehaviour
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
		if (Input.GetKeyDown (KeyCode.R))
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
						// Rotate the clicked block
						Vector3 block = chunk.GetBlockPosition(hitInfo, -0.5f);


						byte rotation = (byte)Mathf.Repeat(chunk.chunkData.GetVoxel((int)block.x, (int) block.y, (int) block.z).rotation+1, 4);
						chunk.chunkData.GetVoxel((int)block.x, (int) block.y, (int) block.z).rotation = rotation;
						chunk.chunkData.DataUpdated();
						Debug.Log ("Set rotation to " + rotation + " at " + new Vector3((int)block.x, (int) block.y, (int) block.z));
					}
				}
			}
		}
	}
}
