using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MusicSelectCoreScript : MonoBehaviour
{
    [SerializeField] private MusicDataList musicDataList;
    [SerializeField] private MusicSelection currentSelection;

    [Header("難易度ボタン")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    [Header("曲情報")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI artistText;

    [Header("難易度ラベル")]
    [SerializeField] private TextMeshProUGUI easyLabelText;
    [SerializeField] private TextMeshProUGUI normalLabelText;
    [SerializeField] private TextMeshProUGUI hardLabelText;

    [Header("曲リスト")]
    [SerializeField] private GameObject songItemPrefab;
    [SerializeField] private Transform contentParent;

    private int currentIndex = 0;

    private void Start()
    {
        easyButton.onClick.AddListener(() => StartGame(MusicDataManager.Difficulty.Easy));
        normalButton.onClick.AddListener(() => StartGame(MusicDataManager.Difficulty.Normal));
        hardButton.onClick.AddListener(() => StartGame(MusicDataManager.Difficulty.Hard));

        BuildSongList();
        RefreshPanel();
    }

    private void RefreshPanel()
    {
        if (musicDataList == null || musicDataList.allTracks == null || musicDataList.allTracks.Count == 0)
            return;

        MusicData music = musicDataList.allTracks[currentIndex];

        if (music == null)
            return;

        titleText.text = music.title;
        artistText.text = music.artist;

        bool hasAudio = music.audioClip != null;    // 音源が無い曲はそもそも遊べない

        ApplyDifficulty(easyButton, easyLabelText, "Easy", music.chartEasy, music.levelEasy, hasAudio);
        ApplyDifficulty(normalButton, normalLabelText, "Normal", music.chartNormal, music.levelNormal, hasAudio);
        ApplyDifficulty(hardButton, hardLabelText, "Hard", music.chartHard, music.levelHard, hasAudio);
    }

    private void ApplyDifficulty(Button button, TextMeshProUGUI labelText, string difficultyName, TextAsset chart, int level, bool hasAudio)
    {
        labelText.text = $"{difficultyName}  Lv.{level}\nGame Start!";
        button.interactable = hasAudio && (chart != null);
    }

    private void BuildSongList()
    {
        if (musicDataList == null || musicDataList.allTracks == null)
        {
            return;
        }

        // 手で置いたテスト用アイテムが残っていても掃除する
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < musicDataList.allTracks.Count; i++)
        {
            MusicData music = musicDataList.allTracks[i];

            if (music == null)
                continue;

            GameObject item = Instantiate(songItemPrefab, contentParent);

            TextMeshProUGUI label = item.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.text = music.title;

            int index = i;

            Button button = item.GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(() => SelectMusic(index));
        }
    }

    public void SelectMusic(int index)
    {
        if (musicDataList == null || musicDataList.allTracks == null)
            return;

        if (index < 0 || index >= musicDataList.allTracks.Count)
            return;

        currentIndex = index;
        RefreshPanel();
    }

    private void StartGame(MusicDataManager.Difficulty difficulty)
    {
        if (currentSelection == null)        
            return;        

        if (musicDataList == null || musicDataList.allTracks == null || currentIndex < 0 || currentIndex >= musicDataList.allTracks.Count)
            return;

        MusicData music = musicDataList.allTracks[currentIndex];

        if (music == null)
            return;

        // 共有の CurrentSelection に「今選ばれているもの」を書き込む
        currentSelection.musicData = music;
        currentSelection.difficulty = difficulty;

        SceneManager.LoadScene("GameScene");
    }
}
