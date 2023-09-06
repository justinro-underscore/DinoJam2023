using System.Collections.Generic;
using UnityEngine;

public class PlayController : ISceneController
{
    override protected GameState GetGameState() { return GameState.PLAY; }

    public static PlayController instance = null;

    private List<IManagedController> managedControllers = new List<IManagedController>();

    public bool Active { get; private set; }

    protected void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);

        Active = true;
    }

    override protected void SceneUpdate()
    {
        if (Active)
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.instance.ChangeState(GameState.LEVEL_SELECT);
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            SetPlayActive(!Active);
        }
    }

    override protected void SceneFixedUpdate()
    {
        if (Active)
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

    public void SetPlayActive(bool active)
    {
        if (Active == active) return;
        Active = active;
        foreach (IManagedController managedController in managedControllers)
        {
            managedController.OnStateChanged(Active);
        }
    }

    public void RegisterManagedController(IManagedController controller)
    {
        managedControllers.Add(controller);
    }
}
