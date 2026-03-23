using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static OptionData;
using static GameDataManager;

public class GameUIManager : MonoBehaviour
{
    // TimeLine
    [SerializeField]
    private TextMeshProUGUI DefaultMusicWaitText;
    [SerializeField]
    private Slider MusicWaitSlider;
    [SerializeField]
    private TextMeshProUGUI MusicWaitText;
    [SerializeField]
    private Slider GameWaitSlider;
    [SerializeField]
    private TextMeshProUGUI GameWaitText;
    [SerializeField]
    private Slider OffsetSlider;
    [SerializeField]
    private TextMeshProUGUI OffsetText;
    [SerializeField]
    private Slider VirtualTimeSlider;
    [SerializeField]
    private TextMeshProUGUI VirtualTimeText;

    // Status
    [SerializeField]
    private TextMeshProUGUI IsStartText;
    [SerializeField]
    private TextMeshProUGUI IsPauseText;
    [SerializeField]
    private TextMeshProUGUI SpeedScaleText;
    [SerializeField]
    private TextMeshProUGUI BaseNoteSpeedText;
    [SerializeField]
    private TextMeshProUGUI CurrentNoteSpeedText;
    [SerializeField]
    private TextMeshProUGUI RemainNoteText;

    // GameUI
    [SerializeField]
    private RectTransform TotalLaneLengthRect;
    [SerializeField]
    private Slider NoteStartOffsetCoverSlider;
    [SerializeField]
    private TextMeshProUGUI NoteStartOffsetText;
    [SerializeField]
    private RectTransform LaneLengthRect;
    [SerializeField]
    private TextMeshProUGUI LaneLengthText;
    [SerializeField]
    private RectTransform LaneAfterLengthRect;
    [SerializeField]
    private TextMeshProUGUI LaneAfterLengthText;

    // Pause
    [SerializeField]
    private Button PauseButton;
    [SerializeField]
    private GameObject PausePanel;
    [SerializeField]
    private Button RetireButton;
    [SerializeField]
    private Button RetryButton;
    [SerializeField]
    private Button ReturnButton;

    // FirstNote
    [SerializeField]
    private TextMeshProUGUI GenerateTimeText;
    [SerializeField]
    private TextMeshProUGUI FinishTimeText;
    [SerializeField]
    private TextMeshProUGUI DestroyTimeText;

    public event Action OnPause;
    public event Action OnRetire;
    public event Action OnRetry;
    public event Action OnReturn;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (DefaultMusicWaitText, nameof(DefaultMusicWaitText)),
            (MusicWaitSlider, nameof(MusicWaitSlider)),
            (MusicWaitText, nameof(MusicWaitText)),
            (GameWaitSlider, nameof(GameWaitSlider)),
            (GameWaitText, nameof(GameWaitText)),
            (OffsetSlider, nameof(OffsetSlider)),
            (OffsetText, nameof(OffsetText)),
            (VirtualTimeSlider, nameof(VirtualTimeSlider)),
            (VirtualTimeText, nameof(VirtualTimeText)),

            (IsStartText, nameof(IsStartText)),
            (IsPauseText, nameof(IsPauseText)),
            (SpeedScaleText, nameof(SpeedScaleText)),
            (BaseNoteSpeedText, nameof(BaseNoteSpeedText)),
            (CurrentNoteSpeedText, nameof(CurrentNoteSpeedText)),
            (RemainNoteText, nameof(RemainNoteText)),

            (TotalLaneLengthRect, nameof(TotalLaneLengthRect)),
            (NoteStartOffsetCoverSlider, nameof(NoteStartOffsetCoverSlider)),
            (NoteStartOffsetText, nameof(NoteStartOffsetText)),
            (LaneLengthRect, nameof(LaneLengthRect)),
            (LaneLengthText, nameof(LaneLengthText)),
            (LaneAfterLengthRect, nameof(LaneAfterLengthRect)),
            (LaneAfterLengthText, nameof(LaneAfterLengthText)),

            (PauseButton, nameof(PauseButton)),
            (PausePanel, nameof(PausePanel)),
            (RetireButton, nameof(RetireButton)),
            (RetryButton, nameof(RetryButton)),
            (ReturnButton, nameof(ReturnButton)),

