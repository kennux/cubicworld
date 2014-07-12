using UnityEngine;
using System.Collections;

/// <summary>
/// Block implementation.
/// Blocks will get registered in your own implementation of ABlockBootstrap.
/// </summary>
public class Block
{
	public Vector2[] topUv, bottomUv, leftUv, rightUv, frontUv, backUv;

	public void SetTextures(int topTexture, int bottomTexture, int leftTexture, int rightTexture, int frontTexture, int backTexture)
	{
		this.topUv = Blocks.GetUvForTexture (topTexture);
		this.bottomUv = Blocks.GetUvForTexture (bottomTexture);
		this.leftUv = Blocks.GetUvForTexture (leftTexture);
		this.rightUv = Blocks.GetUvForTexture (rightTexture);
		this.frontUv = Blocks.GetUvForTexture (frontTexture);
		this.backUv = Blocks.GetUvForTexture (backTexture);
	}
}
