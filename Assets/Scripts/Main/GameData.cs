using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public class PlayLevelData
    {
        public float levelTime = 0;
        public int eggHealth = 0; // 0 means not broken
        public int tokensCollected = 0;
    }
    public PlayLevelData playLevelData;

    public int lastPlayedLevelDataIndex;

    public string currentPlaySceneName;

    public List<LevelData> levelData;

    public bool shouldIrisInLevelSelect;
    public bool shouldShowFullLevelIntro;

    public bool resumingGame;

    public void Init()
    {
        playLevelData = new PlayLevelData();

        // -1 because levels in level controller has one more entry for home base
        lastPlayedLevelDataIndex = -1;

        levelData = new List<LevelData>();

        currentPlaySceneName = "";

        shouldIrisInLevelSelect = true; // True because always iris in from main menu
        shouldShowFullLevelIntro = false;

        resumingGame = false;
    }
}
