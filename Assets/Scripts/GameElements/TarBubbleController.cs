using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class TarBubbleController : MonoBehaviour
{
    [SerializeField] private int timeToLive;

    [SerializeField] [Range(0.0f, 20.0f)] private float speed;
    
    [SerializeField] private float pathAmplitudeMax;
    [SerializeField] private float pathAmplitudeMin;
    [SerializeField] private float pathPeriodMax;
    [SerializeField] private float pathPeriodMin;

    [SerializeField] private float scaleDurationMax;
    [SerializeField] private float scaleDurationMin;

    private float pathAmplitude;
    private float pathPeriod;
    private float scaleDuration;

    private Vector3 currentPosition;

    private float degrees;

    [SerializeField] private GameObject sourceTarTile;

    [SerializeField] private bool isActive = true;

    private bool hasCollided;

    void Start()
    {
        // Init values
        currentPosition = transform.position;
        pathAmplitude = Random.Range(pathAmplitudeMin, pathAmplitudeMax);
        pathPeriod = Random.Range(pathPeriodMin, pathPeriodMax);
        scaleDuration = Random.Range(scaleDurationMin, scaleDurationMax);

        // DoTween scale to (1, 1) in scaleDuration seconds
        gameObject.transform.DOScale(Vector3.one, scaleDuration);

        // Call die function after ttl has expirded
        Invoke("Die", timeToLive);

        hasCollided = false;
    }

    // Ty to https://forum.unity.com/threads/moving-along-a-sine-curve.178281/
    void Update()
    {
        if (!isActive)
        {
            return;
        }
        
        float deltaTime = Time.deltaTime;

        if (!hasCollided)
        { 
            // Move along y axis
            currentPosition.y += deltaTime * speed;
            
            float degreesPerSecond = 360.0f / pathPeriod;
            degrees = Mathf.Repeat(degrees + (deltaTime * degreesPerSecond), 360.0f);
            float radians = degrees * Mathf.Deg2Rad;
            
            // Offset by cos wave
            Vector3 offset = new Vector3(pathAmplitude * Mathf.Cos(radians), 0.0f, 0.0f);
            transform.position = currentPosition + offset;
        }
        else
        {
            // currentPosition.y -= deltaTime * speed;
            // transform.position = currentPosition;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag(Constants.playerTag) && isActive)
        {
            // Abort if player is already trapped
            if (collider.gameObject.GetComponent<PlayerController>().IsTrapped())
            {
                return;
            }

            // We have collided with the player
            // Cancel the invoke death
            CancelInvoke("Die");

            // Set the source tar tile to stop generating bubbles while player is trapped
            sourceTarTile.GetComponent<TarTileController>().SetActive(false);

            // Set player as parent
            transform.parent = collider.gameObject.transform;

            // TODO: Resize bubble to size of player

            // Stop movement
            hasCollided = true;
            currentPosition = collider.gameObject.transform.position;
            transform.position = currentPosition;
        }
    }

    public void Die()
    {
        if (!isActive)
        {
            return;
        }

        // Reenable tar tile if we set to inactive
        sourceTarTile.GetComponent<TarTileController>().SetActive(true);

        // Destroy bubble
        Destroy(gameObject);
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
    }

    public bool IsActive()
    {
        return isActive;
    }

}
