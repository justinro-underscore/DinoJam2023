using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class TarBubbleController : IManagedController
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

    [SerializeField] private TarBubbleSourceController sourceTarTile;

    [SerializeField] private bool isActive = true;

    private float startTTLTime;
    private float timeLeftToLive;

    override protected void ManagedStart()
    {
        // Init values
        currentPosition = transform.position;
        pathAmplitude = Random.Range(pathAmplitudeMin, pathAmplitudeMax);
        pathPeriod = Random.Range(pathPeriodMin, pathPeriodMax);
        scaleDuration = Random.Range(scaleDurationMin, scaleDurationMax);

        // DoTween scale to (1, 1) in scaleDuration seconds
        transform.DOScale(Vector3.one, scaleDuration);

        // Call die function after ttl has expirded
        startTTLTime = Time.time;
        timeLeftToLive = 0;
        Invoke("Die", timeToLive);
    }

    // Ty to https://forum.unity.com/threads/moving-along-a-sine-curve.178281/
    override public void ManagedUpdate()
    {
        if (!isActive)
        {
            return;
        }
        
        float deltaTime = Time.deltaTime;

        // Move along y axis
        currentPosition.y += deltaTime * speed;
        
        float degreesPerSecond = 360.0f / pathPeriod;
        degrees = Mathf.Repeat(degrees + (deltaTime * degreesPerSecond), 360.0f);
        float radians = degrees * Mathf.Deg2Rad;
        
        // Offset by cos wave
        Vector3 offset = new Vector3(pathAmplitude * Mathf.Cos(radians), 0.0f, 0.0f);
        transform.position = currentPosition + offset;
    }

    override public void OnStateChanged(PlayState oldState, PlayState newState)
    {
        if (newState == PlayState.PAUSE)
        {
            float deltaTime = Time.time - startTTLTime;

            // This check is kinda superfluous (however you spell that lmao)
            // Since we would have died otherwise and this function wouldn't be called
            // but oh well
            if (deltaTime < timeToLive)
            {
                // Stop the invoke!
                CancelInvoke("Die");

                timeLeftToLive = timeToLive - deltaTime;
            }
        }
        
        if (oldState == PlayState.PAUSE)
        {
            Invoke("Die", timeLeftToLive);
            timeLeftToLive = 0;
        }
    }


    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag(Constants.PLAYER_TAG) && isActive)
        {
            PlayerController playerController = collider.gameObject.GetComponent<PlayerController>();
            // Trap player
            playerController.TrapPlayer();

            // We have collided with the player
            // Cancel the invoke death
            CancelInvoke("Die");

            // Then die
            Die();
        }
    }

    public void Die()
    {
        if (!isActive)
        {
            return;
        }

        // Reenable tar tile if we set to inactive
        sourceTarTile.SetActive(true);

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
