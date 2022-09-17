using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeldruck.JPS2D
{
	public class Pathfinding : MonoBehaviour 
	{
		private PathRequestManager requestManager;
		private Grid grid;
		
		void Awake() 
		{
			requestManager = GetComponent<PathRequestManager>();
			grid = GetComponent<Grid>();
		}

		public void StartFindPath(Vector3 startPos, Vector3 targetPos, bool isAstar) 
		{
			if (isAstar)
				StartCoroutine(FindPathAStar(startPos, targetPos));
			else
				StartCoroutine(FindPathJPS(startPos, targetPos));
		}
		
		IEnumerator FindPathAStar(Vector3 startPos, Vector3 targetPos) 
		{
			Vector3[] waypoints = Array.Empty<Vector3>();
			bool pathSuccess = false;
			
			Node startNode = grid.NodeFromWorldPoint(startPos);
			Node targetNode = grid.NodeFromWorldPoint(targetPos);

			if (!startNode.isObstructed && !targetNode.isObstructed) 
			{
				Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
				HashSet<Node> closedSet = new HashSet<Node>();
				openSet.Add(startNode);
				
				while (openSet.Count > 0) 
				{
					Node currentNode = openSet.RemoveFirst();
					closedSet.Add(currentNode);
					
					if (currentNode == targetNode) 
					{
						pathSuccess = true;
						break;
					}
					
					foreach (Node neighbour in grid.GetNeighbours(currentNode)) 
					{
						if (neighbour.isObstructed || closedSet.Contains(neighbour)) 
						{
							continue;
						}
						
						int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
						
						if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) 
						{
							neighbour.gCost = newMovementCostToNeighbour;
							neighbour.hCost = GetDistance(neighbour, targetNode);
							neighbour.parent = currentNode;
							
							if (!openSet.Contains(neighbour))
								openSet.Add(neighbour);
						}
					}
				}
			}
			
			yield return null;
			
			if (pathSuccess) 
			{
				waypoints = RetracePath(startNode, targetNode, true);
			}
			
			requestManager.FinishedProcessingPath(waypoints, pathSuccess);
		}
		
		IEnumerator FindPathJPS(Vector3 startPos, Vector3 targetPos)
        {
	        Vector3[] wayPoints = Array.Empty<Vector3>();
            bool pathSuccess = false;
            
            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);
            
            if (!startNode.isObstructed && !targetNode.isObstructed)
            {
                Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
                HashSet<Node> closeSet = new HashSet<Node>();

                openSet.Add(startNode);
                
                while (openSet.Count > 0)
                {
                    Node currentNode = openSet.RemoveFirst();

                    closeSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        pathSuccess = true;
                        break;
                    }

                    List<Node> neighbours = grid.PruneNeighbours(currentNode, targetNode);

                    foreach (Node neighbour in neighbours)
                    {
                        if (neighbour.isObstructed || closeSet.Contains(neighbour))
                            continue;

                        int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);

                            neighbour.parent = currentNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                            else
                                openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
            
            yield return null;

            if (pathSuccess)
            {
                wayPoints = RetracePath(startNode, targetNode, false);
                pathSuccess = wayPoints.Length > 0;
            }
  
            requestManager.FinishedProcessingPath(wayPoints, pathSuccess);
        }
		
		Vector3[] RetracePath(Node startNode, Node endNode, bool isAstar) 
		{
			List<Node> path = new List<Node>();
			Node currentNode = endNode;
			
			while (currentNode != startNode) 
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}

			Vector3[] waypoints;
			
			if (isAstar)
			{
				waypoints = SimplifyPath(path);
			}
			else
			{
				waypoints = new Vector3[path.Count];
				for (int i = 0; i < waypoints.Length; i++)
				{
					waypoints[i] = path[i].position;
				}
			}

			Array.Reverse(waypoints);
			
			return waypoints;
		}
		
		Vector3[] SimplifyPath(List<Node> path) 
		{
			List<Vector3> waypoints = new List<Vector3>();
			Vector2 directionOld = Vector2.zero;
			
			for (int i = 1; i < path.Count; i ++) 
			{
				Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX,path[i - 1].gridZ - path[i].gridZ);
				
				if (directionNew != directionOld) 
				{
					waypoints.Add(path[i].position);
				}
				
				directionOld = directionNew;
			}
			
			return waypoints.ToArray();
		}
		
		int GetDistance(Node nodeA, Node nodeB) 
		{
			int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
			int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);
			
			if (dstX > dstZ)
				return 14 * dstZ + 10 * (dstX - dstZ);
			
			return 14 * dstX + 10 * (dstZ - dstX);
		}
	}
}