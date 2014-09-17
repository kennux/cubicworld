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

	private CubicPath path;
	
	public void Update()
	{
		bool firstPoint = Input.GetKeyDown (KeyCode.P);
		bool lastPoint = Input.GetKeyDown (KeyCode.L);
		bool definePoint = firstPoint || lastPoint;

		if (this.path != null)
		{
			if (this.path.isReady)
			{
				Debug.Log ("Path found in " + path.runtime + " milliseconds!");
				foreach (Vector3 blockPos in this.path.pathData)
				{
					CubicTerrain.GetInstance().SetBlock((int)blockPos.x, (int)blockPos.y-1, (int)blockPos.z, -1);
				}
				this.path = null;
			}
			else if (!this.path.foundPath)
			{
				Debug.Log ("Path not found :-( after search for " + path.runtime + " milliseconds!");
			}
		}

		// Right mouse button.
		if (definePoint)
		{
			if (CubicTerrain.GetInstance().useMeshColliders)
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
							GameObject chunkObject = chunk.master.GetChunkObject((int) chunkPos.x, (int)chunkPos.y, (int) chunkPos.z);
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
			else
			{
				// Cubic World Physics way
				CubicRaycastHitInfo hitInfo = new CubicRaycastHitInfo();
				if (CubicPhysics.TerrainRaycastUnprecise(this.playerCamera.camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2)), 5.0f, out hitInfo))
				{
					// Debug.Log ("Hit: " + hitInfo.hitPoint + ", Block: " + hitInfo.blockHit + ", Face: " + hitInfo.faceHit);
					// Hit block
					Vector3 topBlock = hitInfo.blockHit + Vector3.up;
					Debug.Log ("Top Block: " + topBlock);
					if (firstPoint)
						startPoint = topBlock;
					else if (lastPoint)
						goalPoint = topBlock;
				}
			}
		}

		if (startPoint != Vector3.zero && goalPoint != Vector3.zero)
		{
			Debug.Log ("Starting A* path finding. Distance: " + Vector3.Distance(startPoint, goalPoint));

			// Start pathfinding
			CubicPathfinding pathfinder = CubicPathfinding.GetInstance();
			this.path = pathfinder.GetPath(startPoint, goalPoint, true);

			startPoint = Vector3.zero;
			goalPoint = Vector3.zero;
		}
	}
}
