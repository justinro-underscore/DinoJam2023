using UnityEngine;

public class LevelSelectController : ISceneController
{
    override protected GameState GetGameState() { return GameState.LEVEL_SELECT; }

    protected void Start()
    {
    }

    override protected void SceneUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameController.instance.ChangeState(GameState.PLAY);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.instance.ChangeState(GameState.MAIN_MENU);
        }
    }
}
