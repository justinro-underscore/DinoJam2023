using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class EnemyMovementController : MonoBehaviour {
    [SerializeField] [Range(0.0f, 20.0f)] public float speed = 2.0f;

    // -1 will causes path to be run forward and backwards infinite number of times
    [SerializeField] int numLoops;

    [SerializeField] bool flipX = false;
    [SerializeField] bool flipY = false;

    [SerializeField] LoopType loopType;

    public void Awake()
    {
        List<Vector3> vectorWaypoints = new List<Vector3>();
        foreach (Transform child in gameObject.transform)
        {
            if (child.tag == Constants.waypointTag)
            {
                vectorWaypoints.Add(child.position);
            }
        }

        transform
            .DOPath(vectorWaypoints.ToArray(), speed, PathType.Linear, PathMode.TopDown2D)
            .SetLoops(numLoops, loopType)
            .SetSpeedBased(true)
            .OnStepComplete(OptionalFlipSprite);
    }

    private void OptionalFlipSprite()
    {
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