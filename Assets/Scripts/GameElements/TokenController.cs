using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TokenController : IManagedController
{
    private Animator animator;
    private float animatorDefaultSpeed;

    [Header("Variables")]
    [SerializeField] [Range(0.0f, 1.0f)] private float collectDistance;
    [SerializeField] [Range(0.1f, 3.0f)] private float collectTime = 0.1f;
    [SerializeField] [Range(1.0f, 3.0f)] private float collectAnimatorSpeed = 1.0f;

    override protected void ManagedStart()
    {
        animator = GetComponent<Animator>();
        animatorDefaultSpeed = 1;
    }

    override public void OnStateChanged(PlayState oldState, PlayState newState)
    {
        animator.speed = newState == PlayState.PAUSE ? 0 : animatorDefaultSpeed;
    }

    public void CollectToken()
    {
        animatorDefaultSpeed = collectAnimatorSpeed;
        animator.speed = animatorDefaultSpeed;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.gray;
        DOTween.Sequence().Append(transform.DOLocalMoveY(transform.localPosition.y + collectDistance, collectTime).SetEase(Ease.Linear))
            .Join(sr.DOFade(0, collectTime).SetEase(Ease.InQuad))
            .OnComplete(() => Destroy(gameObject));
    }
}
