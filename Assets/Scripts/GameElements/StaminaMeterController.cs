using DG.Tweening;
using UnityEngine;

public class StaminaMeterController : IManagedController
{
    [Header("References")]
    [SerializeField] private Transform staminaMeterTicker;

    [Header("Stamina Meter Collision Values")]
    [SerializeField] [Range(0.0f, 30.0f)] private float staminaMeterOffsetY;

    [Header("Stamina Meter Enter Values")]
    [SerializeField] [Range(0.0f, 1.0f)] private float fadeInTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float bounceInTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float bounceOutTime;
    [SerializeField] [Range(1.0f, 1.25f)] private float bounceInScalar = 1.0f;

    [Header("Stamina Meter Exit Values")]
    [SerializeField] [Range(0.0f, 2.0f)] private float exitTime;
    [SerializeField] [Range(0.0f, 20.0f)] private float flyOutDistance;

    [Header("Stamina Meter Collision Values")]
    [SerializeField] [Range(0.0f, 2.0f)] private float collisionTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float collisionShakeStrength;
    [SerializeField] [Range(0, 40)] private int collisionShakeVibrato;

    private SpriteRenderer[] staminaMeterSprites;

    override protected void ManagedStart()
    {
        staminaMeterSprites = new SpriteRenderer[] { GetComponent<SpriteRenderer>(), staminaMeterTicker.GetComponent<SpriteRenderer>() };
        foreach (SpriteRenderer staminaMeterSprite in staminaMeterSprites)
        {
            staminaMeterSprite.color = new Color(1, 1, 1, 0);
        }
    }

    public void SetStaminaPercentage(float val)
    {
        val = Mathf.Clamp01(val);
        float rot = ((1 - val) * 180) - 90;
        staminaMeterTicker.localEulerAngles = new Vector3(0, 0, rot);
    }

    public void UpdatePosition(Vector2 playerPos)
    {
        transform.position = new Vector2(playerPos.x, playerPos.y + staminaMeterOffsetY);
    }

    public void OnCollision()
    {
        ResetDOTweens();
        transform.DOShakePosition(collisionTime, new Vector3(collisionShakeStrength, 0, 0), collisionShakeVibrato);
    }

    public void Enter()
    {
        ResetDOTweens();
        transform.position = new Vector3(transform.position.x, 0);
        foreach (SpriteRenderer staminaMeterSprite in staminaMeterSprites)
        {
            staminaMeterSprite.DOFade(1, fadeInTime);
        }

        transform.localScale = Vector3.zero;
        DOTween.Sequence().Append(transform.DOScale(bounceInScalar, bounceInTime))
            .Append(transform.DOScale(1, bounceOutTime));
    }

    public void Exit()
    {
        ResetDOTweens();
        foreach (SpriteRenderer staminaMeterSprite in staminaMeterSprites)
        {
            staminaMeterSprite.DOFade(0, exitTime / 2);
        }

        transform.DOMoveY(-flyOutDistance, exitTime).SetEase(Ease.OutCubic)
            .OnComplete(() => transform.position = new Vector3(transform.position.x, 0));
    }

    private void ResetDOTweens()
    {
        foreach (SpriteRenderer staminaMeterSprite in staminaMeterSprites)
        {
            staminaMeterSprite.DOKill(true);
        }
        transform.DOKill(true);
    }
}
