using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private Button RertyButton;
    [SerializeField]
    private Button ReturnButton;

    // FirstNote
    [SerializeField]
    private TextMeshProUGUI GenerateTimeText;
    [SerializeField]
    private TextMeshProUGUI FinishTimeText;
    [SerializeField]
    private TextMeshProUGUI DestoryTimeText;

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
            (RertyButton, nameof(RertyButton)),
            (ReturnButton, nameof(ReturnButton)),

            (GenerateTimeText, nameof(GenerateTimeText)),
            (FinishTimeText, nameof(FinishTimeText)),
            (DestoryTimeText, nameof(DestoryTimeText))
            );
    }
}
