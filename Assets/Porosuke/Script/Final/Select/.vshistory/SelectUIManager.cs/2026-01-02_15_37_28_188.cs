using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectUIManager : MonoBehaviour
{
    [SerializeField]
    private MusicDataManager MusicDataManagerClass;
    [SerializeField]
    private ScrollRectPool ScrollRectPoolClass;
    [SerializeField]
    private ThemeColorDefinition ThemeColorDefinitionClass;
    [SerializeField]
    private MusicSelection MusicSelectionClass;

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
    private TextMeshProUGUI EasyText;
    [SerializeField]
    private Button NormalButton;
    [SerializeField]
    private TextMeshProUGUI NormalText;
    [SerializeField]
    private Button HardButton;
    [SerializeField]
    private TextMeshProUGUI HardText;
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
            (ThemeColorDefinitionClass, nameof(ThemeColorDefinitionClass)),
            (MusicSelectionClass, nameof(MusicSelectionClass)),
            (NoMatchText, nameof(NoMatchText)),
            (TitleText, nameof(TitleText)),
            (JacketImage, nameof(JacketImage)),
            (ArtistText, nameof(ArtistText)),
            (EasyButton, nameof(EasyButton)),
            (EasyText, nameof(EasyText)),
            (NormalButton, nameof(NormalButton)),
            (NormalText, nameof(NormalText)),
            (HardButton, nameof(HardButton)),
            (HardText, nameof(HardText)),
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
                return ThemeColorDefinitionClass.EasyColor;
            case MusicDataManager.Difficulty.Normal:
                return ThemeColorDefinitionClass.NormalColor;
            case MusicDataManager.Difficulty.Hard:
                return ThemeColorDefinitionClass.HardColor;
            default:
                return ThemeColorDefinitionClass.EasyColor;
        }
    }

    #region UI Setter
    private void SetNoMatchTextVisible(bool bVisible)
    {
        NoMatchText.enabled = bVisible;
    }

    private void SetMusicDataUI(MusicData musicData)
    {
        SetText(TitleText, musicData.title);
        if (JacketImage.sprite != musicData.jacketImage) JacketImage.sprite = musicData.jacketImage;
        SetText(ArtistText, musicData.artist);
        bool hasEasy = musicData.chartEasy != null;
        SetButtonInteractable(EasyButton, hasEasy);
        SetText(EasyText, musicData.levelEasy.ToString());
        bool hasNormal = musicData.chartNormal != null;
        SetButtonInteractable(NormalButton, hasNormal);
        SetText(NormalText, musicData.levelNormal.ToString());
        bool hasHard = musicData.chartHard != null;
        SetButtonInteractable(HardButton, hasHard);
        SetText(HardText, musicData.levelHard.ToString());
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

    private void SetText(TextMeshProUGUI tmp, string text)
    {
        if (tmp.text != text) tmp.text = text;
    }
    #endregion

    #region UI Events
    public void OnDifficultyButtonPressed(MusicDataManager.Difficulty difficulty)
    {
        if(!ScrollRectPoolClass.IsUserInteracting()) MusicDataManagerClass.SetDifficulty(difficulty);
    }

    public void OnPlayButtonPressed()
    {
        MusicSelectionClass.musicData = ScrollRectPoolClass.LastCenterMusic;
        MusicSelectionClass.difficulty = MusicDataManagerClass.CurrentDifficulty;
        SceneManager.LoadScene("FinalGame");
    }
    #endregion
}
