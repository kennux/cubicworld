using UnityEngine;
using System.Collections;

/// <summary>
/// Demo block bootstrap.
/// </summary>
public class TestBootstrap : ABlockBootstrap
{
	public Texture2D dirtTexture;

	public Texture2D grassSideTexture;
	public Texture2D grassTopTexture;

	protected override void Bootstrap()
	{
		int dirtId = Blocks.AddTexture (this.dirtTexture);
		int grassSideId = Blocks.AddTexture (this.grassSideTexture);
		int grassTopId = Blocks.AddTexture (this.grassTopTexture);
		Blocks.BuildTextureAtlas ();

		Blocks.RegisterBlock (1).SetTextures(grassTopId,dirtId,grassSideId,grassSideId,grassSideId,grassSideId);
	}
}
