using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: one day maybe we can have these as the dropdown value for levels
public static class Scenes {
    public const string Base = "BaseScene";
    public const string MainMenu = "MainMenuScene";
    public const string LevelSelect = "LevelSelectScene";

    // Levels
    // TODO: temp level name
    public const string TestLevel = "ObstacleLevel";
}

public class SceneController : MonoBehaviour {
    private IDictionary<string, GameState> gameSceneStateMapping = new Dictionary<string, GameState>() {
        {Scenes.MainMenu, GameState.MAIN_MENU},
        {Scenes.LevelSelect, GameState.LEVEL_SELECT},
        {Scenes.TestLevel, GameState.PLAY}
    };

    public static SceneController instance = null;

    public List<string> activeScenes { get; private set; }

    public void Initialize() {
        activeScenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            activeScenes.Add(scene.name);
        }
    }

    public List<string> GetActiveScenes() {
        return activeScenes.GetRange(1, activeScenes.Count - 1);
    }

    public void UnloadScene(string sceneName) {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    public void LoadScene(string sceneName, bool async=true) {
        if (async) {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
        else {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    public GameState GetCurrSceneGameState()
    {
        string currSceneName = activeScenes[^1];
        if (gameSceneStateMapping.ContainsKey(currSceneName))
        {
            return gameSceneStateMapping[currSceneName];
        }
        Debug.LogError($"No game state associated with scene {{{currSceneName}}}");
        return GameState.UNKNOWN;
    }
}