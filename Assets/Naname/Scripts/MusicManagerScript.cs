using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript Instance;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private TextMeshProUGUI songTimeText;

    public static float SongTime;
    public static float MsSongTime;

    private double dspStartTime;
    private bool isPlaying = false;

    public static double SongStartRealTime;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 曲開始時の時間を保持し、曲を再生する関数。
    /// </summary>
    public void StartSong()
    {
        // [ AudioSettings.dspTime; について ]
        // OSそのものの音をベースに作られたタイマー
        //
        // 普通のタイマー（Time.time）が「1秒、2秒」と数えるのに対し、dspTime は裏側で「音の粒（サンプル）を何個処理したか」を数えている
        // 音の粒 44100個分 ＝ 1秒
        // といったように、音のデータ量から時間を計算している

        dspStartTime = AudioSettings.dspTime;

        SongStartRealTime = Time.realtimeSinceStartupAsDouble;

        audioSource.Play();
        isPlaying = true;
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame && !isPlaying)
        {
            StartSong();
        }

        if (isPlaying)
        {
            SongTime = (float)(AudioSettings.dspTime - dspStartTime);   //  正確な経過秒数 = (floatに変換)(現在の時刻 - 開始時の時刻)

            MsSongTime = Mathf.FloorToInt(SongTime * 1000);

            songTimeText.text = $"{MsSongTime}";
        }
    }
}