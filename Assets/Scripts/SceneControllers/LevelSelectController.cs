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
    private class LevelInfoTuple
    {
        public Level level;
        public LevelData levelData;

        public LevelInfoTuple(Level level, LevelData levelData)
        {
            this.level = level;
            this.levelData = levelData;
        }
    }
    [SerializeField] private LevelSelectUIController levelSelectUIController;

    // List of level objects
    // TODO: had we more time and could start over this design of where
    // levels get loaded is NOT my favourite. Terrible design.
    // Would rather move this elsewhere and reference level data but oh well
    [SerializeField] private List<Level> levels;

    // Player icon transform on map
    [SerializeField] private Transform playerTransform;

    [SerializeField] [Range(0.5f, 10.0f)] private float playerIconSpeed = 1.0f;

    [SerializeField] private IrisController irisController;
    [SerializeField] [Range(0.1f, 1.0f)] private float irisInSpeed = 0.1f;
    [SerializeField] [Range(0.1f, 1.0f)] private float irisOutSpeed = 0.1f;

    // Current level player icon is on
    private LevelInfoTuple selectedLevel;
    private LevelData selectedLevelData;
    private int selectedLevelIndex;

    // We are level select state
    override protected GameState GetGameState() { return GameState.LEVEL_SELECT; }

    private bool isMovingIcon;

    // Don't look at this ;-;
    private List<LevelInfoTuple> levelList;

    private GameData gameData;

    // Init values
    protected void Start()
    {
        // Tuple of level and level data for level select controller to work on
        levelList = new List<LevelInfoTuple>();

        // Get game data and level data
        gameData = GameController.instance.GetGameData();
        List<LevelData> levelData = gameData.levelData;

        // Initialize level data objects if they haven't been created yet
        if (levelData.Count == 0)
        {
            foreach (Level level in levels)
            {
                LevelData data = null;
                if (!level.IsHomeBase())
                {
                    // ew
                    data = new LevelData(0, level.IsLevelLocked(), level.GetNumberOfTokens(), level.GetTargetLevelTime());
                    levelData.Add(data);
                }

                // Add tuple of level info to our level list
                levelList.Add(new LevelInfoTuple(level, data));
            }
        }

        // Start at level saved in game data
        selectedLevelIndex = gameData.lastPlayedLevelIndex;
        selectedLevel = levelList[selectedLevelIndex];
        playerTransform.position = selectedLevel.level.GetLevelIconLocation();

        // Update game data scene name
        gameData.currentPlaySceneName = selectedLevel.level.GetSceneName();

        // Initialize levels (unlocked etc.)
        LoadLevels();

        isMovingIcon = false;

        if (gameData.shouldIrisInLevelSelect)
        {
            gameData.shouldIrisInLevelSelect = false;
            irisController.AnimateIrisIn(irisInSpeed).SetEase(Ease.OutSine);
        }
    }

    override protected void SceneUpdate()
    {
        // Enter level
        if (Input.GetKeyDown(KeyCode.Return) && !isMovingIcon)
        {
            // Store required data and change state to play
            gameData.lastPlayedLevelIndex = selectedLevelIndex;
            gameData.currentPlaySceneName = selectedLevel.level.GetSceneName();
            gameData.shouldShowFullLevelIntro = true;

            DOTween.Sequence().Append(irisController.AnimateIrisOut(irisOutSpeed).SetEase(Ease.Linear))
                .AppendInterval(0.75f)
                .OnComplete(() => GameController.instance.ChangeState(GameState.PLAY));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // TODO: temporary code - remove later
            GameController.instance.ChangeState(GameState.MAIN_MENU);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && !isMovingIcon)
        {
            // If not at end of list, move to next index
            if (selectedLevelIndex != levels.Count - 1)
            {
                if (!levelList[selectedLevelIndex + 1].levelData.isLocked)
                {
                    selectedLevelIndex += 1;
                    ChangeSelectedLevel();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMovingIcon)
        {
            // If not at end of list, move to previous index
            if (selectedLevelIndex != 0)
            {
                // Don't need to check backwards for locked as we only move forward
                selectedLevelIndex -= 1;
                ChangeSelectedLevel();
            }
        }
    }

    private void ChangeSelectedLevel()
    {
        // Update selected level
        selectedLevel = levelList[selectedLevelIndex];

        // We are moving icon
        isMovingIcon = true;

        // Disable any active level menu
        levelSelectUIController.SetLevelMenuActive(false);

        // Move player icon to new selected level icon
        playerTransform.DOMove(selectedLevel.level.GetLevelIconLocation(), playerIconSpeed, false)
            .OnComplete(() =>
                {
                    isMovingIcon = false;

                    if (!selectedLevel.level.IsHomeBase())
                    {
                        UpdateLevelMenu();
                    }
                }
            );
    }

    private void UpdateLevelMenu()
    {
        // Populate level menu with new data
        // TODO: Debatable whether this should be here or ui controller
        levelSelectUIController.SetLevelTitle(selectedLevel.level.GetLevelTitle());
        levelSelectUIController.SetLevelPreviewImage(selectedLevel.level.GetLevelPreview());
        levelSelectUIController.SetLevelTime(selectedLevel.level.GetTargetLevelTime());
        levelSelectUIController.SetLevelTokens(selectedLevel.level.GetNumberOfTokens());
        levelSelectUIController.SetLevelStars(selectedLevel.levelData.starData);

        // Display level menu
        levelSelectUIController.SetLevelMenuActive(true);
    }

    public void LoadLevels()
    {
        int collectedStars = GetTotalCollectedStars();

        // Skip home level
        for (int i = 1; i < levelList.Count; i++)
        {
            // If we are locked check if we can unlock
            // TODO: move this unlock level check to level data
            if (levelList[i].levelData.isLocked && levelList[i].level.CanUnlockLevel(collectedStars))
            {
                levelList[i].level.UnlockLevel();
            }

            levelList[i].level.LoadLevel(levelList[i].levelData);
        }
    }

    public int GetTotalCollectedStars()
    {
        // TODO: could do a map but ehh
        int total = 0;
        foreach (LevelInfoTuple levelInfo in levelList)
        {
            if (!levelInfo.level.IsHomeBase())
            {
                total += levelInfo.levelData.GetUnlockedStars();   
            }
        }

        return total;
    }
}
