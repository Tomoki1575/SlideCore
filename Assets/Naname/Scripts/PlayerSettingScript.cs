using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSettingsScript : MonoBehaviour
{
    [SerializeField] private Slider scrollSpeedSlider;
    [SerializeField] private Slider musicOffsetSlider;
    [SerializeField] private TextMeshProUGUI scrollSpeedText;
    [SerializeField] private TextMeshProUGUI musicOffsetText;

    private void Start()
    {
        scrollSpeedSlider.value = MoveScript.ScrollSpeed;
        musicOffsetSlider.value = NoteGenerator.MusicOffset;
        UpdateTexts();

        scrollSpeedSlider.onValueChanged.AddListener(v =>
        {
            MoveScript.ScrollSpeed = v;
            UpdateTexts();
        });

        musicOffsetSlider.onValueChanged.AddListener(v =>
        {
            NoteGenerator.MusicOffset = v;
            UpdateTexts();
        });
    }

    private void UpdateTexts()
    {
        if (scrollSpeedText != null) scrollSpeedText.text = $"ÉmÅ[ÉcÇÃë¨Ç≥: {MoveScript.ScrollSpeed:0}";
        if (musicOffsetText != null) musicOffsetText.text = $"âπÉYÉåï‚ê≥: {NoteGenerator.MusicOffset:0.000}s";
    }
}