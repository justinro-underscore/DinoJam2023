using UnityEngine;

public abstract class ISceneController : MonoBehaviour
{
    protected abstract GameState GetGameState();

    protected bool SceneActive() { return GameController.instance.currGameState == GetGameState(); }

    protected void Update()
    {
        if (SceneActive())
        {
            SceneUpdate();
        }
    }

    protected void FixedUpdate()
    {
        if (SceneActive())
        {
            SceneFixedUpdate();
        }
    }

    protected virtual void SceneUpdate() { }
    protected virtual void SceneFixedUpdate() { }
}
