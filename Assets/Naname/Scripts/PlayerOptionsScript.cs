using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerOptionsScript : MonoBehaviour
{
    public static PlayerOptionsScript Instance { get; private set; }


    // ノーツスピードの設定
    [SerializeField] private TextMeshProUGUI notesSpeedText;
    public static float NotesSpeedSetting { get; private set; } = 2.5f;
    private const float NotesSpeedPerSetting = 1000;
    public static float NotesSpeed => NotesSpeedSetting * NotesSpeedPerSetting;


    // オフセットの設定
    //
    // 「鳴っている音楽」と「ノーツ」のズレを直すためのオフセット
    // こっちはプレイヤーが設定する
    //
    // 「目押しではなくリズム押しをした時」に
    // lateぎみだったら値を増やして、fastぎみだったら値を減らす
    [SerializeField] private TextMeshProUGUI musicOffsetText;
    public static float MusicOffsetSetting { get; private set; } = 0f;
    private const float OffsetPerSetting = 1f / 60f;
    public static float MusicOffset => MusicOffsetSetting * OffsetPerSetting;


    // 音関連の音量設定
    [SerializeField] private AudioMixer audioMixer;

    // AudioMixer 側で公開したパラメータ名
    private const string BGMVolumeParam = "BGMVolume";
    private const string SEVolumeParam = "SEVolume";

    // 音量0のときの dB（Mixer の下限）
    private const float MinDecibel = -80f;


    // BGM音量の設定
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    public static float BGMVolumeSetting { get; private set; } = 100f;
    private const float BGMVolumePerSetting = 0.01f;
    public static float BGMVolume => BGMVolumeSetting * BGMVolumePerSetting;


    // SEの音量設定
    [SerializeField] private Slider seVolumeSlider;
    [SerializeField] private TextMeshProUGUI seVolumeText;
    public static float SEVolumeSetting { get; private set; } = 100f;
    private const float SEVolumePerSetting = 0.01f;
    public static float SEVolume => SEVolumeSetting * SEVolumePerSetting;


    // ノーツスピードの設定
    [SerializeField] private Toggle isTimingFeedbackModeToggle;
    public static bool IsTimingFeedbackMode { get; private set; } = false;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (bgmVolumeSlider != null) bgmVolumeSlider.SetValueWithoutNotify(BGMVolumeSetting);
        if (seVolumeSlider != null) seVolumeSlider.SetValueWithoutNotify(SEVolumeSetting);
        if (isTimingFeedbackModeToggle != null) isTimingFeedbackModeToggle.SetIsOnWithoutNotify(IsTimingFeedbackMode);

        RefreshSettingText();
        ApplyVolume();
    }

    /// <summary>
    /// OptionDataManagerからセーブデータをデータをロードする際、これを仲介することにより、受け取り口の範囲を絞る。
    /// (もしも仮にBGMVolumeSetting = -9999みたいなバグセーブデータを読んでしまっても制御できるようにするため)
    /// </summary>
    public static void ApplyLoadedValues(float notesSpeed, float musicOffset, float bgm, float se, bool isTimingFeedbackMode)
    {
        NotesSpeedSetting = Mathf.Clamp(notesSpeed, 1f, 10f);
        MusicOffsetSetting = Mathf.Clamp(musicOffset, -20f, 20f);
        BGMVolumeSetting = Mathf.Clamp(bgm, 0f, 100f);
        SEVolumeSetting = Mathf.Clamp(se, 0f, 100f);
        IsTimingFeedbackMode = isTimingFeedbackMode;
    }

    public void OnChangeNotesSpeed(float delta)
    {
        NotesSpeedSetting = Mathf.Clamp(NotesSpeedSetting + delta, 1f, 10f);
        RefreshSettingText();
    }

    public void OnChangeMusicOffset(float delta)
    {
        MusicOffsetSetting = Mathf.Clamp(MusicOffsetSetting + delta, -20f, 20f);
        RefreshSettingText();
    }

    /// <summary>
    /// Slider の OnValueChanged から呼ばれる
    /// </summary>
    public void OnBgmVolumeChanged(float value)
    {
        BGMVolumeSetting = value;
        RefreshSettingText();
        ApplyVolume();
    }

    /// <summary>
    /// Slider の OnValueChanged から呼ばれる
    /// </summary>
    public void OnSeVolumeChanged(float value)
    {
        SEVolumeSetting = value;
        RefreshSettingText();
        ApplyVolume();
    }

    /// <summary>
    /// 変更された値を、目に見える部分として書き換える
    /// </summary>
    private void RefreshSettingText()
    {
        if (notesSpeedText != null) notesSpeedText.text = $"{NotesSpeedSetting:0.00}";
        if (musicOffsetText != null) musicOffsetText.text = $"{MusicOffsetSetting}";
        if (bgmVolumeText != null) bgmVolumeText.text = $"{BGMVolumeSetting:0}";
        if (seVolumeText != null) seVolumeText.text = $"{SEVolumeSetting:0}";
    }

    /// <summary>
    /// Mixerに音量を送る
    /// </summary>
    private void ApplyVolume()
    {
        if (audioMixer == null)
        {
            Debug.LogError("[Unassigned] AudioMixer が割り当てられていません。", this);
            return;
        }

        audioMixer.SetFloat(BGMVolumeParam, ToDecibel(BGMVolume));
        audioMixer.SetFloat(SEVolumeParam, ToDecibel(SEVolume));
    }

    /// <summary>
    /// 倍率(0-1)を、デシベルに変換する
    /// </summary>
    public static float ToDecibel(float volume)
    {
        // Log10(0) は -∞ になってしまうので、下限で打ち止めにする
        if (volume <= 0.0001f) 
            return MinDecibel;

        return Mathf.Log10(volume) * 20f;
    }

    /// <summary>
    /// SEの音量調整sliderから指を離した際に、音量確認のためにタップノーツのSEを鳴らす
    /// </summary>
    public void OnPlayPreviewSE()
    {
        SoundEffectScript.Instance.PlayNotesSound(GameDataManager.NoteType.Tap);
    }

    /// <summary>
    /// Toggle の OnValueChanged から呼ばれる
    /// </summary>
    public void OnMirrorChartChanged(bool value)
    {
        IsTimingFeedbackMode = value;
    }
}
