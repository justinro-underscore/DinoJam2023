using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EggController : IManagedController
{
    [Header("References")]
    [SerializeField] private GameObject eggOutline;
    [SerializeField] private GameObject eggTrigger;

    [Header("Crack Values")]
    [SerializeField] [Range(0.0f, 20.0f)] private float crackThreshold;
    [SerializeField] private List<Sprite> eggCrackSprites;
    [SerializeField] [Range(0.0f, 4.0f)] private float invulnerabilityInitTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float crackShakeTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float crackShakeStrength;
    [SerializeField] [Range(0, 40)] private int crackShakeVibrato;

    private Rigidbody2D rb2d;
    private SpriteRenderer sr;
    private Transform initParent;
    private bool wasRB2DActive;
    private bool invulnerable;
    private float invulnerabilityTime;
    private bool broken;
    private float lastVelMagnitude;

    override protected void ManagedStart()
    {
        rb2d = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        initParent = transform.parent;
        // We don't want it to take damage if the player hasn't interacted with it yet
        invulnerable = true;
    }

    override public void ManagedUpdate()
    {
        if (broken) return;

        if (invulnerabilityTime > 0)
        {
            invulnerabilityTime -= Time.deltaTime;
            if (invulnerabilityTime <= 0)
            {
                invulnerable = false;
            }
        }

        // for (int i = 1; i < lastVelMagnitudes.Length; i++)
        // {
        //     lastVelMagnitudes[i - 1] = lastVelMagnitudes[i];
        // }
        lastVelMagnitude = rb2d.velocity.magnitude;
    }

    override public void OnStateChanged(PlayState oldState, PlayState newState)
    {
        if (newState == PlayState.PAUSE)
        {
            wasRB2DActive = rb2d.simulated;
            rb2d.simulated = false;
        }
        if (oldState == PlayState.PAUSE) rb2d.simulated = wasRB2DActive;
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
        invulnerable = false;
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
        if (crackNum >= eggCrackSprites.Count) crackNum = eggCrackSprites.Count - 1;
        if (crackNum > 0)
            transform.DOShakePosition(crackShakeTime, crackShakeStrength, crackShakeVibrato);
        sr.sprite = eggCrackSprites[crackNum];
    }

    public void BreakEgg()
    {
        broken = true;
        transform.DOShakePosition(crackShakeTime * 2, crackShakeStrength * 2, crackShakeVibrato);
        sr.DOFade(0, crackShakeTime * 2);
        SetEggOutlineVisible(false);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayController.Instance.State != PlayState.RUNNING && !broken) return;

        if (other.gameObject.CompareTag("Nest"))
        {
            PlayController.Instance.WinLevel();
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (PlayController.Instance.State != PlayState.RUNNING && !broken) return;

        if (collision.gameObject.CompareTag("Walls") && !invulnerable)
        {
            // Only crack the egg if the velocity is past the crack threshold
            if (lastVelMagnitude > crackThreshold)
            {
                PlayController.Instance.TakeEggDamage();
                invulnerable = true;
                invulnerabilityTime = invulnerabilityInitTime;
            }
        }
    }
}