            (GenerateTimeText, nameof(GenerateTimeText)),
            (FinishTimeText, nameof(FinishTimeText)),
            (DestroyTimeText, nameof(DestroyTimeText))
            );

        SetPausePanel(false);

        PauseButton.onClick.AddListener(() => OnPause?.Invoke());
        RetireButton.onClick.AddListener(() => OnRetire?.Invoke());
        RetryButton.onClick.AddListener(() => OnRetry?.Invoke());
        ReturnButton.onClick.AddListener(() => OnReturn?.Invoke());
    }

    public void SetPausePanel(bool bActive)
    {
        PausePanel.SetActive(bActive);
        SetText(IsPauseText, $"IsPause : {bActive}");
    }

    public void SetConstantUI()
    {
        // ゲーム開始時に決定した後、それ以上変わらないUIをセットする
        SetText(DefaultMusicWaitText, $"(DefaultMusicWaitSec : {MusicStartWaitTime})");
        SetSliderValue(MusicWaitSlider, MusicStartWaitSec, 0, 5);
        SetText(MusicWaitText, $"MusicWaitSec : {ConvertValueToString(MusicStartWaitSec, 3)}");
        SetSliderValue(GameWaitSlider, GameWaitSec, 0, 5);
        SetText(GameWaitText, $"GameWaitSec : {ConvertValueToString(GameWaitSec, 3)}");
        SetSliderValue(OffsetSlider, Math.Abs(OffsetSec), 0, 5);
        SetText(OffsetText, $"OffsetSec : {ConvertValueToString(OffsetSec, 3)}");
        SetText(BaseNoteSpeedText, $" × BaseNoteSpeed : {ConvertValueToString(BaseNoteSpeed, 3)}");
        SetRectHeight(TotalLaneLengthRect, LaneLength + LaneAfterLength);
        SetSliderValue(NoteStartOffsetCoverSlider, NoteStartOffset, 0, 100);
        SetText(NoteStartOffsetText, $"NoteStartOffset : {NoteStartOffset}");
        SetRectHeight(LaneLengthRect, LaneLength);
        SetText(LaneLengthText, LaneLength.ToString());
        SetRectHeight(LaneAfterLengthRect, LaneAfterLength);
        SetText(LaneAfterLengthText, LaneAfterLength.ToString());
    }

    public void SetModifyUI(double virtualTime, bool bStartPlay, bool bPause, NoteData firstNoteData)
    {
        // 常に変化するUIをセットする
        SetSliderValue(VirtualTimeSlider, virtualTime, 0, 5);
        SetText(VirtualTimeText, $"VTime : {ConvertValueToString(virtualTime, 3)}");
        SetText(IsStartText, $"IsStart : {bStartPlay}");
        SpeedSection section = GetSectionAtTime(virtualTime);
        SetText(SpeedScaleText, $"SpeedScale : {ConvertValueToString(section.speedScale, 3)}");
        SetText(CurrentNoteSpeedText, $"  = CurrentNoteSpeed : {ConvertValueToString(section.speed, 3)}");
        SetText(RemainNoteText, $"RemainNoteCount : {NoteListSize}");
        if(firstNoteData != null)
        {
            SetText(GenerateTimeText, $"GenerateTime : {ConvertValueToString(firstNoteData.generateTime, 3)}");
            SetText(FinishTimeText, $"FinishTime : {ConvertValueToString(firstNoteData.finishTime, 3)}");
            SetText(DestroyTimeText, $"Destroy : {ConvertValueToString(firstNoteData.destroyTime, 3)}");
        }
        else
        {
            SetText(GenerateTimeText, $"GenerateTime : ---");
            SetText(FinishTimeText, $"FinishTime : ---");
            SetText(DestroyTimeText, $"Destroy : ---");
        }
    }

    // Utility
    private void SetText(TextMeshProUGUI tmp, string text)
    {
        if (tmp.text != text) tmp.text = text;
    }

    private void SetSliderValue(Slider slider, double value, int minValue, int maxValue)
    {
        float clampedValue = (float)Math.Clamp(value, minValue, maxValue);
        if (!Mathf.Approximately(clampedValue, slider.value)) slider.value = clampedValue;
    }

    private void SetRectHeight(RectTransform rectTransform, int height)
    {
        if(rectTransform.sizeDelta.y != height)
        {
            Vector2 size = new Vector2(rectTransform.sizeDelta.x, height);
            rectTransform.sizeDelta = size;
        }
    }

    private string ConvertValueToString(double value, int digit)
    {
        return value.ToString($"F{digit}");
    }
}
