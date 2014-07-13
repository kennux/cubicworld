using UnityEngine;
using System.Collections;

/// <summary>
/// Block implementation.
/// Blocks will get registered in your own implementation of ABlockBootstrap.
/// </summary>
public class Block
{
	public int topTexture, bottomTexture, leftTexture, rightTexture, frontTexture, backTexture;
	public Vector2[] topUv, bottomUv, leftUv, rightUv, frontUv, backUv;

	public bool transparentBlock;

	/// <summary>
	/// Sets the texture information for the given block.
	/// </summary>
	/// <param name="topTexture">Top texture.</param>
	/// <param name="bottomTexture">Bottom texture.</param>
	/// <param name="leftTexture">Left texture.</param>
	/// <param name="rightTexture">Right texture.</param>
	/// <param name="frontTexture">Front texture.</param>
	/// <param name="backTexture">Back texture.</param>
	/// <param name="transparentBlock">If set to <c>true</c> transparent block.</param>
	public void SetTextures(int topTexture, int bottomTexture, int leftTexture, int rightTexture, int frontTexture, int backTexture, bool transparentBlock)
	{
		// Save texture ids
		this.topTexture = topTexture;
		this.bottomTexture = bottomTexture;
		this.leftTexture = leftTexture;
		this.rightTexture = rightTexture;
		this.frontTexture = frontTexture;
		this.backTexture = backTexture;

		// Save UV data
		this.topUv = Blocks.GetUvForTexture (topTexture);
		this.bottomUv = Blocks.GetUvForTexture (bottomTexture);
		this.leftUv = Blocks.GetUvForTexture (leftTexture);
		this.rightUv = Blocks.GetUvForTexture (rightTexture);
		this.frontUv = Blocks.GetUvForTexture (frontTexture);
		this.backUv = Blocks.GetUvForTexture (backTexture);
		this.transparentBlock = transparentBlock;
	}
}
