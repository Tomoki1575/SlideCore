using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectToGameScript : MonoBehaviour
{
    [SerializeField] private MusicSelection musicSelection;   // NoteGenerator/MusicManagerと同じアセット
    [SerializeField] private MusicData rainMusicData;         // 雨の音のMusicData

    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    private void Start()
    {
        easyButton.onClick.AddListener(() => StartGame(MusicDataManager.Difficulty.Easy));
        normalButton.onClick.AddListener(() => StartGame(MusicDataManager.Difficulty.Normal));
        hardButton.onClick.AddListener(() => StartGame(MusicDataManager.Difficulty.Hard));
    }

    private void StartGame(MusicDataManager.Difficulty difficulty)
    {
        // 選んだ曲と難易度を、共有の MusicSelection アセットに書き込む
        musicSelection.musicData = rainMusicData;
        musicSelection.difficulty = difficulty;

        // ゲームシーンへ
        SceneManager.LoadScene("GameScene");
    }
}
