using System.Collections;
using System.Collections.Generic;

public class GameData
{
    private int lastUnlockedLevelIndex;
    private int lastPlayedLevelIndex;

    public void Init()
    {
        lastUnlockedLevelIndex = 0;
        lastPlayedLevelIndex = 0;
    }

    public void SetLastUnlockedLevelIndex(int lastUnlockedLevelIndex)
    {
        this.lastUnlockedLevelIndex = lastUnlockedLevelIndex;
    }

    public int GetLastUnlockedLevelIndex()
    {
        return lastUnlockedLevelIndex;
    }

    public void SetLastPlayedLevelIndex(int lastPlayedLevelIndex)
    {
        this.lastPlayedLevelIndex = lastPlayedLevelIndex;
    }

    public int GetLastPlayedLevelIndex()
    {
        return lastPlayedLevelIndex;
    }

}
