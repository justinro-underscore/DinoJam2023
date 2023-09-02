using UnityEngine;

public class MainMenuController : ISceneController
{
    override protected GameState GetGameState() { return GameState.MAIN_MENU; }

    private void Start()
    {
    }

    override protected void SceneUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameController.instance.ChangeState(GameState.LEVEL_SELECT);
        }
    }
}
