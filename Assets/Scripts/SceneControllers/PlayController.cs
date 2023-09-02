using UnityEngine;

public class PlayController : ISceneController
{
    override protected GameState GetGameState() { return GameState.PLAY; }

    private void Start()
    {
    }

    override protected void SceneUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.instance.ChangeState(GameState.LEVEL_SELECT);
        }
    }
}
