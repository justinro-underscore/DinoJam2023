using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum PlayState
{
    INTRO,
    RUNNING,
    PAUSE,
    WIN,
    LOSE
}

public class PlayController : ISceneController
{
    override protected GameState GetGameState() { return GameState.PLAY; }

    public static PlayController Instance { get; private set; }
    public PlayState State { get; private set; }

    private List<IManagedController> managedControllers = new List<IManagedController>();

    [Header("References")]
    [SerializeField] private Camera playCamera;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EggController eggController;

    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private IrisController irisController;

    [SerializeField] private Transform startText;

    [SerializeField] private Image overlay;
    [SerializeField] private GameObject levelFailedUIParent;
    [SerializeField] private RectTransform levelFailedText;
    [SerializeField] private SelectableMenuController levelFailedMenu;
    [SerializeField] private GameObject winnerText;

    [Header("Variables")]
    [SerializeField] [Range(1, 5)] private int maxEggLives = 1;
    [SerializeField] private float introCameraSize;
    [SerializeField] [Range(0.1f, 1.0f)] private float exitIrisTime = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float exitWaitTime = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float overlayOpacity; // .3
    [SerializeField] [Range(0.01f, 0.5f)] private float overlayFadeTime = 0.01f; // 0.2

    [Header("Intro Variables")]
    [SerializeField] private bool debugShowFullIntroOverride;
    [SerializeField] private Vector3 introCameraStartOffset;
    [SerializeField] [Range(0.1f, 1.0f)] private float introIrisStartTime = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float shortIntroIrisTime = 0.1f;
    [SerializeField] private int introIrisStartSize;
    [SerializeField] [Range(0.0f, 1.0f)] private float introPlayerStartPauseTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float introPlayerEndPauseTime;
    [SerializeField] [Range(0.1f, 2.0f)] private float introCameraTime = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float introEndTime;
    [SerializeField] [Range(0.1f, 1.0f)] private float introTextMoveTime = 0.1f;
    [SerializeField] [Range(0.0f, 2.0f)] private float introTextWaitTime;

    [Header("Win/Lose Variables")]
    [SerializeField] [Range(0.1f, 2.0f)] private float failedMoveTime = 0.1f;

    private bool showFullIntro;

    private int eggLives;

    private float initCameraSize;

