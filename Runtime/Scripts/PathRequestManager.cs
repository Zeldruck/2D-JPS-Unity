using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeldruck.JPS2D
{
    public class PathRequestManager : MonoBehaviour 
    {
        private static PathRequestManager instance;
        private Pathfinding pathfinding;
        
        private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        private PathRequest currentPathRequest;

        private bool isProcessingPath;

        void Awake() 
        {
            instance = this;
            pathfinding = GetComponent<Pathfinding>();
        }

        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, bool isAstar) 
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, isAstar);
            
            instance.pathRequestQueue.Enqueue(newRequest);
            
            instance.TryProcessNext();
        }

        void TryProcessNext() 
        {
            if (!isProcessingPath && pathRequestQueue.Count > 0) 
            {
                currentPathRequest = pathRequestQueue.Dequeue();
                
                isProcessingPath = true;
                
                pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, currentPathRequest.isAstar);
            }
        }

        public void FinishedProcessingPath(Vector3[] path, bool success) 
        {
            currentPathRequest.callback(path, success);
            isProcessingPath = false;
            
            TryProcessNext();
        }

        struct PathRequest 
        {
            public Vector3 pathStart;
            public Vector3 pathEnd;
            public Action<Vector3[], bool> callback;

            public bool isAstar;

            public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, bool _isAstar) 
            {
                pathStart = _start;
                pathEnd = _end;
                callback = _callback;
                isAstar = _isAstar;
            }
        }
    }
}