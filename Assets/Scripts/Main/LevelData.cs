using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LevelData
{
    public int starRating;

    public bool isLocked;

    public List<bool> starData;

    public LevelData(int starRating, bool isLocked)
    {
        this.starRating = Mathf.Clamp(starRating, 0, Constants.MAX_STAR_RATING);
        this.isLocked = isLocked;

        starData = new List<bool>();
        for (int i = 0; i < Constants.MAX_STAR_RATING; i++)
        {
            starData.Add(false);
        }
    }
}

