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

	public void FixedUpdate()
	{
		CubicTerrain terrainInstance = CubicTerrain.GetInstance ();

		// Initialize
		Vector3 currentPosition = this.transform.position;
		Vector3 newPosition = this.transform.position;

		// Add gravity to current velocity
		this.velocity += this.gravity * Time.deltaTime;

		// Move based on velocity
		newPosition += this.velocity;

		Vector3 blockSpaceNewPosition = terrainInstance.GetBlockPosition(newPosition);
		bool newPositionValid = true;

		blockSpaceNewPosition.y -= (float)this.characterHeight / 2.0f;

		// New position valid?
		for (int i = 0; i < this.characterHeight && newPositionValid; i++)
		{
			if (terrainInstance.HasBlock(blockSpaceNewPosition))
			{
				newPositionValid=false;
			}
			blockSpaceNewPosition.y++;
		}

		if (newPositionValid)
		{
			Debug.Log ("New position: " + newPosition);
			this.transform.position = newPosition;
		}
		else
		{
			this.velocity = Vector3.zero;
		}
	}
}
