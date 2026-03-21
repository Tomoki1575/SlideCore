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

    // 

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (DefaultMusicWaitText, nameof(DefaultMusicWaitText))
            );
    }
}
