using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Cubic pathfinding implementation.
/// </summary>
public class CubicPathfinding
{
	#region Singleton
	private static CubicPathfinding instance; 

	public static CubicPathfinding GetInstance()
	{
		if (instance == null)
			instance = new CubicPathfinding (CubicTerrain.GetInstance ());
		return instance;
	}
	#endregion

	/// <summary>
	/// The worker thread instance;
	/// </summary>
	private Thread workerThread;

	/// <summary>
	/// The terrain instance.
	/// </summary>
	private CubicTerrain terrain;

	/// <summary>
	/// The paths to calculate.
	/// </summary>
	private Queue<CubicPath> pathQueue;
	private object pathQueueLock = new object ();

	/// <summary>
	/// Initializes a new instance of the <see cref="CubicPathfinding"/> class.
	/// </summary>
	/// <param name="terrain">Terrain.</param>
	private CubicPathfinding(CubicTerrain terrain)
	{
		this.terrain = terrain;
		this.workerThread = new Thread (this.WorkerThread);
	}

	/// <summary>
	/// Gets the path.
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="startingPos">Starting position.</param>
	/// <param name="goalPos">Goal position.</param>
	public CubicPath GetPath(Vector3 startingPos, Vector3 goalPos)
	{
		CubicPath pathObject = new CubicPath (startingPos, goalPos);

		// Enqueue path
		lock (this.pathQueueLock)
		{
			this.pathQueue.Enqueue(pathObject);
		}

		return pathObject;
	}

	/// <summary>
	/// The pathfinding worker thread.
	/// </summary>
	private void WorkerThread()
	{
		List<CubicPath> paths = new List<CubicPath>();

		lock (this.pathQueueLock)
		{
			// Check for work
			while (this.pathQueue.Count > 0)
			{
				// Save path for calculation
				paths.Add (this.pathQueue.Dequeue());
			}
		}

		// Do path calculations
		foreach (CubicPath path in paths)
		{

		}

		// Wait before the next recursive call
		Thread.Sleep (10);
		this.WorkerThread ();
	}
}

/// <summary>
/// Cubic path information holder.
/// Will get returned from the CubicPathfinder class (calculated in another thread).
/// </summary>
public class CubicPath
{
	public Vector3 startPos;
	public Vector3 goalPos;

	public bool isReady
	{
		get { return false; }
	}

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
}