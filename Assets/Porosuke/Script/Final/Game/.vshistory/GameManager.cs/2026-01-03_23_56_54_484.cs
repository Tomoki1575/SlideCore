using System.Collections.Generic;
using UnityEngine;

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

    private double VirtualTime;

    private bool bStartPlay;

    private bool bPause;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicSelection, nameof(MusicSelection)),
            (LoadPanelManagerClass, nameof(LoadPanelManagerClass)),
            (SoundManagerClass, nameof(SoundManagerClass)),
            (GameUIManagerClass, nameof(GameUIManagerClass))
            );
    }

    void Start()
    {
        VirtualTime = 0;
        bStartPlay = false;
        bPause = false;

        LoadPanelManagerClass.OnShowLoadPanelFinished += OnShowLoadPanelFinished;

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
        GameDataManager.ConvertJsonDataToGameData(jsonData);

        // 曲をAudioSourceにセット
        SoundManagerClass.SetBGMClip(data.audioClip);

        // 読み込み成功
        return true;
    }

    private void OnShowLoadPanelFinished()
    {
        // ゲームプレイ開始
        bStartPlay = true;
    }

    private void Update()
    {
        if (!bStartPlay || bPause) return;
        // プレイ中はここを通る
        ControlMusic();
    }

    private void ControlMusic()
    {
        // 待機時間中はdeltaTimeで進める
        if(VirtualTime < GameDataManager.MusicStartWaitSec)
        {
            VirtualTime += Time.deltaTime;
        }
        else
        {
            // 再生していないなら再生して同期
            if (!SoundManagerClass.IsBGMPlaying())
            {
                SoundManagerClass.SetBGMTime(0);
                VirtualTime = GameDataManager.MusicStartWaitSec;
                SoundManagerClass.PlayBGM();
            }
            // 再生中は曲で同期をとる
            else VirtualTime = GameDataManager.MusicStartWaitSec + SoundManagerClass.GetBGMTime();
        }
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
