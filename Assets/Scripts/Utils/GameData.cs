using System.Collections;
using System.Collections.Generic;

public class GameData
{
    private Dictionary<string, bool> m_LevelData;

    private int lastPlayedLevelIndex;

    public void Init()
    {
        m_LevelData = new Dictionary<string, bool>();
        lastPlayedLevelIndex = 0;
    }

    public void SetLevelData(string levelId, bool isUnlocked)
    {
        m_LevelData.Add(levelId, isUnlocked);
    }

    public void SetLastPlayedLevelIndex(int lastPlayedLevelIndex)
    {
        this.lastPlayedLevelIndex = lastPlayedLevelIndex;
    }

}
