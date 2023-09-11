using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool isLocked = true;

    // Unlocked sprite for unlocked levels and filled stars
    [SerializeField] private Sprite unlockedIconSprite;
    [SerializeField] private Sprite fullStarSprite;

    private List<GameObject> stars;

    public void Awake()
    {
        stars = new List<GameObject>();
        
        foreach (Transform child in transform)
        {
            if (child.tag == Constants.starTag)
            {
                stars.Add(child.gameObject);
            }
        }
    }

    public Vector3 GetLevelIconLocation()
    {
        return gameObject.GetComponent<SpriteRenderer>().bounds.center;
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
    }

    public void UnlockLevel()
    {
        this.isLocked = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = unlockedIconSprite;

    }

    public string GetSceneName()
    {
        return sceneName;
    }
}
