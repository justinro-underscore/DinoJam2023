using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public int lastUnlockedLevelIndex;
    public int lastPlayedLevelIndex;

    public string currentPlaySceneName;

    public List<LevelData> levelData;

    public void Init()
    {
        lastUnlockedLevelIndex = 0;
        lastPlayedLevelIndex = 0;

        levelData = new List<LevelData>();

        // TODO: temporary until I fix this later lmao
        currentPlaySceneName = "ObstacleLevel";
    }
}
