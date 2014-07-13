using UnityEngine;
using System.Collections;

public class PhysicsSample : MonoBehaviour
{
	public GameObject samplePrefab;
	public Rect spawnRect;

	public void Update()
	{
		if (Input.GetKeyDown (KeyCode.U))
		{
			for (int i = 0; i < 25; i++)
			{
				Vector3 spawnPosition = new Vector3
				(
						this.spawnRect.x + (this.spawnRect.width * Random.value),
						20,
						this.spawnRect.y + (this.spawnRect.height * Random.value)
				);

				Instantiate (this.samplePrefab, spawnPosition, Quaternion.Euler(Random.value * 360, Random.value * 360, Random.value * 360));
			}
		}
	}
}
