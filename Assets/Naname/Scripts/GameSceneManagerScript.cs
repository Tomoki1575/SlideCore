using TMPro;
using UnityEngine;

public enum GameState
{
    Preparing,
    Playing,
    Paused,
    Resuming,
    Finishing
};

public class GameSceneManagerScript : MonoBehaviour
{
    public GameState State { get; private set; } = GameState.Preparing;

    public static GameSceneManagerScript Instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;

    private float resultDelay = 1.5f;

    private GameState stateBeforePause;

    public float EndPauseTime;

    public const float EndPauseSec = 3;

    [SerializeField] private TextMeshProUGUI countDownText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FadeManagerScript.Instance.FadeIn();

        // 曲のパッケージイラストなどを表示
    }


    private void Update()
    {
        switch (State)
        {
            case GameState.Preparing:
                {
                    if (MusicManagerScript.Instance.IsMusicStartReady && !FadeManagerScript.Instance.IsFading)
                    {
                        MusicManagerScript.Instance.StartSong();
                        ChangeState(GameState.Playing);
                    }

                    break;
                }

            case GameState.Playing:
                {
                    // 「曲が流れてからの経過時間」が「曲の長さ ＋ 余韻」を過ぎたらリザルトへ
                    if (MusicManagerScript.Instance.CurrentSongTime >= MusicManagerScript.Instance.SongLength + resultDelay)
                    {
                        SceneLoaderScript.Instance.LoadSceneWithFade("ResultScene");
                        ChangeState(GameState.Finishing);
                    }

                    break;
                }

            case GameState.Resuming:
                {
                    TickCountDown();

                    break;
                }
        }
    }

    public void OnPausePressed()
    {
        countDownText.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(true);

        if (State != GameState.Resuming)    // カウントダウン中にEscapeを押すとGameState.Resumingが入ってしまう為、それの対策
            stateBeforePause = State;   // ステートの戻り先を覚えておく

        MusicManagerScript.Instance.PauseSong();
        ChangeState(GameState.Paused);
    }

    /// <summary>
    /// ログを出しながらステートを変更する(多重判定対策済み)
    /// </summary>
    private void ChangeState(GameState next)
    {
        // 多重判定の対策
        if (next == State)
            return;

        State = next;
        Debug.Log("[State] 現在のステート : " + State);
    }

    /// <summary>
    /// 曲をやり直すボタンが押された(ボタンオブジェクトのインスペクターから設定)
    /// </summary>
    public void OnRetryGame()
    {
        SceneLoaderScript.Instance.LoadSceneWithFade("GameScene");
    }

    /// <summary>
    /// 曲選択画面に戻るボタンが押された(ボタンオブジェクトのインスペクターから設定)
    /// </summary>
    public void OnPushGotoSelectSceneButton()
    {
        SceneLoaderScript.Instance.LoadSceneWithFade("SelectScene");
    }

    /// <summary>
	/// 曲を再開するボタンが押された(ボタンオブジェクトのインスペクターから設定)
	/// </summary>
    public void OnContinueGame()
    {
        if (countDownText == null)
        {
            Debug.LogError("[Unassigned] カウントダウンのUIが割り当てられていません。", this);
            return;
        }

        pauseMenu.gameObject.SetActive(false);

        // カウントダウンを3に初期化
        EndPauseTime = EndPauseSec;

        countDownText.gameObject.SetActive(true);

        ChangeState(GameState.Resuming);
    }

    /// <summary>
    /// ポーズから戻る際に、カウントダウンしながら戻る
    /// </summary>
    private void TickCountDown()
    {
        EndPauseTime -= Time.deltaTime;

        countDownText.text = $"{(int)EndPauseTime + 1}";

        if (EndPauseTime <= 0)
        {
            countDownText.gameObject.SetActive(false);

            // 曲が始まっていたときだけ再開処理が要る
            if (stateBeforePause == GameState.Playing)
                MusicManagerScript.Instance.ResumeSong();

            ChangeState(stateBeforePause);
        }
    }
}
