using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CubicCharacterController))]
public class FPSController : MonoBehaviour
{
	private CubicCharacterController characterController;

	public float movementSpeed = 0.5f;

	public void Start()
	{
		this.characterController = this.GetComponent<CubicCharacterController> ();
	}

	public void FixedUpdate()
	{
		Vector3 movementDirection = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
		this.characterController.SimpleMove (movementDirection, movementSpeed * Time.deltaTime);
	}
}
