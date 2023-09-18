using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public class PlayLevelData
    {
        public float levelTime = 0;
        // TODO Add egg health and token count
    }
    public PlayLevelData playLevelData;

    public int lastUnlockedLevelIndex;
    public int lastPlayedLevelIndex;

    public string currentPlaySceneName;

    public List<LevelData> levelData;

    public bool shouldIrisInLevelSelect;
    public bool shouldShowFullLevelIntro;

    public bool resumingGame;

    public void Init()
    {
        playLevelData = new PlayLevelData();

        lastUnlockedLevelIndex = 0;
        lastPlayedLevelIndex = 0;

        levelData = new List<LevelData>();

        currentPlaySceneName = "";

        shouldIrisInLevelSelect = false;
        shouldShowFullLevelIntro = false;

        resumingGame = false;
    }
}
