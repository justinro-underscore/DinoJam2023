using System.Collections.Generic;
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

    public static PlayController instance = null;
    public PlayState State { get; private set; }

    private List<IManagedController> managedControllers = new List<IManagedController>();

    protected void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);

        State = PlayState.RUNNING;
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

        if (Input.GetKeyDown(KeyCode.Escape) && State == PlayState.PAUSE)
        {
            GameController.instance.ChangeState(GameState.LEVEL_SELECT);
        }
        if (Input.GetKeyDown(KeyCode.Backspace) && (State == PlayState.PAUSE || State == PlayState.RUNNING))
        {
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

    public void SetPlayState(PlayState newState)
    {
        if (State == newState) return;
        State = newState;
        foreach (IManagedController managedController in managedControllers)
        {
            managedController.OnStateChanged(State);
        }
    }

    public void RegisterManagedController(IManagedController controller)
    {
        managedControllers.Add(controller);
    }

    public void WinLevel()
    {
        // You can't win the level if you've already lost, sorry
        if (State == PlayState.LOSE) return;
        SetPlayState(PlayState.WIN);
        Debug.Log("WIN!");
    }

    public void LoseLevel()
    {
        // You can't lose the level if you've already won it (:think-about-it:)
        if (State == PlayState.WIN) return;
        SetPlayState(PlayState.LOSE);
        Debug.Log("LOSE!");
    }
}
