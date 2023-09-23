using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: one day maybe we can have these as the dropdown value for levels
public static class Scenes {
    public const string Base = "BaseScene";
    public const string MainMenu = "MainMenuScene";
    public const string LevelSelect = "LevelSelectScene";
    public const string Pause = "PauseScene";

    // Levels
    // TODO: temp level name
    public const string BaseLevel = "PlayScene";
    public const string JurassicJam = "JurassicJamScene";
    public const string TriassicTrial = "TriassicTrialScene";
    public const string CretaceousChaos = "CretaceousChaosScene";
    public const string MesezoicMania = "MesezoicManiaScene";
}

public class SceneController : MonoBehaviour {
    private IDictionary<string, GameState> gameSceneStateMapping = new Dictionary<string, GameState>() {
        {Scenes.MainMenu, GameState.MAIN_MENU},
        {Scenes.LevelSelect, GameState.LEVEL_SELECT},
        {Scenes.Pause, GameState.PAUSE},
        {Scenes.BaseLevel, GameState.PLAY},
        {Scenes.JurassicJam, GameState.PLAY},
        {Scenes.TriassicTrial, GameState.PLAY},
        {Scenes.CretaceousChaos, GameState.PLAY},
        {Scenes.MesezoicMania, GameState.PLAY}
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

    public bool IsSceneLoaded(string sceneName) {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene != null && scene.isLoaded;
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