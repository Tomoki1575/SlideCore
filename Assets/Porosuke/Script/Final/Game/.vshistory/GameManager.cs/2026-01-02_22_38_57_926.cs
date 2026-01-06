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

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicSelection, nameof(MusicSelection)),
            (LoadPanelManagerClass, nameof(LoadPanelManagerClass)),
            (SoundManagerClass, nameof(SoundManagerClass))
            );
    }

    void Start()
    {
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


        // 曲をAudioSourceにセット
        SoundManagerClass.SetBGMClip(data.audioClip);

        // 読み込み成功
        return true;
    }

    private void OnShowLoadPanelFinished()
    {
        // ゲームプレイ開始

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
