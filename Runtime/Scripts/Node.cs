using UnityEngine;

namespace Zeldruck.JPS2D
{
    public class Node : IHeapItem<Node>
    {
        public int HeapIndex { get; set; }

        public Node parent;
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;

        public bool isObstructed;
        
        public Vector3 position;
        public int gridX;
        public int gridZ;

        public Node(bool _isObstructed, Vector3 _position, int _gridX, int _gridZ)
        {
            isObstructed = _isObstructed;
            position = _position;

            gridX = _gridX;
            gridZ = _gridZ;
        }
        
        public int CompareTo(Node nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);

            if (compare == 0)
                compare = hCost.CompareTo(nodeToCompare.hCost);

            return -compare;
        }
    }
}
