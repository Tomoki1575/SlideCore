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
    [SerializeField]
    private LineManager LineManagerClass;

    private double VirtualTime;

    private bool bStartPlay;

    private bool bPause;

    private List<Note> ActiveNoteList;

    private int ActiveNoteListSize => ActiveNoteList.Count;

    private List<NoteData> HoldStartNoteList;

    private int HoldStartNoteListSize => HoldStartNoteList.Count;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicSelection, nameof(MusicSelection)),
            (LoadPanelManagerClass, nameof(LoadPanelManagerClass)),
            (SoundManagerClass, nameof(SoundManagerClass)),
            (GameUIManagerClass, nameof(GameUIManagerClass)),
            (NotePoolManagerClass, nameof(NotePoolManagerClass)),
            (LineManagerClass, nameof(LineManagerClass))
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
        HoldStartNoteList = new List<NoteData>();

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
        bool bNoteFinish = NoteQueueSize == 0 && ActiveNoteListSize == 0;
        bool bMusicFinish = VirtualTime >= (MusicStartWaitSec + SoundManagerClass.GetBGMLength());

        if(bNoteFinish && bMusicFinish)
        {
            Debug.Log("Finish!!");
            bStartPlay = false;
        }
        else if (bNoteFinish)
        {
            // ノーツは終わったが曲はまだ流れている
            VirtualTime = MusicStartWaitSec + SoundManagerClass.GetBGMTime();
            
        }
        else if (bMusicFinish)
        {
            // 曲は終わったがノーツはまだ残っている
            VirtualTime += Time.deltaTime;
        }
        else
        {
            // 待機時間中 または 曲終了後 はdeltaTimeで進める
            if (VirtualTime < MusicStartWaitSec)
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
            // HoldStartを追加する際、HoldStartListに入れておき、追跡可能にしておく
            if (noteData.type == NoteType.Hold) HoldStartNoteList.Add(noteData);
            // Debug.Log("Add");
        }
    }

    private void MoveNotes()
    {
        LineManagerClass.ClearLines();

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

                // 加算ではなく絶対算出で移動することで誤差・ズレを無くす
                double position = GetPositionAtTime(note.noteData.generateTime, VirtualTime);
                note.prefab.nc.SetPositionY(-position);

                // 同時押し線の描画
                LineManagerClass.RegistSamePosition(note.noteData, position);
            }
            // 破棄
            else
            {
                // Debug.Log("Remove");
                ActiveNoteList.RemoveAt(i);
                NotePoolManagerClass.ReleaseNote(note.prefab);
                // HoldEndを消す際、対応するHoldStartをリストから削除する
                if(note.noteData.type == NoteType.HoldEnd)
                {
                    HoldStartNoteList.Remove(note.noteData.pairNoteData);
                }
            }
        }

        // Holdの場合は、Start-Endがレーンの長さを超えている（レーンをまたいでいる）ときにも線を描画しなければいけないため、Activeリストを走査するだけでは不十分
        for(int j = 0; j < HoldStartNoteListSize; ++j)
        {
            NoteData noteData = HoldStartNoteList[j];
            double position = GetPositionAtTime(noteData.generateTime, VirtualTime);
            LineManagerClass.AddHoldLine(noteData, position, VirtualTime);
        }

        LineManagerClass.DrawLines();
    }

    private void Judge(NoteData noteData)
    {
        // 今回は、判定は行わず、音だけ鳴らす
        bool bSpecial = noteData.bSpecial;
        switch (noteData.type)
        {
            case NoteType.Tap:
                SoundManagerClass.PlayTapSE(bSpecial);
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
                SoundManagerClass.StopHoldSE(noteData.pairNoteData.loopHandle);
                break;
            case NoteType.Slide:
                SoundManagerClass.PlaySlideSE(bSpecial);
                break;
            case NoteType.Noise:
                SoundManagerClass.PlayNoiseSE(bSpecial);
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
