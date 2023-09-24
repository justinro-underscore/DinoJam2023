using DG.Tweening;
using UnityEngine;

public class PuppetController : ISceneController
{
    override protected GameState GetGameState() { return GameState.PUPPET; }

    [SerializeField] private IrisController irisController;
    [SerializeField] private PlayerController playerController;

    protected void Awake()
    {
        DOTween.Sequence().AppendInterval(0.2f)
            .Append(irisController.AnimateIrisIn(0.5f).SetEase(Ease.OutSine));
    }

    override protected void SceneUpdate()
    {
        playerController.ManagedUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            irisController.AnimateIrisOut(0.5f).SetEase(Ease.OutSine)
                .OnComplete(() => GameController.instance.ChangeState(GameState.MAIN_MENU));
        }
    }

    override protected void SceneFixedUpdate()
    {
        playerController.ManagedFixedUpdate();
    }
}
