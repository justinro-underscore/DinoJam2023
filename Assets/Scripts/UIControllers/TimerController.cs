using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    private const int MAX_TIME = (60 * 99) + 59;

    [Header("References")]
    [SerializeField] private RectTransform timerDigitsParent;
    [SerializeField] private List<Sprite> digitSprites;

    private List<Image> digitImages;

    protected void Start()
    {
        if (digitSprites.Count != 10)
            Debug.LogError("Invalid number of digit sprites given!");

        digitImages = new List<Image>();
        foreach (RectTransform timerDigit in timerDigitsParent)
        {
            digitImages.Add(timerDigit.GetComponent<Image>());
        }
        if (digitImages.Count != 4)
            Debug.LogError("Invalid number of digit images set!");
    }

    public void SetTime(int time)
    {
        if (time > MAX_TIME) time = MAX_TIME;
        int minutes = time / 60;
        int seconds = time % 60;
        SetDigit(digitImages[0], minutes / 10);
        SetDigit(digitImages[1], minutes % 10);
        SetDigit(digitImages[2], seconds / 10);
        SetDigit(digitImages[3], seconds % 10);
    }

    private void SetDigit(Image digitImage, int digitNum)
    {
        digitNum = Mathf.RoundToInt(Mathf.Clamp(digitNum, 0, 9));
        digitImage.sprite = digitSprites[digitNum];
    }
}
