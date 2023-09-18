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

    public int timeRequirement;
    public int tokenRequirement;

    public bool[] starData;

    public LevelData(bool isLocked, int tokenRequirement, int timeRequirement)
    {
        this.starRating = 0;
        this.isLocked = isLocked;
        this.tokenRequirement = tokenRequirement;
        this.timeRequirement = timeRequirement;

        starData = new bool[Constants.MAX_STAR_RATING];
    }

    public void AwardStar(StarTypes starType)
    {
        // oh god...
        starData[(int) starType] = true;
    }

    public bool IsStarUnlocked(StarTypes starType)
    {
        return starData[(int) starType];
    }

    public int GetUnlockedStars()
    {
        int total = 0;
        for (int i = 0; i < Constants.MAX_STAR_RATING; i++)
        {
            if (starData[i]) total += 1;
        }

        return total;
    }
}

