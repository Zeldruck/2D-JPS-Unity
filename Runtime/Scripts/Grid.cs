using System.Collections.Generic;
using UnityEngine;

namespace Zeldruck.JPS2D
{
    public class Grid : MonoBehaviour 
    {
	    private Node[,] grid;
	    
	    private float nodeDiameter;
	    private int gridSizeX, gridSizeZ;
	    
	    public int MaxSize => gridSizeX * gridSizeZ;
	    
	    [Header("Grid")]
	    public Vector2 gridWorldSize;
	    public float nodeRadius;
	    public LayerMask unWalkableMask;
	    [Space]
		public bool displayGridGizmos;

		private void Awake() 
		{
			nodeDiameter = nodeRadius * 2;
			gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
			gridSizeZ = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
			CreateGrid();
		}

		void CreateGrid() 
		{
			grid = new Node[gridSizeX,gridSizeZ];
			Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

			for (int x = 0; x < gridSizeX; x ++)
			{
				for (int z = 0; z < gridSizeZ; z ++)
				{
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
					bool isObstructed = Physics.CheckSphere(worldPoint, nodeRadius, unWalkableMask);
					grid[x, z] = new Node(isObstructed, worldPoint, x,z);
				}
			}
		}

		public List<Node> GetNeighbours(Node node) 
		{
			List<Node> neighbours = new List<Node>();

			for (int x = -1; x <= 1; x++) 
			{
				for (int z = -1; z <= 1; z++) 
				{
					if (x == 0 && z == 0)
						continue;

					int checkX = node.gridX + x;
					int checkZ = node.gridZ + z;

					if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ) 
					{
						neighbours.Add(grid[checkX, checkZ]);
					}
				}
			}

			return neighbours;
		}

		#region JPS

		public List<Node> PruneNeighbours(Node currentNode, Node destinationNode)
		{
			List<Node> returnNeighbours = new List<Node>();
            
			List<Node> neighbours = GetNeighbours(currentNode);

			foreach (Node neighbour in neighbours)
			{
				int x = Mathf.Clamp(neighbour.gridX - currentNode.gridX, -1, 1);
				int y = Mathf.Clamp(neighbour.gridZ - currentNode.gridZ, -1, 1);

				Node jumpPoint = Jump(currentNode, x, y, destinationNode);
                
				if (jumpPoint != null)
					returnNeighbours.Add(jumpPoint);
			}

			return returnNeighbours;
		}
		
		private Node Jump(Node currentNode, int xDirection, int yDirection, Node destination)
        {
            int xJumpPosition = currentNode.gridX + xDirection;
            int yJumpPosition = currentNode.gridZ + yDirection;

            if (!IsWalkable(xJumpPosition, yJumpPosition))
                return null;

            Node jumpPoint = grid[xJumpPosition, yJumpPosition];

            if (jumpPoint == destination)
                return jumpPoint;

            // Horizontals
            if (xDirection != 0 && yDirection == 0)
            {
                if (!IsWalkable(currentNode.gridX, currentNode.gridZ + 1) &&
                    IsWalkable(currentNode.gridX + xDirection, currentNode.gridZ + 1))
                {
                    return jumpPoint;
                }
                else if (!IsWalkable(currentNode.gridX, currentNode.gridZ - 1) &&
                         IsWalkable(currentNode.gridX + xDirection, currentNode.gridZ - 1))
                {
                    return jumpPoint;
                }
            }
            // Verticals
            else if (xDirection == 0 && yDirection != 0)
            {
                if (!IsWalkable(currentNode.gridX + 1, currentNode.gridZ) &&
                    IsWalkable(currentNode.gridX + 1, currentNode.gridZ + yDirection))
                {
                    return jumpPoint;
                }
                else if (!IsWalkable(currentNode.gridX - 1, currentNode.gridZ) &&
                         IsWalkable(currentNode.gridX - 1, currentNode.gridZ + yDirection))
                {
                    return jumpPoint;
                }
            }
            // Diagonals
            else if (xDirection != 0 && yDirection != 0)
            {
                if (!IsWalkable(currentNode.gridX + xDirection, currentNode.gridZ))
                {
                    return jumpPoint;
                }
                else if (!IsWalkable(currentNode.gridX, currentNode.gridZ + yDirection))
                {
                    return jumpPoint;
                }

                if (Jump(jumpPoint, xDirection, 0, destination) != null ||
                    Jump(jumpPoint, 0, yDirection, destination) != null)
                {
                    return jumpPoint;
                }
            }

            return Jump(jumpPoint, xDirection, yDirection, destination);
        }
		
		private bool IsWalkable(int gridX, int gridZ)
		{
			if (gridX < 0 || gridX > gridSizeX - 1 || gridZ < 0 || gridZ > gridSizeZ - 1)
				return false;

			return !grid[gridX, gridZ].isObstructed;
		}

		#endregion
		

		public Node NodeFromWorldPoint(Vector3 worldPosition) 
		{
			float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
			float percentZ = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
			percentX = Mathf.Clamp01(percentX);
			percentZ = Mathf.Clamp01(percentZ);

			int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
			int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);
			
			return grid[x, z];
		}
		
		void OnDrawGizmos() 
		{
			Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));
			
			if (grid != null && displayGridGizmos) 
			{
				foreach (Node n in grid) 
				{
					Gizmos.color = !n.isObstructed ? Color.white : Color.red;
					Gizmos.DrawCube(n.position, Vector3.one * (nodeDiameter - 0.1f));
				}
			}
		}
	}
}