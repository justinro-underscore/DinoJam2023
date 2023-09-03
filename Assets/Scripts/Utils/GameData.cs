using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public int lastUnlockedLevelIndex;
    public int lastPlayedLevelIndex;

    public string currentPlaySceneName;

    public void Init()
    {
        lastUnlockedLevelIndex = 0;
        lastPlayedLevelIndex = 0;
        currentPlaySceneName = "";
    }
}
