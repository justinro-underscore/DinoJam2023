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


public class LevelSelectController : ISceneController
{
    // Unlocked sprite for unlocked levels
    [SerializeField] private Sprite unlockedIconSprite;

    // List of level scriptable objects
    [SerializeField] private List<Level> levels;

    // Player icon transform on map
    [SerializeField] private Transform playerTransform;

    [SerializeField] [Range(0.5f, 10.0f)] private float playerIconSpeed = 1.0f;

    // Current level player icon is on
    private Level selectedLevel;
    private int selectedLevelIndex;

    // We are level select state
    override protected GameState GetGameState() { return GameState.LEVEL_SELECT; }

    // Init values
    protected void Start()
    {
        // Start at initial level
        // TODO: this should know what level the player just finished move the player to that level (could be helper method)
        selectedLevelIndex = 0;
        selectedLevel = levels[selectedLevelIndex];


        // TOOD: temporary stuff below
        selectedLevel.UnlockLevel(unlockedIconSprite);
        LoadLevels();
    }

    override protected void SceneUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameController.instance.ChangeState(GameState.PLAY, selectedLevel.GetSceneName());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.instance.ChangeState(GameState.MAIN_MENU);
        }

        // TODO: check for left and right arrow key press to switch between levels and call dotween on player to move between icons on map
        if (Input.GetKeyDown(KeyCode.RightArrow))
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

    // TODO: use player prefs to load which levels have been unlocked?
    public void LoadLevels()
    {
        foreach (Level level in levels)
        {
            // If level is not locked, unlock it
            if (!level.IsLevelLocked())
            {
                level.UnlockLevel(unlockedIconSprite);
            }
        }
    }
}
