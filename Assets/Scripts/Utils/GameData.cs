using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public int lastUnlockedLevelIndex;
    public int lastPlayedLevelIndex;

    public string currentPlaySceneName;

    public bool shouldIrisInLevelSelect;
    public bool shouldShowFullLevelIntro;

    public void Init()
    {
        lastUnlockedLevelIndex = 0;
        lastPlayedLevelIndex = 0;

        // TODO: temporary until I fix this later lmao
        currentPlaySceneName = "";

        shouldIrisInLevelSelect = false;
        shouldShowFullLevelIntro = false;
    }
}
