using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarBubbleSourceController : IManagedController
{
    [SerializeField] private int spawnInterval;
    [SerializeField] private bool isActive;

    [SerializeField] private GameObject tarBubblePrefab;

    private float startTime;
    private float timeCompleted;

    override protected void ManagedStart()
    {
        startTime = Time.time;
        timeCompleted = 0;
    }

    override public void ManagedUpdate()
    {
        float deltaTime = Time.time - startTime;

        if (deltaTime >= spawnInterval)
        {
            SpawnBubble();
            startTime = Time.time;
        }
    }

    override public void OnStateChanged(PlayState oldState, PlayState newState)
    {
        if (newState == PlayState.PAUSE)
        {
            timeCompleted = Time.time - startTime;
        }
        
        if (oldState == PlayState.PAUSE)
        {
            startTime = Time.time + timeCompleted;
            timeCompleted = 0;
        }
    }

    private void SpawnBubble()
    {
        // Spawn if we are active
        if (isActive)
        {
            GameObject temp = Instantiate(tarBubblePrefab, gameObject.transform.position + new Vector3(0, 2, 0), Quaternion.identity, gameObject.transform.parent);
            temp.GetComponent<TarBubbleController>().SetActive(true);
        }
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
    }

}