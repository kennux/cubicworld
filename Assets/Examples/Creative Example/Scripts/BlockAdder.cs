using UnityEngine;
using System.Collections;

public class BlockAdder : MonoBehaviour
{
	/// <summary>
	/// The player camera where the raycast originates.
	/// </summary>
	public Camera playerCamera;
	/// <summary>
	/// The detection mask.
	/// </summary>
	public LayerMask detectionMask;

	public int blockId = 1;
	
	public void Update()
	{
		// Right mouse button.
		if (Input.GetMouseButtonDown(0))
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
						CubicTerrainChunk.TriangleBlockInfo blockInfo = chunk.triangleIndexInfo(hitInfo.triangleIndex);
						int x = blockInfo.x;
						int y = blockInfo.y;
						int z = blockInfo.z;

						// Which face was hit? calculate target position for the new block
						switch (blockInfo.face)
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
						chunkObject.GetComponent<CubicTerrainChunk>().chunkData.SetVoxel(x,y,z,(short)this.blockId);
					}
				}
			}
		}
	}
}
