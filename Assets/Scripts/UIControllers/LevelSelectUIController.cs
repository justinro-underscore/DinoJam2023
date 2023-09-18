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
        // Convert to images
        // TODO: work with Justin here
    }

    public void SetLevelTokens(int tokens)
    {
        // TODO: work with Justin here
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
