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
		while (true)
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
				this.FindPath(path);
			}

			Thread.Sleep (10);
		}
	}

	/// <summary>
	/// Finds the path path.
	/// </summary>
	/// <param name="path">Path.</param>
	private void FindPath (CubicPath path)
	{
		Vector3 globalDirection = (path.goalPos - path.startPos).normalized;
		List<Vector3> nodes = new List<Vector3> ();

	}
}