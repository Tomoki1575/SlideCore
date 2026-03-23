using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class MusicData
    {
        public AudioClip music;
        public TextAsset chart;
    }

    [SerializeField] private List<MusicData> MusicDataList;
    private int MusicDataListSize => MusicDataList.Count;

    [SerializeField] private int SelectIndex;

    void Start()
    {
        bool bSuccess = TryLoadMusicData(SelectIndex);
        if (!bSuccess)
        {
            OnFailedLoadMusicData();
            return;
        }
    }

    private bool TryLoadMusicData(int index)
    {
        // まず、インデックスがリストの有効な場所を指しているかチェックする
        if (index < 0 || index >= MusicDataListSize) return false;

        // 次に、wavとJsonがどちらもセットされているかチェックする
        MusicData data = MusicDataList[index];
        if (data.music == null || data.chart == null) return false;

        // Jsonを読み込み、データを変換する


        return true;
    }

    // Utility
    private void OnFailedLoadMusicData()
    {
#if UNITY_EDITOR
        Debug.LogError("Failed Load MusicData");
#endif
    }
}
