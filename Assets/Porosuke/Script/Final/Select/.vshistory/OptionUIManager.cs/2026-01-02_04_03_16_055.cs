using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class OptionUIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI BaseNoteSpeedText;
    [SerializeField]
    private Button BaseNoteSpeedMinus1Button;
    [SerializeField]
    private Button BaseNoteSpeedMinus01Button;
    [SerializeField]
    private Button BaseNoteSpeedMinus001Button;
    [SerializeField]
    private Button BaseNoteSpeedPlus001Button;
    [SerializeField]
    private Button BaseNoteSpeedPlus01Button;
    [SerializeField]
    private Button BaseNoteSpeedPlus1Button;

    [SerializeField]
    private TextMeshProUGUI StartOffsetText;
    [SerializeField]
    private Button StartOffsetMinus10Button;
    [SerializeField]
    private Button StartOffsetMinus1Button;
    [SerializeField]
    private Button StartOffsetPlus1Button;
    [SerializeField]
    private Button StartOffsetPlus10Button;

    [SerializeField]
    private TextMeshProUGUI JudgeOffsetText;
    [SerializeField]
    private Button JudgeOffsetMinus1Button;
    [SerializeField]
    private Button JudgeOffsetMinus01Button;
    [SerializeField]
    private Button JudgeOffsetMinus001Button;
    [SerializeField]
    private Button JudgeOffsetPlus001Button;
    [SerializeField]
    private Button JudgeOffsetPlus01Button;
    [SerializeField]
    private Button JudgeOffsetPlus1Button;

    [SerializeField]
    private TextMeshProUGUI BGMText;
    [SerializeField]
    private Slider BGMSlider;
    [SerializeField]
    private TextMeshProUGUI SEText;
    [SerializeField]
    private Slider SESlider;

    [SerializeField]
    private AudioMixer AudioMixer;

    private const float BaseNoteSpeedMin = 1;
    private const float BaseNoteSpeedMax = 12;
    private const int StartOffsetMin = 0;
    private const int StartOffsetMax = 100;
    private const float JudgeOffsetMin = -20;
    private const float JudgeOffsetMax = 20;
    private const string BGMMixerName = "MusicVolume";

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (BaseNoteSpeedText, nameof(BaseNoteSpeedText)),
            (BaseNoteSpeedMinus1Button, nameof(BaseNoteSpeedMinus1Button)),
            (BaseNoteSpeedMinus01Button, nameof(BaseNoteSpeedMinus01Button)),
            (BaseNoteSpeedMinus001Button, nameof(BaseNoteSpeedMinus001Button)),
            (BaseNoteSpeedPlus001Button, nameof(BaseNoteSpeedPlus001Button)),
            (BaseNoteSpeedPlus01Button, nameof(BaseNoteSpeedPlus01Button)),
            (BaseNoteSpeedPlus1Button, nameof(BaseNoteSpeedPlus1Button)),

            (StartOffsetText, nameof(StartOffsetText)),
            (StartOffsetMinus10Button, nameof(StartOffsetMinus10Button)),
            (StartOffsetMinus1Button, nameof(StartOffsetMinus1Button)),
            (StartOffsetPlus1Button, nameof(StartOffsetPlus1Button)),
            (StartOffsetPlus10Button, nameof(StartOffsetPlus10Button)),

            (JudgeOffsetText, nameof(JudgeOffsetText)),
            (JudgeOffsetMinus1Button, nameof(JudgeOffsetMinus1Button)),
            (JudgeOffsetMinus01Button, nameof(JudgeOffsetMinus01Button)),
            (JudgeOffsetMinus001Button, nameof(JudgeOffsetMinus001Button)),
            (JudgeOffsetPlus001Button, nameof(JudgeOffsetPlus001Button)),
            (JudgeOffsetPlus01Button, nameof(JudgeOffsetPlus01Button)),
            (JudgeOffsetPlus1Button, nameof(JudgeOffsetPlus1Button)),

            (BGMText, nameof(BGMText)),
            (BGMSlider, nameof(BGMSlider)),
            (SEText, nameof(SEText)),
            (SESlider, nameof(SESlider))
        );
    }

    private void Start()
    {
        BaseNoteSpeedMinus1Button.onClick.AddListener(() => OnBaseNoteSpeedChanged(-1f));
        BaseNoteSpeedMinus01Button.onClick.AddListener(() => OnBaseNoteSpeedChanged(-0.1f));
        BaseNoteSpeedMinus001Button.onClick.AddListener(() => OnBaseNoteSpeedChanged(-0.01f));
        BaseNoteSpeedPlus001Button.onClick.AddListener(() => OnBaseNoteSpeedChanged(0.01f));
        BaseNoteSpeedPlus01Button.onClick.AddListener(() => OnBaseNoteSpeedChanged(0.1f));
        BaseNoteSpeedPlus1Button.onClick.AddListener(() => OnBaseNoteSpeedChanged(1f));

        StartOffsetMinus10Button.onClick.AddListener(() => OnStartOffsetChanged(-10));
        StartOffsetMinus1Button.onClick.AddListener(() => OnStartOffsetChanged(-1));
        StartOffsetPlus1Button.onClick.AddListener(() => OnStartOffsetChanged(1));
        StartOffsetPlus10Button.onClick.AddListener(() => OnStartOffsetChanged(10));

        JudgeOffsetMinus1Button.onClick.AddListener(() => OnJudgeOffsetChanged(-1f));
        JudgeOffsetMinus01Button.onClick.AddListener(() => OnJudgeOffsetChanged(-0.1f));
        JudgeOffsetMinus001Button.onClick.AddListener(() => OnJudgeOffsetChanged(-0.01f));
        JudgeOffsetPlus001Button.onClick.AddListener(() => OnJudgeOffsetChanged(0.01f));
        JudgeOffsetPlus01Button.onClick.AddListener(() => OnJudgeOffsetChanged(0.1f));
        JudgeOffsetPlus1Button.onClick.AddListener(() => OnJudgeOffsetChanged(1f));

        BGMSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        SESlider.onValueChanged.AddListener(OnSESliderChanged);
    }

    private void OnBaseNoteSpeedChanged(float value)
    {
        float speed = Mathf.Clamp(OptionData.UserNoteSpeed + value, BaseNoteSpeedMin, BaseNoteSpeedMax);
        if(OptionData.UserNoteSpeed != speed)
        {
            OptionData.UserNoteSpeed = speed;
            BaseNoteSpeedText.text = speed.ToString("F2");

            bool bLower = Mathf.Approximately(speed, BaseNoteSpeedMin);
            BaseNoteSpeedMinus1Button.interactable = bLower;
            BaseNoteSpeedMinus01Button.interactable = bLower;
            BaseNoteSpeedMinus001Button.interactable = bLower;

            bool bUpper = Mathf.Approximately(speed, BaseNoteSpeedMax);
            BaseNoteSpeedPlus001Button.interactable = bUpper;
            BaseNoteSpeedPlus01Button.interactable = bUpper;
            BaseNoteSpeedPlus1Button.interactable = bUpper;
        }
    }

    private void OnStartOffsetChanged(int value)
    {
        int offset = Math.Clamp(OptionData.NoteStartOffset + value, StartOffsetMin, StartOffsetMax);
        if(OptionData.NoteStartOffset != offset)
        {
            OptionData.NoteStartOffset = offset;
            StartOffsetText.text = offset.ToString();

            bool bLower = offset == StartOffsetMin;
            StartOffsetMinus10Button.interactable = bLower;
            StartOffsetMinus1Button.interactable = bLower;

            bool bUpper = offset == StartOffsetMax;
            StartOffsetPlus1Button.interactable = bUpper;
            StartOffsetPlus10Button.interactable = bUpper;
        }
    }

    private void OnJudgeOffsetChanged(float value)
    {
        float offset = Mathf.Clamp(OptionData.JudgeOffset + value, JudgeOffsetMin, JudgeOffsetMax);
        if(OptionData.JudgeOffset != offset)
        {
            OptionData.JudgeOffset = offset;
            JudgeOffsetText.text = offset.ToString("F2");

            bool bLower = Mathf.Approximately(offset, JudgeOffsetMin);
            JudgeOffsetMinus1Button.interactable = bLower;
            JudgeOffsetMinus01Button.interactable = bLower;
            JudgeOffsetMinus001Button.interactable = bLower;

            bool bUpper = Mathf.Approximately(offset, JudgeOffsetMax);
            JudgeOffsetPlus001Button.interactable = bUpper;
            JudgeOffsetPlus01Button.interactable = bUpper;
            JudgeOffsetPlus1Button.interactable = bUpper;
        }
    }

    private void OnBGMSliderChanged(float value)
    {
        int intValue = (int)value;
        BGMText.text = intValue.ToString();
        OptionData.BGMVolume = intValue / 100f;

        AudioMixer.SetFloat(BGMMixerName, ConvertDecibel(OptionData.BGMVolume));
    }

    private float ConvertDecibel(float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = 20f * Mathf.Log10(value);
        decibel = Mathf.Clamp(decibel, -80f, 0f);
        return decibel;
    }

    private void OnSESliderChanged(float value)
    {
        int intValue = (int)value;
        SEText.text = intValue.ToString();
        OptionData.SEVolume = intValue / 100f;
    }
}
