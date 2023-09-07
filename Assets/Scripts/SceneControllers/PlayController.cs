using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum PlayState
{
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
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EggController eggController;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject winnerText;
    [SerializeField] private GameObject loserText;

    [Header("Variables")]
    [SerializeField] [Range(1, 5)] private int maxEggLives = 1;

    private int eggLives;

    protected void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);

        State = PlayState.RUNNING;

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
            DOTween.TogglePauseAll();
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
}
