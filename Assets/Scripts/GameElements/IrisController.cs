using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IrisController : MonoBehaviour
{
    [SerializeField] public int IrisMaxSize;

    [SerializeField] private Image irisImage;

    public void SetActive(bool active, int newSize=-1)
    {
        if (newSize >= 0) SetIrisSize(newSize);
        gameObject.SetActive(active);
    }

    public void SetIrisSize(int size)
    {
        irisImage.rectTransform.sizeDelta = new Vector2(size, size);
    }

    public Tweener AnimateIrisIn(float time)
    {
        return AnimateIris(0, IrisMaxSize, time);
    }

    public Tweener AnimateIrisOut(float time)
    {
        return AnimateIris(IrisMaxSize, 0, time);
    }

    public Tweener AnimateIris(int startSize, int endSize, float time, bool playAudio=true)
    {
        gameObject.SetActive(true);
        if (playAudio)
        {
            if (Mathf.Abs(endSize - startSize) + 1 >= IrisMaxSize)
                AudioController.Instance.PlayOneShotAudio(endSize < startSize ? SoundEffectKeys.TromboneDown : SoundEffectKeys.TromboneUp);
            else
                AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.TromboneUpSmall);
        }
        return DOTween.To(x => SetIrisSize((int)x), startSize, endSize, time)
            .OnComplete(() => { if (endSize >= IrisMaxSize) gameObject.SetActive(false); });
    }
}