    protected void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);

        // Debug only: if the developer has the level scene already loaded in, set the scene name as the current play scene
        GameData gameData = GameController.instance.GetGameData();
        if (gameData.currentPlaySceneName == "")
        {
            showFullIntro = debugShowFullIntroOverride;
            gameData.currentPlaySceneName = gameObject.scene.name;
        }
        else
        {
            showFullIntro = gameData.shouldShowFullLevelIntro;
        }

        State = PlayState.INTRO;
        StartIntroSequence();

        eggLives = maxEggLives;
    }

    override protected void SceneUpdate()
    {
        if (State != PlayState.PAUSE)
        {
            for (int i = managedControllers.Count - 1; i >= 0; i--)
            {
                IManagedController managedController = managedControllers[i];
                if (!managedController)
                    managedControllers.RemoveAt(i);
                else
                    managedController.ManagedUpdate();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && State == PlayState.RUNNING && !GameController.instance.IsSceneLoaded(Scenes.Pause))
        {
            Pause();
        }
    }

    override protected void SceneFixedUpdate()
    {
        if (State != PlayState.PAUSE)
        {
            for (int i = managedControllers.Count - 1; i >= 0; i--)
            {
                IManagedController managedController = managedControllers[i];
                if (!managedController)
                    managedControllers.RemoveAt(i);
                else
                    managedController.ManagedFixedUpdate();
            }
        }
    }

    public void RegisterManagedController(IManagedController controller)
    {
        managedControllers.Add(controller);
    }

    public void Pause()
    {
        // Toggle dotweens!
        DOTween.PauseAll();
        SetPlayState(PlayState.PAUSE);
        GameController.instance.ChangeState(GameState.PAUSE);
    }

    public void Resume()
    {
        DOTween.PlayAll();
        SetPlayState(PlayState.RUNNING);
        overlay.DOFade(0, overlayFadeTime).OnComplete(() => overlay.gameObject.SetActive(false));
    }

    public void TakeEggDamage()
    {
        eggLives--;
        eggController.SetCrack(maxEggLives - eggLives);
        if (eggLives <= 0)
        {
            eggController.BreakEgg();
            LoseLevel();
        }
    }

    public void WinLevel()
    {
        // You can't win the level if you've already lost or if you've already won
        if (State == PlayState.LOSE || State == PlayState.WIN) return;
        SetPlayState(PlayState.WIN);
        overlay.gameObject.SetActive(true);
        winnerText.SetActive(true);
    }

    public void LoseLevel()
    {
        // You can't lose the level if you've already won it or if you've already lost
        if (State == PlayState.WIN || State == PlayState.LOSE) return;
        SetPlayState(PlayState.LOSE);

        overlay.color = Color.clear;
        overlay.gameObject.SetActive(true);
        float initTextY = levelFailedText.localPosition.y;
        float textOffscreenY = (canvasRect.sizeDelta.y * 0.5f) + (startText.transform as RectTransform).sizeDelta.y;
        levelFailedText.localPosition = new Vector2(0, textOffscreenY);
        float initMenuY = levelFailedMenu.transform.localPosition.y;
        float menuOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - (levelFailedMenu.transform as RectTransform).sizeDelta.y;
        levelFailedMenu.transform.localPosition = new Vector2(0, menuOffscreenY);
        levelFailedMenu.SetActive(false);
        levelFailedUIParent.SetActive(true);
        DOTween.Sequence().Append(overlay.DOFade(overlayOpacity, overlayFadeTime))
            .Join(levelFailedText.DOLocalMoveY(initTextY, failedMoveTime).SetEase(Ease.OutBounce))
            .Join(levelFailedMenu.transform.DOLocalMoveY(initMenuY, failedMoveTime))
            .AppendCallback(() => levelFailedMenu.SetActive(true));
    }

    private void SetPlayState(PlayState newState)
    {
        if (State == newState) return;
        PlayState oldState = State;
        State = newState;
        foreach (IManagedController managedController in managedControllers)
        {
            managedController.OnStateChanged(oldState, State);
        }
    }

    private void StartIntroSequence()
    {
        irisController.SetActive(true, 0);
        initCameraSize = playCamera.orthographicSize;
        if (showFullIntro)
        {
            playCamera.orthographicSize = introCameraSize;
            Vector3 cameraPos = playerController.transform.position + introCameraStartOffset;
            playCamera.transform.position = new Vector3(cameraPos.x, cameraPos.y, playCamera.transform.position.z);
        }

        Sequence seq = DOTween.Sequence().AppendInterval(0.2f);
        if (showFullIntro)
        {
            seq.Append(irisController.AnimateIris(0, introIrisStartSize, introIrisStartTime).SetEase(Ease.OutBack))
                .AppendInterval(introPlayerStartPauseTime)
                .AppendCallback(() => playerController.RunIntroSequence());
        }
        else
        {
            seq.Append(irisController.AnimateIrisIn(shortIntroIrisTime).SetEase(Ease.OutSine))
                .AppendCallback(() => StartGame());
        }
    }

    public void FinishIntroSequence()
    {
        DOTween.Sequence().AppendInterval(introPlayerEndPauseTime)
            .Append(playCamera.DOOrthoSize(initCameraSize, introCameraTime).SetEase(Ease.InOutQuad))
            .Join(playCamera.transform.DOMove(new Vector3(0, 0, playCamera.transform.position.z), introCameraTime).SetEase(Ease.InOutQuad))
            .Join(irisController.AnimateIris(introIrisStartSize, irisController.IrisMaxSize, introCameraTime * 0.5f).SetEase(Ease.OutSine))
            .AppendInterval(introEndTime)
            .AppendCallback(() => StartGame());
    }

    public void StartGame()
    {
        float textOffscreenY = (canvasRect.sizeDelta.y * 0.5f) + (startText.transform as RectTransform).sizeDelta.y;
        startText.localPosition = new Vector2(0, textOffscreenY);
        startText.gameObject.SetActive(true);
        SetPlayState(PlayState.RUNNING);

        DOTween.Sequence().Append(startText.DOLocalMoveY(0, introTextMoveTime).SetEase(Ease.OutQuad))
            .AppendCallback(() => SetPlayState(PlayState.RUNNING))
            .AppendInterval(introTextWaitTime)
            .Append(startText.DOLocalMoveY(-textOffscreenY, introTextMoveTime).SetEase(Ease.InQuad))
            .AppendCallback(() => startText.gameObject.SetActive(false));
    }

    public void RestartLevel()
    {
        RestartLevel(true);
    }

    public void RestartLevel(bool irisEnabled)
    {
        GameController.instance.GetGameData().resumingGame = false;
        GameController.instance.GetGameData().shouldShowFullLevelIntro = false;
        Sequence seq = DOTween.Sequence();
        if (irisEnabled)
            seq.Append(irisController.AnimateIrisOut(exitIrisTime).SetEase(Ease.OutSine));
        else
            seq.AppendInterval(exitIrisTime);
        seq.AppendInterval(exitWaitTime)
            .OnComplete(() => GameController.instance.ChangeState(GameState.PLAY));
    }

    public void QuitLevel()
    {
        QuitLevel(true);
    }

    public void QuitLevel(bool irisEnabled)
    {
        GameController.instance.GetGameData().shouldIrisInLevelSelect = true;
        Sequence seq = DOTween.Sequence();
        if (irisEnabled)
            seq.Append(irisController.AnimateIrisOut(exitIrisTime).SetEase(Ease.OutSine));
        else
            seq.AppendInterval(exitIrisTime);
        seq.AppendInterval(exitWaitTime)
            .OnComplete(() => GameController.instance.ChangeState(GameState.LEVEL_SELECT));
    }
}
