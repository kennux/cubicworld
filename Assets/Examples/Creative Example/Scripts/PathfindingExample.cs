using UnityEngine;
using System.Collections;

public class PathfindingExample : MonoBehaviour
{
	/// <summary>
	/// The player camera where the raycast originates.
	/// </summary>
	public Camera playerCamera;
	/// <summary>
	/// The detection mask.
	/// </summary>
	public LayerMask detectionMask;

	// A* path points
	private Vector3 startPoint = Vector3.zero;
	private Vector3 goalPoint = Vector3.zero;
	
	public void Update()
	{
		bool firstPoint = Input.GetKeyDown (KeyCode.P);
		bool lastPoint = Input.GetKeyDown (KeyCode.L);
		bool definePoint = firstPoint || lastPoint;

		// Right mouse button.
		if (definePoint)
		{
			// Get ready to perform the raycast.
			RaycastHit hitInfo = new RaycastHit();
			Ray cameraRay = this.playerCamera.camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
			Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 100.0f);
			
			// Perform the raycast
			if (Physics.Raycast(cameraRay, out hitInfo, 5, this.detectionMask.value))
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
						BlockHitInfo blockHitInfo = chunk.GetBlockHitInfo(hitInfo);
						int x = (int)blockHitInfo.hitBlock.x;
						int y = (int)blockHitInfo.hitBlock.y;
						int z = (int)blockHitInfo.hitBlock.z;
						
						// Which face was hit? calculate target position for the new block
						switch (blockHitInfo.hitFace)
						{
						case BlockFace.LEFT:
							x-=1;
							break;
						case BlockFace.RIGHT:
							x+=1;
							break;
						case BlockFace.TOP:
							y+=1;
							break;
						case BlockFace.BOTTOM:
							y-=1;
							break;
						case BlockFace.FRONT:
							z+=1;
							break;
						case BlockFace.BACK:
							z-=1;
							break;
						}
						
						Vector3 chunkPos = chunk.chunkPosition;
						
						// Get chunk we want to place the block on
						if (x < 0)
						{
							chunkPos.x-= 1;
							x=chunk.master.chunkWidth-1;
						}
						if (x >= chunk.master.chunkWidth)
						{
							chunkPos.x+= 1;
							x=0;
						}
						if (z < 0)
						{
							chunkPos.z-= 1;
							z=chunk.master.chunkDepth-1;
						}
						if (z >= chunk.master.chunkDepth)
						{
							chunkPos.z+= 1;
							z=0;
						}
						
						// Finally place the object
						GameObject chunkObject = chunk.master.GetChunkObject((int) chunkPos.x, (int) chunkPos.z);
						chunk = chunkObject.GetComponent<CubicTerrainChunk>();

						// Get absolute position
						Vector3 absoluteVoxelspace = chunk.GetAbsolutePosition(new Vector3(x,y,z));

						Debug.Log ("Point: " + absoluteVoxelspace);
						if (firstPoint)
							startPoint = absoluteVoxelspace;
						else if (lastPoint)
							goalPoint = absoluteVoxelspace;
					}
				}
			}
		}

		if (startPoint != Vector3.zero && goalPoint != Vector3.zero)
		{
			Debug.Log ("Starting A* path finding");

		}
	}
}
