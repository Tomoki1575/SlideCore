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


    // Pause

    // FirstNote

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
            (VirtualTimeText, nameof(VirtualTimeText))


            );
    }
}
