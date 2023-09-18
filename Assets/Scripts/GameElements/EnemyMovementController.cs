using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class EnemyMovementController : IManagedController
{
    // Struct for paths
    private class Path
    {
        public List<Vector3> waypoints;
        
        public Path(List<Vector3> waypoints)
        {
            this.waypoints = waypoints;
        }
    }

    [SerializeField] [Range(0.0f, 20.0f)] public float speed = 2.0f;

    // -1 will causes path to be run forward and backwards infinite number of times
    [SerializeField] int numLoops;

    [SerializeField] bool flipX = false;
    [SerializeField] bool flipY = false;
    
    // If true, will only flip sprites once all paths have been traversed
    // else will flip between each path
    [SerializeField] bool flipAtEnd = false;

    [SerializeField] LoopType loopType;

    private List<Path> paths;

    // Using this variable instead of callback functions to avoid
    // stack depth overflow
    private bool finishedPaths;

    private int currLoopNum;

    // Need to do some funky stuff with initial position due to DOTween
    // assuming start/current position is a "waypoint"
    private Vector3 initialPosition;

    protected override void ManagedStart()
    {
        // Init list of paths
        paths = new List<Path>();

        // Find child objects with path tag
        foreach (Transform child in transform)
        {
            if (child.CompareTag(Constants.PATH_TAG))
            {
                // Find sub children with waypoint tag
                // ie. paths have waypoints
                List<Vector3> vectorWaypoints = new List<Vector3>();
                foreach (Transform subChild in child)
                {
                    if (subChild.CompareTag(Constants.WAYPOINT_TAG))
                    {
                        vectorWaypoints.Add(subChild.position);
                    }
                }

                paths.Add(new Path(vectorWaypoints));
            }
        }

        finishedPaths = true;
        currLoopNum = 0;
        initialPosition = transform.position;

        // Adding initial position as first "waypoint" in path
        // This is so we handle reverse traversals gracefull with dotween
        foreach (Path path in paths)
        {
            path.waypoints.Insert(0, initialPosition);
        }
    }

    public override void ManagedUpdate()
    {
        // Abort if we don't have paths to move on
        if (paths.Count == 0)
        {
            return;
        }

        // Only start movement if we haven't done all the requested loops
        // -1 is loop forever
        if (currLoopNum < numLoops || numLoops == -1)
        {
            if (finishedPaths)
            {
                finishedPaths = false;
                MoveAlongPath(0);
                currLoopNum++;
            }
        }
    }

    private void MoveAlongPath(int pathIndex)
    {
        Path currentPath = paths[pathIndex];

        transform
            .DOPath(currentPath.waypoints.ToArray(), speed, PathType.Linear, PathMode.TopDown2D)
            .SetSpeedBased(true)
            .OnStepComplete( () =>
                {
                    // Have we finished the last path?
                    bool wasLastPath = pathIndex == paths.Count - 1;

                    // We have finished the current path
                    // Flip sprites if we want
                    // Check we can flip between each path
                    // and we aren't at the last path (last path always tries to flip regardless)
                    if (!flipAtEnd && !wasLastPath)
                    {
                        OptionalFlipSprite();
                    }

                    // We have finished all paths
                    if (wasLastPath)
                    {
                        // Check loop type if we need to flip paths
                        // Abusing dotween's types :P
                        if (loopType == LoopType.Yoyo)
                        {
                            // Reverse paths and each individual path to traverse backwards
                            // TODO: a lower cost version would precompute the reverse in start and just toggle between them
                            paths.Reverse();
                            foreach (Path path in paths)
                            {
                                path.waypoints.Reverse();
                            }
                        }
                        else if (loopType == LoopType.Restart)
                        {
                            // Restart by moving back to start
                            transform.position = initialPosition;
                        }
                        // Else we are increment
                        // I have decided without looking anything up about what incrememnt does
                        // That we will NOT change the initial position or flip any paths
                        // And will instead just do a loop from our ending position
                        // ie. we do no more logic lmao

                        // Always try to flip sprites after traversing all paths
                        OptionalFlipSprite();

                        // We have finished
                        finishedPaths = true;
                    }
                    // If we aren't on the last path, move along next path
                    else
                    {
                        // Recursive call with next path index
                        // Depth should only reach num paths which should be fine
                        MoveAlongPath(pathIndex + 1);
                    }
                }
            );
    }

    private void OptionalFlipSprite()
    {
        // Else attempt to flip in X and Y if specified
        if (flipX)
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = !gameObject.GetComponent<SpriteRenderer>().flipX;
        }

        if (flipY)
        {
            gameObject.GetComponent<SpriteRenderer>().flipY = !gameObject.GetComponent<SpriteRenderer>().flipY;
        }
    }

}