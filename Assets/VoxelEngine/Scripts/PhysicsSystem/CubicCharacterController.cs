using UnityEngine;
using System.Collections;

public class CubicCharacterController : MonoBehaviour
{
	/// <summary>
	/// The gravity constant in units/second.
	/// </summary>
	public Vector3 gravity = new Vector3(0, -9.81f, 0);

	/// <summary>
	/// The height of the character in blocks.
	/// </summary>
	public int characterHeight;

	/// <summary>
	/// The velocity.
	/// </summary>
	private Vector3 velocity;

	public void Start()
	{

	}

	public void SimpleMove(Vector3 direction, float speed)
	{
		this.velocity += direction.normalized * speed;
	}

	public void FixedUpdate()
	{
		CubicTerrain terrainInstance = CubicTerrain.GetInstance ();

		// Initialize
		Vector3 currentPosition = this.transform.position;
		Vector3 newPosition = this.transform.position;

		// Add gravity to current velocity
		this.velocity += this.gravity * Time.deltaTime;

		// Move based on velocity
		newPosition += this.velocity * Time.deltaTime;
		Vector3 realNewPosition = newPosition;

		Vector3 blockSpaceNewPosition = terrainInstance.GetBlockPosition(newPosition);
		bool newPositionValid = true;

		blockSpaceNewPosition.y -= (float)this.characterHeight / 2.0f;
		newPosition.y=blockSpaceNewPosition.y;

		// New position valid?
		for (int i = 0; i < this.characterHeight && newPositionValid; i++)
		{
			if (terrainInstance.HasBlock(blockSpaceNewPosition))
			{
				newPositionValid=false;
			}

			// Check left, right, front and back
			Vector3[] checkPositions = new Vector3[]
			{
				Vector3.left * 0.5f,
				Vector3.right * 0.5f,
				Vector3.forward * 0.5f,
				Vector3.back * 0.5f
			};

			// Check all directions
			foreach (Vector3 checkPosition in checkPositions)
			{
				Vector3 checkPos = checkPosition + newPosition;

				if (terrainInstance.HasBlock(terrainInstance.GetBlockPosition(checkPos)))
				{
					newPositionValid=false;
				}
			}

			blockSpaceNewPosition.y++;
			newPosition.y=blockSpaceNewPosition.y;
		}

		if (newPositionValid)
		{
			Debug.Log ("New position: " + realNewPosition);
			this.transform.position = realNewPosition;
			this.velocity -= this.velocity / 2;
		}
		else
		{
			this.velocity = Vector3.zero;
		}
	}
}
