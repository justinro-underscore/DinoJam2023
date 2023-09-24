using UnityEngine;

public abstract class IManagedController : MonoBehaviour
{
    protected void Start()
    {
        if (PlayController.Instance) PlayController.Instance.RegisterManagedController(this);
        ManagedStart();
    }

    protected virtual void ManagedStart() { }

    public virtual void ManagedUpdate() { }
    public virtual void ManagedFixedUpdate() { }

    public virtual void OnStateChanged(PlayState oldState, PlayState newState) { }
}
