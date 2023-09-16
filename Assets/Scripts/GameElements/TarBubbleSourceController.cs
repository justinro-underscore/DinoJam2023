using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarBubbleSourceController : IManagedController
{
    [SerializeField] private int spawnInterval;
    [SerializeField] private bool isActive;

    [SerializeField] private GameObject tarBubblePrefab;

    override protected void ManagedStart()
    {
        // Invoke spawn function every spawnInterval seconds
        InvokeRepeating("SpawnBubble", spawnInterval, spawnInterval);
    }

    private void SpawnBubble()
    {
        // Spawn if we are active
        if (isActive)
        {
            GameObject temp = Instantiate(tarBubblePrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform.parent);
            temp.GetComponent<TarBubbleController>().SetActive(true);
        }
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
    }

}