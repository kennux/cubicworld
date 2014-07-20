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
	/// Gets a value indicating whether this <see cref="CubicPath"/> is ready.
	/// </summary>
	/// <value><c>true</c> if is ready; otherwise, <c>false</c>.</value>
	public bool isReady
	{
		get { lock (this.stepsLock) { return steps != null; } }
	}
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="CubicPath"/> is valid or not.
	/// It is invalid if there was no path from start to goal.
	/// </summary>
	/// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
	public bool isValid
	{
		get { lock (this.stepsLock) { return this.steps.Length > 0; } }
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
	/// An empty array is given if there is no path from start to goal.
	/// </summary>
	/// <param name="steps">Steps.</param>
	public void SetPathData(Vector3[] steps)
	{
		lock (this.stepsLock)
		{
			this.steps = steps;
		}
	}
}