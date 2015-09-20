using UnityEngine;
using System.Collections;

/// <summary>
/// An abstract block bootstrap.
/// Register your blocks in Bootstrap().
/// </summary>
public abstract class ABlockBootstrap : MonoBehaviour
{
	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Awake()
	{
		this.Bootstrap ();
	}

	/// <summary>
	/// Override this function and do your Blocks. class in here to register all blocks.
	/// </summary>
	protected abstract void Bootstrap();
}
