using UnityEngine;
using DG.Tweening;

public class MainMenuController : ISceneController
{
    override protected GameState GetGameState() { return GameState.MAIN_MENU; }

    [SerializeField]
    private Transform testImage;

    protected void Start()
    {
        testImage.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
    }

    override protected void SceneUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameController.instance.ChangeState(GameState.LEVEL_SELECT);
        }
    }
}
