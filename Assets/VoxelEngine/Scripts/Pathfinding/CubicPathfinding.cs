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
		this.workerThread.Start ();
		this.pathQueue = new Queue<CubicPath> ();
	}

	/// <summary>
	/// Gets the path.
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="startingPos">Starting position.</param>
	/// <param name="goalPos">Goal position.</param>
	/// <param name="needsGround"><see cref="CubicPath.needsGround"/></param> 
	public CubicPath GetPath(Vector3 startingPos, Vector3 goalPos, bool needsGround = false)
	{
		CubicPath pathObject = new CubicPath (startingPos, goalPos);
		pathObject.needsGround = needsGround;

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
		// List definitions
		Dictionary<Vector3, PathNode> openList = new Dictionary<Vector3, PathNode>();

		// Start pathfinding
		PathNode startNode = new PathNode(path.startPos, null, 0, path.goalPos);
		startNode.owner=startNode;
		PathNode currentNode = startNode;
		
		bool pathFound = false;
		bool noPath = false;
		int movementCost = 0;
		while (!pathFound && !noPath)
		{
			// Calculate block direction positions
			Vector3[] positions = new Vector3[]
			{
				// Front
				currentNode.position + Vector3.forward,
				// Back
				currentNode.position + Vector3.back,
				// Left
				currentNode.position + Vector3.left,
				// Right
				currentNode.position + Vector3.right,
				// Front right
				currentNode.position + Vector3.forward + Vector3.right,
				// Front left
				currentNode.position + Vector3.forward + Vector3.left,
				// Back right
				currentNode.position + Vector3.back + Vector3.right,
				// Back left
				currentNode.position + Vector3.back + Vector3.left
			};

			// Analyze surrounding path nodes
			PathNode[] nodes = new PathNode[positions.Length];
			PathNode lowestCostNode = null;

			// Check which ones are walkable and add them to the nodes-array
			for (int i = 0; i < positions.Length; i++)
			{
				// Movement cost from this to the surrounding block
				int currentMovementCost = (int)(Vector3.Distance(positions[i], currentNode.position)*10);

				// Check if this node is walkable
				if (!this.terrain.HasBlock((int)positions[i].x, (int)positions[i].y, (int)positions[i].z) && 
				    // Walkable check
				    (!path.needsGround || this.terrain.HasBlock((int)positions[i].x, (int)positions[i].y-1, (int)positions[i].z)))
				{
					// Add node to the nodes-array
					if (openList.ContainsKey(positions[i]))
					{
						nodes[i]=openList[positions[i]];
					}
					else
					{
						nodes[i] = new PathNode(positions[i], currentNode, movementCost+currentMovementCost, path.goalPos);
						openList.Add (positions[i], nodes[i]);
					}

				}

				// Check for lowest cost
				if (nodes[i] != null && (lowestCostNode == null || nodes[i].completeCost < lowestCostNode.completeCost))
				{
					lowestCostNode = nodes[i];
				}
			}

			// Failed? o_O
			if (lowestCostNode == null)
			{
				noPath = true;
				break;
			}

			if (currentNode.position == path.goalPos)
				pathFound=true;

			// Put the lowest cost node on the closed list
			if (currentNode.owner.position == lowestCostNode.owner.position)
			{
				currentNode.owner.nextNode=lowestCostNode;
			}
			else
				currentNode.nextNode = lowestCostNode;

			currentNode = lowestCostNode;
		}

		// No path found?
		if (noPath)
		{
			// :'(
			path.SetPathData(null);
		}
		else
		{
			// :^)
			List<Vector3> pathData = new List<Vector3>();
			PathNode cNode = startNode;
			while (cNode != null)
			{
				pathData.Add (cNode.position);
				cNode = cNode.nextNode;
			}

			path.SetPathData(pathData.ToArray());
		}
	}
}

/// <summary>
/// Path node implementation.
/// Holds all data for path nodes.
/// 
/// Implementation details:
/// All costs are distances times 10.
/// So heuristic cost is calculated by (int)(Vector3.distance(myPosition, goalPosition) * 10).
/// This is done to avoid floating-point calculations for better performance.
/// </summary>
public class PathNode
{
	public Vector3 position;

	/// <summary>
	/// The movement cost.
	/// </summary>
	public int movementCost;

	/// <summary>
	/// The heuristic cost from this position to the target position.
	/// </summary>
	public int heuristicCost;

	/// <summary>
	/// The complete cost (movementCost + heuristicCost).
	/// </summary>
	public int completeCost;

	/// <summary>
	/// The owner of this path node.
	/// </summary>
	public PathNode owner;

	public PathNode nextNode;

	public PathNode(Vector3 position, PathNode owner, int movementCost, Vector3 targetPosition)
	{
		this.position = position;

		// Calculate costs
		this.heuristicCost = (int)(Vector3.Distance (position, targetPosition)*10);
		this.movementCost = movementCost;
		this.completeCost = this.heuristicCost + this.movementCost;

		// Set owner reference
		this.owner = owner;
	}
}