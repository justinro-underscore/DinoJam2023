using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using DG.Tweening;


/**
*   Ideas
*   1. Loading level selector brings up scroll or something and dotween expands it out to reveal map (stretch)
*   2. Map is rendered with player on left side dot
*   3. You can move along lines connecting dots (dotween) IF next level is unlocked (ie. linked list action)
*   4. If there is 1 or two dots to the left and right player stays in the middle and background moves
*   5. When player presses enter, active level will be loaded by scene contoller
*/

// todo: add initialize levels
public class LevelSelectController : ISceneController
{
    // List of level objects
    // TODO: had we more time and could start over this design of where
    // levels get loaded is NOT my favourite. Terrible design.
    // Would rather move this elsewhere and reference level data but oh well
    [SerializeField] private List<Level> levels;

    // Player icon transform on map
    [SerializeField] private Transform playerTransform;

    [SerializeField] [Range(0.5f, 10.0f)] private float playerIconSpeed = 1.0f;

    // Current level player icon is on
    private Level selectedLevel;
    private int selectedLevelIndex;

    // Game data
    private GameData gameData;

    // Level data
    private List<LevelData> levelData;

    // We are level select state
    override protected GameState GetGameState() { return GameState.LEVEL_SELECT; }

    // Init values
    protected void Start()
    {
        // Get game data
        gameData = GameController.instance.GetGameData();
        levelData = gameData.levelData;

        // Initialize level data objects if they haven't been created yet
        if (levelData.Count == 0)
        {
            foreach (Level level in levels)
            {
                // ew
                levelData.Add(new LevelData(0, level.IsLevelLocked()));
            }
        }

        // Start at level saved in game data
        selectedLevelIndex = gameData.lastPlayedLevelIndex;
        selectedLevel = levels[selectedLevelIndex];
        playerTransform.position = selectedLevel.GetLevelIconLocation();

        // Update game data scene name
        gameData.currentPlaySceneName = selectedLevel.GetSceneName();

        // Initialize levels (unlocked etc.)
        LoadLevels();
    }

    override protected void SceneUpdate()
    {
        // Enter level
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Store required data and change state to play
            gameData.lastPlayedLevelIndex = selectedLevelIndex;
            gameData.currentPlaySceneName = selectedLevel.GetSceneName();
            GameController.instance.ChangeState(GameState.PLAY);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // TODO: temporary code - remove later
            GameController.instance.ChangeState(GameState.MAIN_MENU);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // If not at end of list, move to next index
            if (selectedLevelIndex != levels.Count - 1)
            {
                if (!levels[selectedLevelIndex + 1].IsLevelLocked())
                {
                    selectedLevelIndex += 1;
                    ChangeSelectedLevel();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // If not at end of list, move to previous index
            if (selectedLevelIndex != 0)
            {
                if (!levels[selectedLevelIndex - 1].IsLevelLocked())
                {
                    selectedLevelIndex -= 1;
                    ChangeSelectedLevel();
                }
            }
        }
    }

    private void ChangeSelectedLevel()
    {
        // Update selected level
        selectedLevel = levels[selectedLevelIndex];

        // Move player icon to new selected level icon
        playerTransform.DOMove(selectedLevel.GetLevelIconLocation(), playerIconSpeed, false);
    }

    // TODO: will need to see how this will work/be called if it can be once player wins a level
    public bool UnlockNextLevel()
    {
        bool unlockedLevel = false;
        if (selectedLevelIndex != levels.Count - 1)
        {
            gameData.lastUnlockedLevelIndex = selectedLevelIndex + 1;
            unlockedLevel = true;
        }

        return unlockedLevel;
    }

    public void LoadLevels()
    {
        for (int i = 0; i < levelData.Count; i++)
        {
            levels[i].LoadLevel(levelData[i]);
        }
    }
}
