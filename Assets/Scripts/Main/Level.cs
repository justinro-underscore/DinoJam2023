using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool isLocked = true;
    [SerializeField] private bool isHomeBase = false;

    // Unlocked sprite for unlocked levels and filled stars
    [SerializeField] private Sprite unlockedIconSprite;
    [SerializeField] private Sprite fullStarSprite;

    // Level menu details
    [SerializeField] private Sprite levelPreview;
    [SerializeField] private Sprite levelTitle;
    [SerializeField] private int numTokens;
    [SerializeField] private int targetLevelTimeSeconds;
    [SerializeField] private int numStarsToPass;

    [SerializeField] private float yOffset;

    private List<Vector3> forwardPathWaypoints;
    private List<Vector3> backPathWaypoints;

    private List<GameObject> stars;

    public void Awake()
    {
        stars = new List<GameObject>();
        forwardPathWaypoints = new List<Vector3>();
        backPathWaypoints = new List<Vector3>();
        
        foreach (Transform child in transform)
        {
            if (child.CompareTag(Constants.STAR_TAG))
            {
                stars.Add(child.gameObject);
            }

            if (child.CompareTag(Constants.WAYPOINT_TAG))
            {
                forwardPathWaypoints.Add(child.position);
            }

            if (child.CompareTag(Constants.BACKWARD_WAYPOINT_TAG))
            {
                backPathWaypoints.Add(child.position);
            }
        }
    }

    public Vector3 GetLevelIconLocation()
    {
        // return gameObject.GetComponent<SpriteRenderer>().bounds.center;
        return transform.position + new Vector3(0, yOffset, 0);
    }

    public bool CanUnlockLevel(int totalStars)
    {
        return (totalStars >= numStarsToPass); 
    }

    public bool IsLevelLocked()
    {
        return isLocked;
    }

    public void LoadLevel(LevelData levelData)
    {
        for (int i = 0; i < levelData.starRating; i++)
        {
            stars[i].GetComponent<SpriteRenderer>().sprite = fullStarSprite;
        }

        if (!levelData.isLocked)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = unlockedIconSprite;
        }
    }

    public void UnlockLevel()
    {
        isLocked = false;
    }

    public string GetSceneName()
    {
        return sceneName;
    }

    public Sprite GetLevelTitle()
    {
        return levelTitle;
    }

    public Sprite GetLevelPreview()
    {
        return levelPreview;
    }

    public int GetNumberOfTokens()
    {
        return numTokens;
    }

    public int GetTargetLevelTime()
    {
        return targetLevelTimeSeconds;
    }

    public int GetRequiredStars()
    {
        return numStarsToPass;
    }

    public bool IsHomeBase()
    {
        return isHomeBase;
    }

    public List<Vector3> GetPathWaypoints(bool backwards)
    {
        return backwards ? backPathWaypoints : forwardPathWaypoints;
    }
}
