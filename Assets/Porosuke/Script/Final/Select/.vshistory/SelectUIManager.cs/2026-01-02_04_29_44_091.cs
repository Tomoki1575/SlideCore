using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectUIManager : MonoBehaviour
{
    [SerializeField]
    private MusicDataManager MusicDataManagerClass;
    [SerializeField]
    private ScrollRectPool ScrollRectPoolClass;
    [SerializeField]
    private ThemeColorDefinition ThemeColorDefinition;

    // UIセット
    [SerializeField]
    private TextMeshProUGUI NoMatchText;
    [SerializeField]
    private TextMeshProUGUI TitleText;
    [SerializeField]
    private Image JacketImage;
    [SerializeField]
    private TextMeshProUGUI ArtistText;
    [SerializeField]
    private Button EasyButton;
    [SerializeField]
    private Button NormalButton;
    [SerializeField]
    private Button HardButton;
    [SerializeField]
    private Button PlayButton;

    // その他
    [SerializeField]
    private AudioSource BGMSource;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicDataManagerClass, nameof(MusicDataManagerClass)),
            (ScrollRectPoolClass, nameof(ScrollRectPoolClass)),
            (ThemeColorDefinition, nameof(ThemeColorDefinition)),
            (NoMatchText, nameof(NoMatchText)),
            (TitleText, nameof(TitleText)),
            (JacketImage, nameof(JacketImage)),
            (ArtistText, nameof(ArtistText)),
            (EasyButton, nameof(EasyButton)),
            (NormalButton, nameof(NormalButton)),
            (HardButton, nameof(HardButton)),
            (PlayButton, nameof(PlayButton)),

            (BGMSource, nameof(BGMSource))
        );
    }


    void Start()
    {
        MusicDataManagerClass.OnDisplayMusicDataListChanged += OnDataListChanged;
        ScrollRectPoolClass.OnSelectMusicDataChanged += OnSelectDataChanged;
        SetNoMatchTextVisible(false);

        EasyButton.onClick.AddListener(() => OnDifficultyButtonPressed(MusicDataManager.Difficulty.Easy));
        NormalButton.onClick.AddListener(() => OnDifficultyButtonPressed(MusicDataManager.Difficulty.Normal));
        HardButton.onClick.AddListener(() => OnDifficultyButtonPressed(MusicDataManager.Difficulty.Hard));
        PlayButton.onClick.AddListener(OnPlayButtonPressed);
    }

    private void OnDataListChanged()
    {
        // MusicDataManagerから新しいDataListを取得
        ScrollRectPoolClass.SetupElements(MusicDataManagerClass.CurrentDisplayMusicDataList,
                                            MusicDataManagerClass.CurrentDifficulty,
                                            GetThemeColorByDifficulty());

        SetNoMatchTextVisible(MusicDataManagerClass.CurrentDisplayMusicDataList.Count == 0);
    }

    private void OnSelectDataChanged()
    {
        // ScrollViewから現在の選択データを取得
        SetMusicDataUI(ScrollRectPoolClass.LastCenterMusic);
    }

    private Color GetThemeColorByDifficulty()
    {
        switch (MusicDataManagerClass.CurrentDifficulty)
        {
            case MusicDataManager.Difficulty.Easy:
                return ThemeColorDefinition.EasyColor;
            case MusicDataManager.Difficulty.Normal:
                return ThemeColorDefinition.NormalColor;
            case MusicDataManager.Difficulty.Hard:
                return ThemeColorDefinition.HardColor;
            default:
                return ThemeColorDefinition.EasyColor;
        }
    }

    #region UI Setter
    private void SetNoMatchTextVisible(bool bVisible)
    {
        NoMatchText.enabled = bVisible;
    }

    private void SetMusicDataUI(MusicData musicData)
    {
        if (TitleText.text != musicData.title) TitleText.text = musicData.title;
        if (JacketImage.sprite != musicData.jacketImage) JacketImage.sprite = musicData.jacketImage;
        if (ArtistText.text != musicData.artist) ArtistText.text = musicData.artist;
        bool hasEasy = musicData.chartEasy != null;
        SetButtonInteractable(EasyButton, hasEasy);
        bool hasNormal = musicData.chartNormal != null;
        SetButtonInteractable(NormalButton, hasNormal);
        bool hasHard = musicData.chartHard != null;
        SetButtonInteractable(HardButton, hasHard);
        if (BGMSource.clip != musicData.audioClip)
        {
            BGMSource.clip = musicData.audioClip;
            BGMSource.Play();
        }
    }

    private void SetButtonInteractable(Button button, bool canIntaract)
    {
        if (button.interactable != canIntaract) button.interactable = canIntaract;
    }
    #endregion

    #region UI Events
    public void OnDifficultyButtonPressed(MusicDataManager.Difficulty difficulty)
    {
        if(!ScrollRectPoolClass.IsUserInteracting()) MusicDataManagerClass.SetDifficulty(difficulty);
    }

    public void OnPlayButtonPressed()
    {

    }
    #endregion
}
