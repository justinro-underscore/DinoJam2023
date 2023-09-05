using DG.Tweening;
using UnityEngine;

public class GripBarController : IManagedController
{
    [Header("Grip Bar Elements")]
    [SerializeField] private SpriteRenderer gripBarOutline;
    [SerializeField] private SpriteRenderer gripBarFillOutline;
    [SerializeField] private float gripBarFillOutlineOffsetY;
    [SerializeField] private SpriteRenderer gripBarFill;
    [SerializeField] private float gripBarFillOffsetY;

    [Header("Grip Bar Enter Values")]
    [SerializeField] [Range(0.0f, 1.0f)] private float fadeInTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float bounceInTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float bounceOutTime;
    [SerializeField] [Range(1.0f, 1.25f)] private float bounceInScalar = 1.0f;

    [Header("Grip Bar Exit Values")]
    [SerializeField] [Range(0.0f, 2.0f)] private float exitTime;
    [SerializeField] [Range(0.0f, 20.0f)] private float flyOutDistance;

    [Header("Grip Bar Collision Values")]
    [SerializeField] [Range(0.0f, 2.0f)] private float collisionTime;
    [SerializeField] [Range(0.0f, 1.0f)] private float collisionShakeStrength;
    [SerializeField] [Range(0, 40)] private int collisionShakeVibrato;

    private SpriteRenderer[] gripBarSpriteRenderers;

    private float gripBarSizeY;
    private float initPosY;

    override protected void ManagedStart()
    {
        gripBarSpriteRenderers = new SpriteRenderer[] {gripBarOutline, gripBarFillOutline, gripBarFill};
        initPosY = transform.position.y;
        gripBarSizeY = gripBarOutline.size.y;
        foreach (SpriteRenderer gripBarSR in gripBarSpriteRenderers)
        {
            gripBarSR.color = new Color(1, 1, 1, 0);
        }
    }

    override public void OnStateChanged(bool active)
    {
        foreach (SpriteRenderer gripBarSR in gripBarSpriteRenderers)
        {
            gripBarSR.DOTogglePause();
        }
        transform.DOTogglePause();
    }

    public void SetGripPercentage(float val)
    {
        val = Mathf.Clamp01(val);
        float fillSizeY = val * (gripBarSizeY - gripBarFillOutlineOffsetY);
        gripBarFillOutline.size = new Vector2(gripBarFillOutline.size.x, fillSizeY);
        gripBarFill.size = new Vector2(gripBarFill.size.x, Mathf.Max(0, fillSizeY - (gripBarFillOffsetY - gripBarFillOutlineOffsetY)));
    }

    public void OnCollision()
    {
        ResetDOTweens();
        transform.DOShakePosition(collisionTime, new Vector3(collisionShakeStrength, 0, 0), collisionShakeVibrato);
    }

    public void Enter()
    {
        ResetDOTweens();
        transform.position = new Vector3(transform.position.x, initPosY);
        foreach (SpriteRenderer gripBarSR in gripBarSpriteRenderers)
        {
            gripBarSR.DOFade(1, fadeInTime);
        }

        gripBarOutline.transform.localScale = Vector3.zero;
        DOTween.Sequence().Append(transform.DOScale(bounceInScalar, bounceInTime))
            .Append(transform.DOScale(1, bounceOutTime));
    }

    public void Exit()
    {
        ResetDOTweens();
        foreach (SpriteRenderer gripBarSR in gripBarSpriteRenderers)
        {
            gripBarSR.DOFade(0, exitTime / 2);
        }

        transform.DOMoveY(-flyOutDistance, exitTime).SetEase(Ease.OutCubic)
            .OnComplete(() => transform.position = new Vector3(transform.position.x, initPosY));
    }

    private void ResetDOTweens()
    {
        foreach (SpriteRenderer gripBarSR in gripBarSpriteRenderers)
        {
            gripBarSR.DOKill(true);
        }
        transform.DOKill(true);
    }
}
