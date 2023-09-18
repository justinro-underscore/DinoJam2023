using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SelectableMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform selectIndicator;
    [SerializeField] private RectTransform menuOptionsParent;

    [Header("Settings")]
    [SerializeField] private float indicatorOffset;
    [SerializeField] [Range(1, 10)] private int indicatorShiftAmount = 1;
    [SerializeField] [Range(0.1f, 1.0f)] private float indicatorShiftTime = 0.1f;
    [SerializeField] [Range(0.01f, 0.3f)] private float selectedBlinkTime = 0.01f;
    [SerializeField] [Range(1, 8)] private int selectedBlinkAmount = 1;

    [SerializeField] private List<EventTrigger.TriggerEvent> menuOptionEvents;

    private List<RectTransform> menuOptions;
    private int numOptions;
    private int currSelectedOption;
    private float shiftTime;
    private bool shifted;
    private bool selected;
    private bool init;
    private bool active;

    // Start is called before the first frame update
    protected void Start()
    {
        menuOptions = new List<RectTransform>();
        foreach (RectTransform menuOption in menuOptionsParent)
        {
            menuOptions.Add(menuOption);
        }
        numOptions = menuOptions.Count;
        if (numOptions != menuOptionEvents.Count)
        {
            Debug.LogError("Different amount of menu options from menu option events!!!");
        }
        currSelectedOption = 0;
        selectIndicator.gameObject.SetActive(false);
        selected = false;
        init = false;
        active = false;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (!init && active)
        {
            if (menuOptions.Count == 1) init = true;
            foreach (RectTransform menuOption in menuOptions)
            {
                if (menuOption.localPosition.y != 0) init = true;
            }
            if (init)
            {
                selectIndicator.gameObject.SetActive(true);
                UpdateIndicatorPosition();
            }
        }
        if (!gameObject.activeSelf || !init || selected) return;
        
        shiftTime += Time.deltaTime;
        if (shiftTime >= indicatorShiftTime)
        {
            shifted = !shifted;
            UpdateIndicatorPosition();
            shiftTime -= indicatorShiftTime;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && numOptions > 1)
            AdjustSelectedOption(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow) && numOptions > 1)
            AdjustSelectedOption(-1);
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            SelectOption();
    }

    private void AdjustSelectedOption(int delta)
    {
        currSelectedOption += delta;
        if (currSelectedOption >= numOptions) currSelectedOption = 0;
        else if (currSelectedOption < 0) currSelectedOption = numOptions - 1;
        shiftTime = 0;
        shifted = false;
        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.MenuCursor);
        UpdateIndicatorPosition();
    }

    private void SelectOption()
    {
        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.MenuClick);
        Sequence seq = DOTween.Sequence();
        if (selectedBlinkAmount % 2 == 1) selectedBlinkAmount++;
        Image menuOptionImage = menuOptions[currSelectedOption].GetComponent<Image>();
        for (int i = 0; i < selectedBlinkAmount; i++)
        {
            seq.Append(menuOptionImage.DOColor(i % 2 == 0 ? Color.gray : Color.white, selectedBlinkTime));
        }
        seq.AppendCallback(() => {
            BaseEventData eventData = new BaseEventData(EventSystem.current) {  };
            menuOptionEvents[currSelectedOption].Invoke(eventData);
        });
        selected = true;
    }

    private void UpdateIndicatorPosition()
    {
        RectTransform selectedOption = menuOptions[currSelectedOption];
        selectIndicator.localPosition = new Vector2(-selectedOption.sizeDelta.x * 0.5f - indicatorOffset + (shifted ? indicatorShiftAmount * 2 : 0), selectedOption.localPosition.y);
    }

    public void SetActive(bool active)
    {
        this.active = active;
        if (!active)
            selectIndicator.gameObject.SetActive(false);
        else if (active && init)
            selectIndicator.gameObject.SetActive(true);
    }
}
