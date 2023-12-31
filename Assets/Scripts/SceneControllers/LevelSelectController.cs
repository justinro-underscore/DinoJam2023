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
    private int selectedLevelDataIndex;
    private int selectedLevelIndex;

    // We are level select state
    override protected GameState GetGameState() { return GameState.LEVEL_SELECT; }

    private bool isMovingIcon;

    // Don't look at this ;-;
    private List<LevelInfoTuple> levelList;

    private GameData gameData;

    private bool enteredLevel;

    // Init values
    protected void Start()
    {
        enteredLevel = false;

        // Tuple of level and level data for level select controller to work on
        levelList = new List<LevelInfoTuple>();

        // Get game data and level data
        gameData = GameController.instance.GetGameData();
        List<LevelData> levelData = gameData.levelData;

        // Initialize level data objects if they haven't been created yet
        // ughhh
        if (levelData.Count == 0)
        {
            foreach (Level level in levels)
            {
                LevelData data = null;
                if (!level.IsHomeBase())
                {
                    // ew
                    data = new LevelData(level.IsLevelLocked(), level.GetNumberOfTokens(), level.GetTargetLevelTime());
                    levelData.Add(data);
                }

                // Add tuple of level info to our level list
                levelList.Add(new LevelInfoTuple(level, data));
            }
        }
        else
        {
            // Zip level and level data for everything beyond home level
            levelList.Add(new LevelInfoTuple(levels[0], null));
            for (int i = 1; i < levels.Count; i++)
            {
                levelList.Add(new LevelInfoTuple(levels[i], levelData[i - 1]));
            }
        }

        // Start at level saved in game data
        selectedLevelDataIndex = gameData.lastPlayedLevelDataIndex;
        selectedLevelIndex = selectedLevelDataIndex + 1;
        selectedLevel = levelList[selectedLevelIndex];
        playerTransform.position = selectedLevel.level.GetLevelIconLocation();

        // Update game data scene name
        gameData.currentPlaySceneName = selectedLevel.level.GetSceneName();

        // Initialize levels (unlocked etc.)
        LoadLevels();

        isMovingIcon = false;

        // Show level menu if not on home base
        if (!selectedLevel.level.IsHomeBase())
        {
            UpdateLevelMenu();
        }

        // Update star total count
        levelSelectUIController.SetStarTotal(GetTotalCollectedStars());

        if (gameData.shouldIrisInLevelSelect)
        {
            gameData.shouldIrisInLevelSelect = false;
            irisController.AnimateIrisIn(irisInSpeed).SetEase(Ease.OutSine)
                .OnComplete(() => {
                    if (!AudioController.Instance.IsMusicPlaying())
                        AudioController.Instance.PlayMusic(MusicKeys.MenuMusic);
                });
        }
        else
        {
            if (!AudioController.Instance.IsMusicPlaying())
                AudioController.Instance.PlayMusic(MusicKeys.MenuMusic);
        }
    }

    override protected void SceneUpdate()
    {
        // Abort if we have entered a level
        if (enteredLevel)
        {
            return;
        }

        // Enter level
        if (Input.GetKeyDown(KeyCode.Return) && !isMovingIcon && !selectedLevel.level.IsHomeBase())
        {
            enteredLevel = true;

            // Store required data and change state to play
            gameData.lastPlayedLevelDataIndex = selectedLevelDataIndex;
            gameData.currentPlaySceneName = selectedLevel.level.GetSceneName();
            gameData.shouldShowFullLevelIntro = true;

            // Pause player animation
            playerTransform.GetComponent<Animator>().speed = 0;

            AudioController.Instance.StopMusic();

            DOTween.Sequence().Append(irisController.AnimateIrisOut(irisOutSpeed).SetEase(Ease.Linear))
                .AppendInterval(0.75f)
                .OnComplete(() => GameController.instance.ChangeState(GameState.PLAY));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && !isMovingIcon)
        {
            // If not at end of list, move to next index
            if (selectedLevelIndex != levels.Count - 1)
            {
                if (!levelList[selectedLevelIndex + 1].levelData.isLocked)
                {
                    AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.LevelSelect);
                    selectedLevelIndex += 1;
                    selectedLevelDataIndex += 1;
                    ChangeSelectedLevel(false);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMovingIcon)
        {
            // If not at end of list, move to previous index
            if (selectedLevelIndex != 0)
            {
                AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.LevelSelect);
                // Don't need to check backwards for locked as we only move forward
                selectedLevelIndex -= 1;
                selectedLevelDataIndex -= 1;
                ChangeSelectedLevel(true);
            }
        }
    }

    private void ChangeSelectedLevel(bool backwards)
    {
        // Update selected level
        selectedLevel = levelList[selectedLevelIndex];

        // We are moving icon
        isMovingIcon = true;

        // Pause player animation
        playerTransform.GetComponent<Animator>().speed = 0;

        // Disable any active level menu
        levelSelectUIController.SetLevelMenuActive(false);

        // Move player icon to new selected level icon
        playerTransform.DOPath(selectedLevel.level.GetPathWaypoints(backwards).ToArray(), playerIconSpeed, PathType.CubicBezier, PathMode.TopDown2D)
            .SetEase(Ease.Linear)
            .SetSpeedBased(true)
            .OnComplete(finishMovement);
    }

    private void finishMovement()
    {
        isMovingIcon = false;

        // TODO: create generic functions
        // Resume player animation
        playerTransform.GetComponent<Animator>().speed = 1;

        if (!selectedLevel.level.IsHomeBase())
        {
            UpdateLevelMenu();
        }
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
            if (levelList[i].levelData.isLocked && levelList[i].level.CanUnlockLevel(collectedStars) && levelList[i - 1].levelData.IsStarUnlocked(LevelData.StarTypes.LEVEL_COMPLETE))
            {
                levelList[i].level.UnlockLevel();
                levelList[i].levelData.isLocked = false;
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
