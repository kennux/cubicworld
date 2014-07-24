using UnityEngine;
using System.Collections;

/// <summary>
/// Cubic path information holder.
/// Will get returned from the CubicPathfinder class (calculated in another thread).
/// </summary>
public class CubicPath
{
	public Vector3 startPos;
	public Vector3 goalPos;

	/// <summary>
	/// The runtime in milliseconds.
	/// </summary>
	public float runtime;

	/// <summary>
	/// If this is set to true the pathfinder will only search in block positions where a block is under it.
	/// </summary>
	public bool needsGround;
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="CubicPath"/> is ready.
	/// </summary>
	/// <value><c>true</c> if is ready; otherwise, <c>false</c>.</value>
	public bool isReady
	{
		get { lock (this.stepsLock) { return this.foundPath && this.steps != null; } }
	}

	/// <summary>
	/// Sets a value indicating whether this <see cref="CubicPath"/> found path.
	/// </summary>
	/// <value><c>true</c> if found path; otherwise, <c>false</c>.</value>
	public bool foundPath
	{
		get { return this._foundPath; }
	}

	/// <summary>
	/// True if the pathfinder found a path.
	/// </summary>
	private bool _foundPath = true;
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="CubicPath"/> is valid or not.
	/// It is invalid if there was no path from start to goal.
	/// </summary>
	/// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
	public bool isValid
	{
		get { lock (this.stepsLock) { return this.steps.Length > 0; } }
	}

	/// <summary>
	/// Gets the path data in form of steps.
	/// Each index means a movement from one to another block.
	/// TODO: More documentation?
	/// </summary>
	/// <value>The path data.</value>
	public Vector3[] pathData
	{
		get { return this.steps; }
	}

	private Vector3[] steps;
	private object stepsLock = new object ();
	
	/// <summary>
	/// Initializes a new instance of the <see cref="CubicPath"/> class.
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="goal">Goal.</param>
	public CubicPath(Vector3 start, Vector3 goal)
	{
		this.startPos = start;
		this.goalPos = goal;
	}
	
	/// <summary>
	/// Sets the path data.
	/// Steps is an array of blockspace coordinates, index 0 will be the first point (start point), the last index will be the last point (goal).
	/// Null is given if there is no path from start to goal.
	/// </summary>
	/// <param name="steps">Steps.</param>
	public void SetPathData(Vector3[] steps)
	{
		if (steps == null)
			this._foundPath = false;

		lock (this.stepsLock)
		{
			this.steps = steps;
		}
	}
}