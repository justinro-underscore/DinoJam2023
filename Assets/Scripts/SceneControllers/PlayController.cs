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

    [SerializeField] private TimerController timerController;

    [SerializeField] private Transform startText;

    [SerializeField] private Image overlay;
    [SerializeField] private GameObject levelFailedUIParent;
    [SerializeField] private RectTransform levelFailedText;
    [SerializeField] private SelectableMenuController levelFailedMenu;

    [SerializeField] private GameObject levelClearedUIParent;
    [SerializeField] private RectTransform levelClearedText;
    [SerializeField] private RectTransform starsDataParent;
    [SerializeField] private List<Image> starImages;
    [SerializeField] private RectTransform clearedTimerParent;
    [SerializeField] private TimerController clearedGoalTimer;
    [SerializeField] private TimerController clearedTimer;
    [SerializeField] private Image clearedEggImage;
    [SerializeField] private RectTransform clearedTokenParent;
    [SerializeField] private Image clearedCollectedTokensImage;
    [SerializeField] private Image clearedTokensSlashImage;
    [SerializeField] private Image clearedTotalTokensImage;
    [SerializeField] private SelectableMenuController clearedContinueMenu;

    [Header("Variables")]
    [SerializeField] private float introCameraSize;
    [SerializeField] [Range(0.1f, 1.0f)] private float exitIrisTime = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float exitWaitTime = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float overlayOpacity;
    [SerializeField] [Range(0.01f, 0.5f)] private float overlayFadeTime = 0.01f;

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

    [Header("Lose Variables")]
    [SerializeField] [Range(0.1f, 2.0f)] private float failedMoveTime = 0.1f;

    [Header("Win Variables")]
    [SerializeField] [Range(0.1f, 1.0f)] private float clearedOverlayFadeTime = 0.1f;
    [SerializeField] [Range(0.0f, 0.5f)] private float clearedStartWaitTime;
    [SerializeField] [Range(0.1f, 2.0f)] private float clearedMoveTime = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float clearedElemStartWaitTime = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float clearedElemEndWaitTime = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] private float clearedStarsWaitTime = 0.1f;
    [SerializeField] private List<LevelData.StarTypes> starAwardOrder;
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] [Range(1.0f, 1.2f)] private float starFillScale = 1.0f;
    [SerializeField] [Range(0.01f, 0.5f)] private float starFillInTime = 0.01f;
    [SerializeField] [Range(0.01f, 0.5f)] private float starFillOutTime = 0.01f;
    [SerializeField] [Range(0.1f, 1.0f)] private float clearedElementMoveTime = 0.1f;

    [SerializeField] [Range(0.1f, 2.0f)] private float clearedLevelWaitTime = 0.1f;

    [SerializeField] [Range(0.1f, 2.0f)] private float clearedTimerCountTime = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float clearedTimerWaitTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float clearedTimerNoAwardWaitTime;
    [SerializeField] [Range(0.0f, 2.0f)] private float clearedTimerAwardWaitTime;

    [SerializeField] [Range(1.0f, 1.2f)] private float clearedEggAwardScale = 1.0f;
    [SerializeField] [Range(0.01f, 0.5f)] private float clearedEggAwardInTime = 0.01f;
    [SerializeField] [Range(0.01f, 0.5f)] private float clearedEggAwardOutTime = 0.01f;
    [SerializeField] [Range(0.0f, 1.0f)] private float clearedEggcrackShakeTime;
    [SerializeField] [Range(0.0f, 10.0f)] private float clearedEggcrackShakeStrength;
    [SerializeField] [Range(0, 40)] private int clearedEggcrackShakeVibrato;
    [SerializeField] private List<Sprite> eggCrackSprites;
    [SerializeField] [Range(0.1f, 2.0f)] private float clearedEggWaitTime = 0.1f;

    [SerializeField] List<Sprite> numberSprites;
    [SerializeField] [Range(0.1f, 2.0f)] private float clearedTokensCountTime = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float clearedTokensWaitTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float clearedTokensAwardWaitTime;

    private GameData gameData;

    private bool showFullIntro;

    private float initCameraSize;
    private float initTimerPosY;

    protected void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);

        // Debug only: if the developer has the level scene already loaded in, set the scene name as the current play scene
        gameData = GameController.instance.GetGameData();
        if (gameData.currentPlaySceneName == "")
        {
            showFullIntro = debugShowFullIntroOverride;
            gameData.currentPlaySceneName = gameObject.scene.name;
            gameData.levelData.Add(new LevelData(false, 2, 95));
        }
        else
        {
            showFullIntro = gameData.shouldShowFullLevelIntro;
        }

        State = PlayState.INTRO;
        StartIntroSequence();

        gameData.playLevelData.eggHealth = 0;
        gameData.playLevelData.levelTime = 0;
        gameData.playLevelData.tokensCollected = 0;
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

        if (State == PlayState.RUNNING)
        {
            gameData.playLevelData.levelTime += Time.deltaTime;
            timerController.SetTime(Mathf.FloorToInt(gameData.playLevelData.levelTime));
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

    public void CollectToken()
    {
        gameData.playLevelData.tokensCollected++;
    }

    public void TakeEggDamage()
    {
        gameData.playLevelData.eggHealth++;
        eggController.SetCrack(gameData.playLevelData.eggHealth);
        if (gameData.playLevelData.eggHealth >= Constants.NUM_EGG_LIVES)
        {
            eggController.BreakEgg();
            LoseLevel();
        }
    }

    public void LoseLevel()
    {
        // You can't lose the level if you've already won it or if you've already lost
        if (State == PlayState.WIN || State == PlayState.LOSE) return;
        SetPlayState(PlayState.LOSE);

        overlay.color = Color.clear;
        overlay.gameObject.SetActive(true);
        float initTextY = levelFailedText.localPosition.y;
        float textOffscreenY = (canvasRect.sizeDelta.y * 0.5f) + (levelFailedText.transform as RectTransform).sizeDelta.y;
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
        initTimerPosY = timerController.transform.localPosition.y;
        float timerPosY = (canvasRect.sizeDelta.y * 0.5f) + (timerController.transform as RectTransform).sizeDelta.y;
        timerController.transform.localPosition = new Vector2(timerController.transform.localPosition.x, timerPosY);
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

        DOTween.Sequence().Append(startText.DOLocalMoveY(1, introTextMoveTime).SetEase(Ease.OutQuad))
            .AppendCallback(() => SetPlayState(PlayState.RUNNING))
            .AppendInterval(introTextWaitTime)
            .Append(startText.DOLocalMoveY(-textOffscreenY, introTextMoveTime).SetEase(Ease.InQuad))
            .Join(timerController.transform.DOLocalMoveY(initTimerPosY, introTextMoveTime).SetEase(Ease.OutQuad))
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

    public void WinLevel()
    {
        // You can't win the level if you've already lost or if you've already won
        if (State == PlayState.LOSE || State == PlayState.WIN) return;
        SetPlayState(PlayState.WIN);

        overlay.color = Color.clear;
        overlay.gameObject.SetActive(true);
        float initTextY = levelClearedText.localPosition.y;
        float textOffscreenY = (canvasRect.sizeDelta.y * 0.5f) + (levelClearedText.transform as RectTransform).sizeDelta.y;
        levelClearedText.localPosition = new Vector2(0, textOffscreenY);
        SetClearedStars();
        float initStarsDataY = starsDataParent.localPosition.y;
        float starsOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - starsDataParent.sizeDelta.y;
        starsDataParent.localPosition = new Vector2(0, starsOffscreenY);
        levelClearedUIParent.SetActive(true);
        DOTween.Sequence().Append(overlay.DOFade(overlayOpacity, clearedOverlayFadeTime))
            .AppendInterval(clearedStartWaitTime)
            .Append(levelClearedText.DOLocalMoveY(initTextY, clearedMoveTime).SetEase(Ease.OutBounce))
            .AppendInterval(clearedStarsWaitTime)
            .Append(starsDataParent.DOLocalMoveY(initStarsDataY, clearedMoveTime * 0.5f))
            .AppendCallback(() => AwardStars());
    }

    private void SetClearedStars()
    {
        GameData gameData = GameController.instance.GetGameData();
        LevelData currentLevelData = gameData.levelData[gameData.lastPlayedLevelDataIndex];
        for (int i = 0; i < starAwardOrder.Count; i++)
        {
            if (currentLevelData.IsStarUnlocked(starAwardOrder[i]))
                starImages[i].sprite = starFilledSprite;
        }
    }

    private void AwardStars()
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < starAwardOrder.Count; i++)
        {
            AwardStar(i, starAwardOrder[i], seq);
        }

        float initMenuY = clearedContinueMenu.transform.localPosition.y;
        float menuOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - (clearedContinueMenu.transform as RectTransform).sizeDelta.y;
        clearedContinueMenu.transform.localPosition = new Vector2(0, menuOffscreenY);
        clearedContinueMenu.gameObject.SetActive(true);
        clearedContinueMenu.SetActive(false);
        seq.Append(clearedContinueMenu.transform.DOLocalMoveY(initMenuY, clearedElementMoveTime))
            .AppendCallback(() => clearedContinueMenu.SetActive(true));
    }

    private void AwardStar(int starOrderIdx, LevelData.StarTypes starType, Sequence seq)
    {
        int idx = starOrderIdx;
        GameData gameData = GameController.instance.GetGameData();
        LevelData currentLevelData = gameData.levelData[gameData.lastPlayedLevelDataIndex];
        GameData.PlayLevelData playLevelData = gameData.playLevelData;

        bool awardStar = false;
        switch (starType)
        {
            case LevelData.StarTypes.LEVEL_COMPLETE:
            {
                awardStar = true;

                seq.AppendInterval(clearedLevelWaitTime);
                break;
            }
            case LevelData.StarTypes.PERFECT_TIME:
            {
                awardStar = Mathf.FloorToInt(playLevelData.levelTime) <= currentLevelData.timeRequirement;

                clearedGoalTimer.SetTime(currentLevelData.timeRequirement);

                float initElemY = clearedTimerParent.localPosition.y;
                float elemOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - clearedTimerParent.sizeDelta.y;
                clearedTimerParent.localPosition = new Vector2(clearedTimerParent.localPosition.x, elemOffscreenY);
                clearedTimerParent.gameObject.SetActive(true);
                seq.Append(clearedTimerParent.DOLocalMoveY(initElemY, clearedElementMoveTime))
                    .AppendInterval(clearedElemStartWaitTime)
                    .Append(DOTween.To(x => clearedTimer.SetTime(Mathf.FloorToInt(x)), 0, playLevelData.levelTime, clearedTimerCountTime).SetEase(Ease.Linear))
                    .AppendInterval(clearedTimerWaitTime)
                    .AppendCallback(() => clearedGoalTimer.SetColor(awardStar ? Color.green : Color.red))
                    .AppendInterval(awardStar ? clearedTimerAwardWaitTime : clearedTimerNoAwardWaitTime);
                break;
            }
            case LevelData.StarTypes.PERFECT_EGG:
            {
                awardStar = playLevelData.eggHealth == 0;

                float initElemY = clearedEggImage.transform.localPosition.y;
                float elemOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - (clearedEggImage.transform as RectTransform).sizeDelta.y;
                clearedEggImage.transform.localPosition = new Vector2(clearedEggImage.transform.localPosition.x, elemOffscreenY);
                clearedEggImage.gameObject.SetActive(true);
                seq.Append(clearedEggImage.transform.DOLocalMoveY(initElemY, clearedElementMoveTime))
                    .AppendInterval(clearedElemStartWaitTime);
                if (awardStar)
                {
                    seq.Append(clearedEggImage.transform.DOScale(clearedEggAwardScale, clearedEggAwardInTime))
                        .Append(clearedEggImage.transform.DOScale(1, clearedEggAwardOutTime))
                        .AppendInterval(clearedEggWaitTime);
                }
                else
                {
                    seq.Append(clearedEggImage.transform.DOShakePosition(clearedEggcrackShakeTime, clearedEggcrackShakeStrength, clearedEggcrackShakeVibrato))
                        .Join(DOTween.Sequence().AppendInterval(clearedEggcrackShakeTime * 0.5f)
                            .AppendCallback(() => clearedEggImage.sprite = eggCrackSprites[playLevelData.eggHealth - 1]))
                        .AppendInterval(clearedEggWaitTime);
                }
                break;
            }
            case LevelData.StarTypes.ALL_TOKENS:
            {
                awardStar = playLevelData.tokensCollected >= currentLevelData.tokenRequirement;

                clearedTotalTokensImage.sprite = numberSprites[currentLevelData.tokenRequirement];

                float initElemY = clearedTokenParent.localPosition.y;
                float elemOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - clearedTokenParent.sizeDelta.y;
                clearedTokenParent.localPosition = new Vector2(clearedTokenParent.localPosition.x, elemOffscreenY);
                clearedTokenParent.gameObject.SetActive(true);
                seq.Append(clearedTokenParent.DOLocalMoveY(initElemY, clearedElementMoveTime))
                    .AppendInterval(clearedElemStartWaitTime);
                if (playLevelData.tokensCollected > 0)
                {
                    seq.Append(DOTween.To(x => clearedCollectedTokensImage.sprite = numberSprites[Mathf.FloorToInt(x)], 0, playLevelData.tokensCollected, clearedTokensCountTime).SetEase(Ease.Linear))
                        .AppendInterval(clearedTokensWaitTime);
                }
                seq.AppendCallback(() => {
                    Color color = awardStar ? Color.green : Color.red;
                    clearedCollectedTokensImage.color = color;
                    clearedTokensSlashImage.color = color;
                    clearedTotalTokensImage.color = color;
                })
                    .AppendInterval(clearedTokensAwardWaitTime);
                break;
            }
            default:
                break;
        }

        if (awardStar)
        {
            currentLevelData.AwardStar(starType);

            Image starImage = starImages[idx];
            seq.Append(starImage.transform.DOScale(starFillScale, starFillInTime))
                .AppendCallback(() => starImage.sprite = starFilledSprite)
                .Append(starImage.transform.DOScale(1, starFillOutTime))
                .AppendInterval(clearedElemEndWaitTime);
        }

        switch (starType)
        {
            case LevelData.StarTypes.PERFECT_TIME:
            {
                float elemOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - clearedTimerParent.sizeDelta.y;
                seq.Append(clearedTimerParent.DOLocalMoveY(elemOffscreenY, clearedElementMoveTime))
                    .AppendCallback(() => clearedTimerParent.gameObject.SetActive(false));
                break;
            }
            case LevelData.StarTypes.PERFECT_EGG:
            {
                float elemOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - (clearedEggImage.transform as RectTransform).sizeDelta.y;
                seq.Append(clearedEggImage.transform.DOLocalMoveY(elemOffscreenY, clearedElementMoveTime))
                    .AppendCallback(() => clearedEggImage.gameObject.SetActive(false));
                break;
            }
            case LevelData.StarTypes.ALL_TOKENS:
            {
                float elemOffscreenY = -(canvasRect.sizeDelta.y * 0.5f) - clearedTokenParent.sizeDelta.y;
                seq.Append(clearedTokenParent.DOLocalMoveY(elemOffscreenY, clearedElementMoveTime))
                    .AppendCallback(() => clearedTokenParent.gameObject.SetActive(false));
                break;
            }
            default:
                break;
        }
    }
}
