using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public int lastUnlockedLevelIndex;
    public int lastPlayedLevelIndex;

    public string currentPlaySceneName;

    public List<LevelData> levelData;

    public bool shouldIrisInLevelSelect;
    public bool shouldShowFullLevelIntro;

    public void Init()
    {
        lastUnlockedLevelIndex = 0;
        lastPlayedLevelIndex = 0;

        levelData = new List<LevelData>();

        currentPlaySceneName = "";

        shouldIrisInLevelSelect = false;
        shouldShowFullLevelIntro = false;
    }
}
