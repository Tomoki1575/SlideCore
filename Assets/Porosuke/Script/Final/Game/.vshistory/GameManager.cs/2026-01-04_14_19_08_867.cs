using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NotePoolManager;
using static GameDataManager;

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
        // 未生成のノーツを走査する（後ろからなのは、Removeによるスキップを防止するため）
        for (int i = NoteListSize - 1; i >= 0; i--)
        {
            NoteData noteData = NoteList[i];
            // 生成すべきノーツを見つけたら
            if (VirtualTime >= noteData.generateTime)
            {
                // Debug.Log("Add");
                NoteList.RemoveAt(i);
                ActiveNoteList.Add(NotePoolManagerClass.MakeNote(noteData, NotePoolManagerClass.GetNote(noteData)));
            }
        }
    }

    private void MoveNotes()
    {
        // 画面に表示されているノーツを走査する（後ろから）
        for (int i = ActiveNoteListSize - 1; i >= 0; i--)
        {
            Note note = ActiveNoteList[i];

            // まだ破棄時間ではないなら
            if (VirtualTime < note.noteData.destroyTime)
            {
                // 加算ではなく絶対算出により誤差・ズレを無くす
                note.prefab.rect.anchoredPosition = new Vector2(0, (float)GetPositionAtTime(note.noteData.generateTime, VirtualTime));
            }
            else
            {
                // Debug.Log("Remove");
                ActiveNoteList.RemoveAt(i);
                NotePoolManagerClass.ReleaseNote(note.prefab);
            }
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
