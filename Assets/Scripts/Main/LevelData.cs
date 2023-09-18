using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LevelData
{
    public enum StarTypes
    {
        LEVEL_COMPLETE,
        ALL_TOKENS,
        PERFECT_EGG,
        PERFECT_TIME
    }

    public int starRating;

    public bool isLocked;
    public bool isHomeBase;

    public List<bool> starData;

    public LevelData(int starRating, bool isLocked)
    {
        this.starRating = Mathf.Clamp(starRating, 0, Constants.MAX_STAR_RATING);
        this.isLocked = isLocked;

        starData = new List<bool>();
        for (int i = 0; i < Constants.MAX_STAR_RATING; i++)
        {
            starData.Add(i < starRating);
        }
    }

    public void UnlockStar(StarTypes starType)
    {
        // oh god...
        starData[(int) starType] = true;
    }

    public int GetUnlockedStars()
    {
        int total = 0;
        for (int i = 0; i < Constants.MAX_STAR_RATING; i++)
        {
            if (starData[i])
            {
                total += 1;
            }
        }

        return total;
    }
}

