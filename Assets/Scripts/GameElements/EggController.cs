using System.Collections.Generic;
using UnityEngine;

public class EggController : IManagedController
{
    [SerializeField] private GameObject eggOutline;
    [SerializeField] private GameObject eggTrigger;
    [SerializeField] private SpriteRenderer eggCracks;

    [SerializeField] private List<Sprite> eggCrackSprites;

    private Rigidbody2D rb2d;
    private Transform initParent;
    private bool wasRB2DActive;

    override protected void ManagedStart()
    {
        rb2d = GetComponent<Rigidbody2D>();
        initParent = transform.parent;
    }

    override public void OnStateChanged(PlayState newState)
    {
        if (newState == PlayState.PAUSE) wasRB2DActive = rb2d.simulated;
        rb2d.simulated = newState != PlayState.PAUSE ? wasRB2DActive : false;
    }

    public void SetEggOutlineVisible(bool visible)
    {
        eggOutline.SetActive(visible);
    }

    public List<PolygonCollider2D> GetEggColliders()
    {
        List<PolygonCollider2D> colliders = new List<PolygonCollider2D>();
        colliders.AddRange(GetComponents<PolygonCollider2D>());
        colliders.Add(eggTrigger.GetComponent<PolygonCollider2D>());
        return colliders;
    }

    public void GrabEgg(Transform playerTransform)
    {
        transform.parent = playerTransform;
        transform.localEulerAngles = Vector3.zero;
        transform.position = transform.position + new Vector3(0, 0.5f);
        rb2d.simulated = false;
        eggTrigger.SetActive(false);
        eggOutline.SetActive(false);
    }

    // Should only be called from the player controller
    public void DropEgg()
    {
        rb2d.simulated = true;
        eggTrigger.SetActive(true);
        transform.parent = initParent;
        transform.localEulerAngles = Vector3.zero;
    }

    public void SetCrack(int crackNum)
    {
        if (crackNum < eggCrackSprites.Count)
        {
            eggCracks.sprite = eggCrackSprites[crackNum];
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayController.instance.State != PlayState.RUNNING) return;

        if (other.gameObject.CompareTag("Nest"))
        {
            PlayController.instance.WinLevel();
        }
    }
}
