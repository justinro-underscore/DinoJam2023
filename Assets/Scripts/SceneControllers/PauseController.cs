using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PauseController : ISceneController
{
    override protected GameState GetGameState() { return GameState.PAUSE; }

    [Header("References")]
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Image overlay;
    [SerializeField] private SelectableMenuController pauseMenu;
    [SerializeField] private IrisController irisController;

    [Header("Variables")]
    [SerializeField] [Range(0.01f, 0.5f)] private float overlayFadeTime = 0.01f;
    [SerializeField] [Range(0.1f, 1.0f)] private float enterMoveTime = 0.1f;
    [SerializeField] [Range(0.1f, 1.0f)] private float exitIrisTime = 0.1f;
    [SerializeField] [Range(0.1f, 1.0f)] private float exitMoveTime = 0.1f;

    private PlayController playController;
    private bool ready;

    protected void Start()
    {
        GameObject playControllerGameObj = GameObject.FindGameObjectWithTag(Constants.PLAY_CONTROLLER_TAG);
        if (!playControllerGameObj)
        {
            Debug.LogError("Play Controller not found!");
            return;
        }
        playController = playControllerGameObj.GetComponent<PlayController>();

        float overlayOpacity = overlay.color.a;
        overlay.color = Color.clear;
        overlay.DOFade(overlayOpacity, overlayFadeTime);
        float initPauseMenuY = pauseMenu.transform.localPosition.y;
        pauseMenu.transform.localPosition = new Vector2(0, -canvas.sizeDelta.y * 0.5f);
        pauseMenu.SetActive(false);
        pauseMenu.transform.DOLocalMoveY(initPauseMenuY, enterMoveTime).SetEase(Ease.OutSine)
            .OnComplete(() => {
                pauseMenu.SetActive(true);
                ready = true;
            });

        ready = false;
    }

    override protected void SceneUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && ready)
        {
            ResumeLevel();
        }
    }

    public void ResumeLevel()
    {
        GameController.instance.GetGameData().resumingGame = true;
        GameController.instance.ChangeState(GameState.PLAY);
        playController.Resume();
        float pauseMenuMoveY = -canvas.sizeDelta.y * 0.5f;
        overlay.DOFade(0, overlayFadeTime);
        pauseMenu.transform.DOLocalMoveY(pauseMenuMoveY, exitMoveTime)
            .OnComplete(() => GameController.instance.UnloadSceneDangerously(Scenes.Pause));
    }

    public void RestartLevel()
    {
        GameController.instance.GetGameData().resumingGame = false;
        irisController.AnimateIrisOut(exitIrisTime).SetEase(Ease.OutSine);
        playController.RestartLevel(false);
    }

    public void QuitLevel()
    {
        irisController.AnimateIrisOut(exitIrisTime).SetEase(Ease.OutSine);
        playController.QuitLevel(false);
    }
}
