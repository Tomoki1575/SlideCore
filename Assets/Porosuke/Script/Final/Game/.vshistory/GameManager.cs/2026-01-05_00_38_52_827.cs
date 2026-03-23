using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameDataManager;
using static NotePoolManager;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private MusicSelection MusicSelection;
    [SerializeField]
    private LoadPanelManager LoadPanelManagerClass;
    [SerializeField]
    private SoundManager SoundManagerClass;
    [SerializeField]
    private GameUIManager GameUIManagerClass;
    [SerializeField]
    private NotePoolManager NotePoolManagerClass;

    private double VirtualTime;

    private bool bStartPlay;

    private bool bPause;

    private List<Note> ActiveNoteList;

    private int ActiveNoteListSize => ActiveNoteList.Count;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicSelection, nameof(MusicSelection)),
            (LoadPanelManagerClass, nameof(LoadPanelManagerClass)),
            (SoundManagerClass, nameof(SoundManagerClass)),
            (GameUIManagerClass, nameof(GameUIManagerClass)),
            (NotePoolManagerClass, nameof(NotePoolManagerClass))
            );

        LoadPanelManagerClass.OnShowLoadPanelFinished += OnShowLoadPanelFinished;
        GameUIManagerClass.OnPause += OnPause;
        GameUIManagerClass.OnRetire += OnRetire;
        GameUIManagerClass.OnRetry += OnRetry;
        GameUIManagerClass.OnReturn += OnReturn;
    }

    void Start()
    {
        VirtualTime = 0;
        bStartPlay = false;
        bPause = false;
        ActiveNoteList = new List<Note>();

        if (TryLoadMusicData(MusicSelection))
        {
            // 読み込みに成功
            // Debug.Log("Success!");
            // パネルに伝える
            LoadPanelManagerClass.NotifyLoadFinished();
        }
        else
        {
            // 読み込みに失敗
            OnFailedLoadMusicData();
        }
    }

    private bool TryLoadMusicData(MusicSelection musicSelection)
    {
        // まず、渡されたデータが空でないかチェックする
        if (musicSelection == null) return false;

        LoadPanelManagerClass.ShowLoadPanel(musicSelection);

        // 次に、wavとJsonがどちらもセットされているかチェックする
        MusicData data = musicSelection.musicData;
        TextAsset chart = musicSelection.GetChart();
        if (data == null || chart == null) return false;

        // Jsonを読み込み、データを変換する
        JsonDataConverter.JsonData jsonData = JsonDataConverter.LoadJson(chart);

        // 変換が失敗していないかチェックする
        if (jsonData == null) return false;

        // JsonDataをゲーム用データに変換する
        ConvertJsonDataToGameData(jsonData);

        // 曲をAudioSourceにセット
        SoundManagerClass.SetBGMClip(data.audioClip);

        // 音量をセット
        SoundManagerClass.SetBGMVolume(OptionData.BGMVolume);
        SoundManagerClass.SetSEVolume(OptionData.SEVolume);

        // 読み込み成功
        return true;
    }

    private void OnShowLoadPanelFinished()
    {
        // ゲームプレイ開始
        bStartPlay = true;
        GameUIManagerClass.SetConstantUI();
    }

    private void Update()
    {
        if (!bStartPlay || bPause) return;
        // プレイ中はここを通る
        ControlMusic();
        CheckAddNotes();
        MoveNotes();

        GameUIManagerClass.SetModifyUI(VirtualTime, bStartPlay, bPause, 
            ActiveNoteListSize > 0 ? ActiveNoteList[0].noteData : null);
    }

    private void ControlMusic()
    {
        // 待機時間中はdeltaTimeで進める
        if(VirtualTime < MusicStartWaitSec)
        {
            VirtualTime += Time.deltaTime;
        }
        else
        {
            // 再生していないなら再生して同期
            if (!SoundManagerClass.IsBGMPlaying())
            {
                SoundManagerClass.SetBGMTime(0);
                VirtualTime = MusicStartWaitSec;
                SoundManagerClass.PlayBGM();
            }
            // 再生中は曲で同期をとる
            else VirtualTime = MusicStartWaitSec + SoundManagerClass.GetBGMTime();
        }
    }

    private void CheckAddNotes()
    {
        while (NoteQueueSize > 0)
        {
            // 先頭を見る
            NoteData noteData = NoteQueue.Peek();

            // まだ生成時刻に達していないなら終了
            if (VirtualTime < noteData.generateTime)
                break;

            // 生成対象なので取り出す
            NoteQueue.Dequeue();

            ActiveNoteList.Add(
                NotePoolManagerClass.MakeNote(
                    noteData,
                    NotePoolManagerClass.GetNote(noteData)
                )
            );
            // Debug.Log("Add");
        }
    }

    private void MoveNotes()
    {
        // 画面に表示されているノーツを走査する（後ろから）
        for (int i = ActiveNoteListSize - 1; i >= 0; i--)
        {
            Note note = ActiveNoteList[i];

            // まだ破棄時間ではない
            if (VirtualTime < note.noteData.destroyTime)
            {
                // 判定線を超えたかつ、未判定
                if(VirtualTime >= note.noteData.finishTime && !note.noteData.bHasJudged)
                {
                    note.noteData.bHasJudged = true;
                    Judge(note.noteData);
                }

                // 加算ではなく絶対算出により移動することで誤差・ズレを無くす
                note.prefab.rect.anchoredPosition = new Vector2(0, (float)GetPositionAtTime(note.noteData.generateTime, VirtualTime));
            }
            // 破棄
            else
            {
                // Debug.Log("Remove");
                ActiveNoteList.RemoveAt(i);
                NotePoolManagerClass.ReleaseNote(note.prefab);
            }
        }
    }

    private void Judge(NoteData noteData)
    {
        // 今回は、判定は行わず、音だけ鳴らす
        bool bSpecial = noteData.bSpecial;
        switch (noteData.type)
        {
            case NoteType.Tap:
                //SoundManagerClass.PlayTapSE(bSpecial);
                break;
            case NoteType.Hold:
                // タップ音を鳴らし、連続したホールド音を再生開始する
                SoundManagerClass.PlayTapSE(bSpecial);
                // ループ対象をNoteDataに格納
                SoundManager.LoopHandle loopHandle = SoundManagerClass.PlayHoldSE(bSpecial);
                noteData.loopHandle = loopHandle;
                break;
            case NoteType.HoldEnd:
                // タップ音を鳴らし、連続したホールド音を再生停止する
                SoundManagerClass.PlayTapSE(bSpecial);
                // Startのハンドルを停止
                Debug.Log(noteData.pairNoteData);
                // SoundManagerClass.StopHoldSE(noteData.pairNoteData.loopHandle);
                break;
            case NoteType.Slide:
                //SoundManagerClass.PlaySlideSE(bSpecial);
                break;
            case NoteType.Noise:
                //SoundManagerClass.PlayNoiseSE(bSpecial);
                break;
        }
    }

    // Event
    private void OnPause()
    {
        bPause = true;
        SoundManagerClass.PauseAllSound();
        GameUIManagerClass.SetPausePanel(true);
    }

    private void OnRetire()
    {
        // Selectに戻る
        SceneManager.LoadScene("Final_Select");
    }

    private void OnRetry()
    {
        // 再ロード
        SceneManager.LoadScene("Final_Play");
    }

    private void OnReturn()
    {
        bPause = false;
        SoundManagerClass.UnPauseAllSound();
        GameUIManagerClass.SetPausePanel(false);
    }
    // Utility
    private void OnFailedLoadMusicData()
    {
#if UNITY_EDITOR
        Debug.LogError("Failed Load MusicData");
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
