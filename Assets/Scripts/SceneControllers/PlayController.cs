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
    [SerializeField] private Image irisImage;
    [SerializeField] private Transform startText;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject winnerText;
    [SerializeField] private GameObject loserText;

    [Header("Variables")]
    [SerializeField] [Range(1, 5)] private int maxEggLives = 1;
    [SerializeField] private float introCameraSize;

    [Header("Intro Variables")]
    [SerializeField] private bool showFullIntro;
    [SerializeField] private Vector3 introCameraStartOffset;
    [SerializeField] [Range(0.1f, 1.0f)] private float introIrisStartTime = 0.1f;
    [SerializeField] private int introIrisStartSize;
    [SerializeField] [Range(0.0f, 1.0f)] private float introPlayerStartPauseTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float introPlayerEndPauseTime;
    [SerializeField] [Range(0.1f, 2.0f)] private float introCameraTime = 0.1f;
    [SerializeField] private int introIrisEndSize;
    [SerializeField] [Range(0.0f, 1.0f)] private float introEndTime;
    [SerializeField] [Range(0.1f, 1.0f)] private float introTextMoveTime = 0.1f;
    [SerializeField] [Range(0.0f, 2.0f)] private float introTextWaitTime;

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
        if (gameData.currentPlaySceneName == "") gameData.currentPlaySceneName = gameObject.scene.name;

        State = PlayState.INTRO;
        if (showFullIntro)
            StartIntroSequence();
        else
            StartGame();

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

        if (Input.GetKeyDown(KeyCode.Escape) && State != PlayState.RUNNING)
        {
            GameController.instance.ChangeState(GameState.LEVEL_SELECT);
        }
        if (Input.GetKeyDown(KeyCode.Backspace) && (State == PlayState.PAUSE || State == PlayState.RUNNING))
        {
            // Toggle dotweens!
            DOTween.TogglePauseAll();

            // Set time scale to zero to pause invokes etc. if we are pausing game from running state
            Time.timeScale = State == PlayState.PAUSE ? 1 : 0;

            // Set play state to pause or running
            SetPlayState(State == PlayState.PAUSE ? PlayState.RUNNING : PlayState.PAUSE);
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
        overlay.SetActive(true);
        winnerText.SetActive(true);
    }

    public void LoseLevel()
    {
        // You can't lose the level if you've already won it or if you've already lost
        if (State == PlayState.WIN || State == PlayState.LOSE) return;
        SetPlayState(PlayState.LOSE);
        overlay.SetActive(true);
        loserText.SetActive(true);
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
        irisImage.gameObject.SetActive(true);
        initCameraSize = playCamera.orthographicSize;
        playCamera.orthographicSize = introCameraSize;
        Vector3 cameraPos = playerController.transform.position + introCameraStartOffset;
        playCamera.transform.position = new Vector3(cameraPos.x, cameraPos.y, playCamera.transform.position.z);

        DOTween.Sequence().AppendInterval(0.2f)
            .Append(DOTween.To(x => irisImage.rectTransform.sizeDelta = new Vector2(x, x), 0, introIrisStartSize, introIrisStartTime).SetEase(Ease.OutBack))
            .AppendInterval(introPlayerStartPauseTime)
            .AppendCallback(() => playerController.RunIntroSequence());
    }

    public void FinishIntroSequence()
    {
        DOTween.Sequence().AppendInterval(introPlayerEndPauseTime)
            .Append(playCamera.DOOrthoSize(initCameraSize, introCameraTime).SetEase(Ease.InOutQuad))
            .Join(playCamera.transform.DOMove(new Vector3(0, 0, playCamera.transform.position.z), introCameraTime).SetEase(Ease.InOutQuad))
            .Join(DOTween.To(x => irisImage.rectTransform.sizeDelta = new Vector2(x, x), introIrisStartSize, introIrisEndSize, introCameraTime * 0.5f).SetEase(Ease.OutSine))
            .AppendInterval(introEndTime)
            .AppendCallback(() => StartGame());
    }

    public void StartGame()
    {
        irisImage.gameObject.SetActive(false);
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
}
