using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Blocks "holder".
/// Holds all registered blocks in a dictionary.
/// </summary>
public class Blocks
{
	/// <summary>
	/// The blocks dictionary.
	/// </summary>
	private static Dictionary<int, Block> blocks;
	private static Dictionary<int, Texture2D> textures;
	private static Dictionary<int, Vector2[]> textureCoordinates;

	public static Texture2D textureAtlas;
	private static Vector2 textureDimension;

	private static void Initialize()
	{
		if (blocks == null)
		{
			blocks = new Dictionary<int, Block>();
		}

		if (textures == null)
		{
			textures = new Dictionary<int, Texture2D>();
		}
	}

	/// <summary>
	/// Adds the texture to the textures dictionary used for building the texture atlas.
	/// Returns -1 in case of an error.
	/// </summary>
	/// <returns>The texture.</returns>
	/// <param name="texture">Texture.</param>
	public static int AddTexture(Texture2D texture)
	{
		Initialize ();

		// Set dimension
		if (textureDimension == Vector2.zero)
				textureDimension = new Vector2 (texture.width, texture.height);

		// Check dimensions
		if (texture.width != textureDimension.x || texture.height != textureDimension.y)
		{
			Debug.LogError("Added texture to block textures with wrong dimension! Set dimension: " + textureDimension + ", texture dimension: " + new Vector2(texture.width, texture.height));
			return -1;
		}

		int textureId = textures.Count;

		// Add texture to the atlas
		textures.Add (textureId, texture);

		return textureId;
	}

	/// <summary>
	/// Gets the texture by identifier.
	/// </summary>
	/// <returns>The texture by identifier.</returns>
	/// <param name="textureId">Texture identifier.</param>
	public static Texture2D GetTextureById(int textureId)
	{
		return textures [textureId];
	}

	/// <summary>
	/// Builds the texture atlas.
	/// </summary>
	public static void BuildTextureAtlas()
	{
		// Build texture array
		Texture2D[] atlasTextures = new Texture2D[textures.Count];
		foreach (KeyValuePair<int, Texture2D> texture in textures)
        {
			atlasTextures[texture.Key] = texture.Value;
		}

		// Build atlas
		Texture2D _textureAtlas = new Texture2D (4096, 4096, TextureFormat.RGBA32, false);
		Rect[] uvRects = _textureAtlas.PackTextures (atlasTextures, 0);

		textureAtlas = new Texture2D(_textureAtlas.width, _textureAtlas.height, TextureFormat.RGBA32, false);
		textureAtlas.SetPixels (0,0,_textureAtlas.width, _textureAtlas.height, _textureAtlas.GetPixels (0,0,_textureAtlas.width, _textureAtlas.height));

		// Set texture atlas properties
		textureAtlas.anisoLevel = 9;
		textureAtlas.filterMode = FilterMode.Point;
		textureAtlas.Apply ();

		// Save uvs
		textureCoordinates = new Dictionary<int, Vector2[]> ();
		foreach (KeyValuePair<int, Texture2D> texture in textures)
		{
			textureCoordinates.Add (texture.Key,new Vector2[]
			{
				new Vector2(uvRects[texture.Key].x, uvRects[texture.Key].y),
				new Vector2(uvRects[texture.Key].x+uvRects[texture.Key].width, uvRects[texture.Key].y),
				new Vector2(uvRects[texture.Key].x+uvRects[texture.Key].width, uvRects[texture.Key].y+uvRects[texture.Key].height),
				new Vector2(uvRects[texture.Key].x, uvRects[texture.Key].y+uvRects[texture.Key].height)
			});
		}
	}

	/// <summary>
	/// Gets the uv coordinates for texture.
	/// indices:
	/// 0 - bottomleft
	/// 1 - bottomright
	/// 2 - topright
	/// 3 - topleft
	/// </summary>
	public static Vector2[] GetUvForTexture(int textureId)
	{
		return textureCoordinates[textureId];
	}

	/// <summary>
	/// Registers the block with the given blockId.
	/// Returns the new block for chaining.
	/// 
	/// Returns null in case of an error.
	/// </summary>
	/// <returns>The block.</returns>
	public static Block RegisterBlock(int blockId)
	{
		Initialize ();

		if (blocks.ContainsKey(blockId))
		{
			Debug.LogError("Block with id " + blockId + " already exists!");
			return null;
		}

		blocks.Add (blockId, new Block ());
		return blocks[blockId];
	}

	/// <summary>
	/// Gets the block with the given blockId.
	/// Returns null if there was an error or the block is not found.
	/// </summary>
	/// <returns>The block.</returns>
	/// <param name="blockId">Block identifier.</param>
	public static Block GetBlock(int blockId)
	{
		if (blocks == null || !blocks.ContainsKey (blockId))
			return null;

		return blocks [blockId];
	}
}

public enum BlockFace
{
	LEFT,
	RIGHT,
	TOP,
	BOTTOM,
	FRONT,
	BACK
}
