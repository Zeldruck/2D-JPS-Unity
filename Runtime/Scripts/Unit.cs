using System;
using System.Collections;
using UnityEngine;

namespace Zeldruck.JPS2D
{
    public class Unit : MonoBehaviour 
    {
        private Vector3[] path;
        private int targetIndex;
        
        [SerializeField] private Transform target;
        [SerializeField] private float speed = 20;
        [SerializeField] private bool useAstar;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound, useAstar);
            }
        }

        public void OnPathFound(Vector3[] newPath, bool pathSuccessful) 
        {
            if (pathSuccessful) 
            {
                path = newPath;
                targetIndex = 0;
                
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        }

        IEnumerator FollowPath() 
        {
            Vector3 currentWaypoint = path[0];
            
            while (true) 
            {
                if (transform.position == currentWaypoint) 
                {
                    targetIndex ++;
                    
                    if (targetIndex >= path.Length) 
                        yield break;
                    
                    currentWaypoint = path[targetIndex];
                }

                transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
                
                yield return null;
            }
        }

        public void OnDrawGizmos() 
        {
            if (path != null) 
            {
                for (int i = targetIndex; i < path.Length; i ++) 
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(path[i], Vector3.one);

                    if (i == targetIndex) 
                        Gizmos.DrawLine(transform.position, path[i]);
                    else 
                        Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}