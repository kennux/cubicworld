using UnityEngine;
using System.Collections;

public class CreativeGUI : MonoBehaviour
{
	// Textures
	public Texture2D cursorTexture;

	// 
	public int selectedBlockId = 1;

	// Drawing rects
	public Rect currentBlockTexturePosition;
	public Rect controlsTextPosition;

	// Block adder script
	public BlockAdder blockAdder;

	public void Update()
	{
		// Change selected block
		if (Input.GetKeyDown (KeyCode.X) && Blocks.GetBlock(this.selectedBlockId+1) != null)
		{
			this.selectedBlockId++;
		}
		else if (Input.GetKeyDown (KeyCode.Y) && Blocks.GetBlock(this.selectedBlockId-1) != null)
		{
			this.selectedBlockId--;
		}

		this.blockAdder.blockId = this.selectedBlockId;
	}

	public void OnGUI()
	{
		// Current block
		GUI.DrawTexture (currentBlockTexturePosition, Blocks.GetTextureById(Blocks.GetBlock (this.selectedBlockId).leftTexture));

		// Cursor
		GUI.DrawTexture (new Rect ((Screen.width / 2) - (this.cursorTexture.width / 2), (Screen.height / 2) - (this.cursorTexture.height / 2), this.cursorTexture.width, this.cursorTexture.height), this.cursorTexture);
	
		// Controls
		GUI.Label (this.controlsTextPosition, "Controls: X: Next block, Y: Last block, Mouse Left: Place Block, Mouse Right: Remove Block");
	}
}
