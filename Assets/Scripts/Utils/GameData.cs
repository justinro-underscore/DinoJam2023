using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public int lastUnlockedLevelIndex { get; set; }
    public int lastPlayedLevelIndex { get; set; }

    public void Init()
    {
        lastUnlockedLevelIndex = 0;
        lastPlayedLevelIndex = 0;
    }
}
