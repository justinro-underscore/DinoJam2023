using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUIController : MonoBehaviour
{
    [SerializeField] private GameObject levelMenu;

    [SerializeField] private Image levelPreviewImage;
    [SerializeField] private Image levelTitle;
    [SerializeField] private List<Image> levelTime;
    [SerializeField] private Image levelTokens;

    [SerializeField] private List<Image> stars;

    [SerializeField] private Image starTotal;

    [SerializeField] private Sprite starFilled;

    [SerializeField] private TimerController timerController;

    // TODO: one day make it so we can share these sprites
    [SerializeField] private List<Sprite> digitSprites;
    

    void Start()
    {
        levelMenu.SetActive(false);
    }

    public void SetLevelMenuActive(bool isActive)
    {
        levelMenu.SetActive(isActive);
    }

    public void SetLevelPreviewImage(Sprite levelPreviewSprite)
    {
        levelPreviewImage.sprite = levelPreviewSprite;
    }

    public void SetLevelTitle(Sprite levelTitleSprite)
    {
        RectTransform rectTransform = levelTitle.transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(levelTitleSprite.rect.width, rectTransform.sizeDelta.y);
        levelTitle.sprite = levelTitleSprite;
    }

    public void SetLevelTime(int seconds)
    {
        timerController.SetTime(seconds);
    }

    public void SetLevelTokens(int tokens)
    {
        levelTokens.sprite = digitSprites[Mathf.RoundToInt(Mathf.Clamp(tokens, 0, digitSprites.Count))];
    }

    public void SetLevelStars(List<bool> collectedStars)
    {
        for (int i = 0; i < stars.Count; i++)
        {
            if (collectedStars[i])
            {
                stars[i].sprite = starFilled;
            }
        }
    }

    public void SetStarTotal(int total)
    {
        // TODO: work with Justin on this
    }

}
