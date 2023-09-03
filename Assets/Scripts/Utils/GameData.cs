using System.Collections;
using System.Collections.Generic;

public class GameData
{
    private List<Level> levelData;

    private int lastPlayedLevelIndex;

    public void Init()
    {
        levelData = new List<Level>();
        lastPlayedLevelIndex = 0;
    }

    public void SetLevelData(List<Level> levelData)
    {
        this.levelData = levelData;
    }

    public List<Level> GetLevelData()
    {
        return levelData;
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
