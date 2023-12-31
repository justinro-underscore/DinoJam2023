using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    UNKNOWN,
    MAIN_MENU,
    LEVEL_SELECT,
    PLAY,
    PAUSE
}

public class GameController : MonoBehaviour {
    public static GameController instance = null;

    [SerializeField] private SceneController sceneController;

    public GameState currGameState { get; private set; }

    private GameData gameData;

    public void Awake() {
        if ( null == instance ) {
            instance = this;
            DontDestroyOnLoad( this.gameObject );
        } else {
            Destroy( this.gameObject );
        }

        // Initialize game data object
        gameData = new GameData();
        gameData.Init();

        // Initialize the scene controller
        sceneController.Initialize();

        // Ensure that the base scene exists
        if (!sceneController.activeScenes.Contains(Scenes.Base))
        {
            // TODO Does this work?
            Debug.Break();
        }
        // This means that only the base scene exists, and we should create the main menu
        if (sceneController.activeScenes.Count == 1)
        {
            sceneController.LoadScene(Scenes.MainMenu, false);
            currGameState = GameState.MAIN_MENU; // Directly assign because LoadScene takes a sec
        }
        else
        {
            currGameState = sceneController.GetCurrSceneGameState();
        }
    }

    public void ChangeState(GameState newGameState)
    {
        bool handled = true;
        string nextSceneName = null;
        switch (currGameState)
        {
            case GameState.MAIN_MENU:
                if (newGameState == GameState.LEVEL_SELECT)
                {
                    sceneController.UnloadScene(Scenes.MainMenu);
                    nextSceneName = Scenes.LevelSelect;
                }
                else
                {
                    handled = false;
                }
                break;
            case GameState.LEVEL_SELECT:
                if (newGameState == GameState.PLAY)
                {
                    sceneController.UnloadScene(Scenes.LevelSelect);
                    nextSceneName = gameData.currentPlaySceneName;
                }
                else if (newGameState == GameState.MAIN_MENU)
                {
                    sceneController.UnloadScene(Scenes.LevelSelect);
                    nextSceneName = Scenes.MainMenu;
                }
                else
                {
                    handled = false;
                }
                break;
            case GameState.PLAY:
                if (newGameState == GameState.PLAY)
                {
                    sceneController.UnloadScene(gameData.currentPlaySceneName);
                    nextSceneName = gameData.currentPlaySceneName;
                }
                else if (newGameState == GameState.PAUSE)
                {
                    nextSceneName = Scenes.Pause;
                }
                else if (newGameState == GameState.LEVEL_SELECT)
                {
                    sceneController.UnloadScene(gameData.currentPlaySceneName);
                    nextSceneName = Scenes.LevelSelect;
                }
                else
                {
                    handled = false;
                }
                break;
            case GameState.PAUSE:
                if (newGameState == GameState.PLAY)
                {
                    if (!gameData.resumingGame)
                    {
                        sceneController.UnloadScene(Scenes.Pause);
                        sceneController.UnloadScene(gameData.currentPlaySceneName);
                        nextSceneName = gameData.currentPlaySceneName;
                    }
                }
                else if (newGameState == GameState.LEVEL_SELECT)
                {
                    sceneController.UnloadScene(Scenes.Pause);
                    sceneController.UnloadScene(gameData.currentPlaySceneName);
                    nextSceneName = Scenes.LevelSelect;
                }
                else
                {
                    handled = false;
                }
                break;
            default:
                handled = false;
                break;
        }
        if (!handled)
        {
            Debug.LogError($"Unknown game flow: {currGameState} -> {newGameState}");
            return;
        }

        if (nextSceneName != null)
        {
            sceneController.LoadScene(nextSceneName, false); // TODO Look into if we need to use async
        }
        Debug.Log($"Changing game state to {newGameState}");
        currGameState = newGameState;
    }

    // DO NOT USE UNLESS YOU KNOW WHAT YOU'RE DOING
    public void UnloadSceneDangerously(string sceneName)
    {
        sceneController.UnloadScene(sceneName);
    }

    public bool IsSceneLoaded(string sceneName)
    {
        return sceneController.IsSceneLoaded(sceneName);
    }

    public GameData GetGameData()
    {
        return gameData;
    }
}
