using UnityEngine;
using DG.Tweening;

public class MainMenuController : ISceneController
{
    override protected GameState GetGameState() { return GameState.MAIN_MENU; }

    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform logoImage;
    [SerializeField] private SelectableMenuController startMenu;
    [SerializeField] private IrisController irisController;

    protected void Start()
    {
        AudioController.Instance.PlayMusic(MusicKeys.MenuMusic);
        
        float initLogoPosY = logoImage.localPosition.y;
        float textOffscreenY = (canvasRect.sizeDelta.y * 0.5f) + logoImage.sizeDelta.y;
        logoImage.gameObject.SetActive(true);
        logoImage.localPosition = new Vector2(0, textOffscreenY);
        DOTween.Sequence().AppendInterval(0.3f).Append(logoImage.DOLocalMoveY(initLogoPosY, 0.5f).SetEase(Ease.OutBounce));
        DOTween.Sequence().AppendInterval(0.4f).AppendCallback(() => AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.DinoFootprint));
        DOTween.Sequence().AppendInterval(1.5f).AppendCallback(() => startMenu.gameObject.SetActive(true))
            .AppendInterval(0.01f).AppendCallback(() => startMenu.SetActive(true));
    }

    public void StartGame()
    {
        DOTween.Sequence().AppendInterval(0.1f)
            .Append(irisController.AnimateIrisOut(0.4f).SetEase(Ease.OutSine))
            .AppendInterval(0.7f)
            .OnComplete(() => GameController.instance.ChangeState(GameState.LEVEL_SELECT));
    }
}
