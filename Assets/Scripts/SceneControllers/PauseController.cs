using System.Collections.Generic;
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
    [SerializeField] private RectTransform levelDataElements;
    [SerializeField] private TimerController timerController;
    [SerializeField] private Image eggImage;
    [SerializeField] private RectTransform eggHearts;

    [Header("Variables")]
    [SerializeField] [Range(0.01f, 0.5f)] private float overlayFadeTime = 0.01f;
    [SerializeField] [Range(0.1f, 1.0f)] private float enterMoveTime = 0.1f;
    [SerializeField] [Range(0.1f, 1.0f)] private float exitIrisTime = 0.1f;
    [SerializeField] [Range(0.1f, 1.0f)] private float exitMoveTime = 0.1f;
    [SerializeField] List<Sprite> eggCrackedSprites;
    [SerializeField] Sprite eggHeartYolkSprite;

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

        float initLevelElementsY = levelDataElements.localPosition.y;
        levelDataElements.transform.localPosition = new Vector2(0, (canvas.sizeDelta.y * 0.5f) + levelDataElements.sizeDelta.y);
        float initPauseMenuY = pauseMenu.transform.localPosition.y;
        pauseMenu.transform.localPosition = new Vector2(0, -canvas.sizeDelta.y * 0.5f);
        pauseMenu.SetActive(false);
        DOTween.Sequence().Append(pauseMenu.transform.DOLocalMoveY(initPauseMenuY, enterMoveTime).SetEase(Ease.OutSine))
            .Join(levelDataElements.DOLocalMoveY(initLevelElementsY, enterMoveTime).SetEase(Ease.OutSine))
            .OnComplete(() => {
                pauseMenu.SetActive(true);
                ready = true;
            });

        InitData();

        ready = false;
    }

    override protected void SceneUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && ready)
        {
            ResumeLevel();
        }
    }

    private void InitData()
    {
        GameData.PlayLevelData levelData = GameController.instance.GetGameData().playLevelData;

        eggImage.sprite = eggCrackedSprites[levelData.eggHealth];
        for (int i = 0; i < levelData.eggHealth; i++)
        {
            Image eggHeart = eggHearts.GetChild(eggHearts.childCount - i - 1).GetComponent<Image>();
            eggHeart.sprite = eggHeartYolkSprite;
        }

        timerController.SetTime(Mathf.FloorToInt(levelData.levelTime));
    }

    public void ResumeLevel()
    {
        GameController.instance.GetGameData().resumingGame = true;
        GameController.instance.ChangeState(GameState.PLAY);
        playController.Resume();
        float levelDataElementsMoveY = (canvas.sizeDelta.y * 0.5f) + (levelDataElements.sizeDelta.y * 1.5f);
        float pauseMenuMoveY = -canvas.sizeDelta.y * 0.5f;
        overlay.DOFade(0, overlayFadeTime);
        DOTween.Sequence().Append(pauseMenu.transform.DOLocalMoveY(pauseMenuMoveY, exitMoveTime))
            .Join(levelDataElements.DOLocalMoveY(levelDataElementsMoveY, enterMoveTime))
            .OnComplete(() => GameController.instance.UnloadSceneDangerously(Scenes.Pause));
    }

    public void RestartLevel()
    {
        irisController.AnimateIrisOut(exitIrisTime).SetEase(Ease.OutSine);
        playController.RestartLevel(false);
    }

    public void QuitLevel()
    {
        irisController.AnimateIrisOut(exitIrisTime).SetEase(Ease.OutSine);
        playController.QuitLevel(false);
    }
}
