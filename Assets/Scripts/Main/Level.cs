using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool isLocked = true;

    public Vector3 GetLevelIconLocation()
    {
        return gameObject.GetComponent<SpriteRenderer>().bounds.center;
    }

    public bool IsLevelLocked()
    {
        return isLocked;
    }

    // hrmmmm i don't like this
    public void UnlockLevel(Sprite levelIconSprite)
    {
        this.isLocked = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = levelIconSprite;
    }

    public string GetSceneName()
    {
        return sceneName;
    }

    public int GetInstanceId()
    {
        return gameObject.GetInstanceID();
    }
}
