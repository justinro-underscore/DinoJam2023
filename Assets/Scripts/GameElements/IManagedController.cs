using UnityEngine;

public abstract class IManagedController : MonoBehaviour
{
    protected void Start()
    {
        PlayController.instance.RegisterManagedController(this);
        ManagedStart();
    }

    protected virtual void ManagedStart() { }

    public virtual void ManagedUpdate() { }
    public virtual void ManagedFixedUpdate() { }

    public virtual void OnStateChanged(PlayState newState) { }
}
