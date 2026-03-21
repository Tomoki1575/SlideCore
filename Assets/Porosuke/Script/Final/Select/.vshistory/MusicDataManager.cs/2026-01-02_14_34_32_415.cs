using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicDataManager : MonoBehaviour
{
    [SerializeField]
    private MusicDataList MusicDataList;

    private List<MusicData> EasyMusicDataList = new List<MusicData>();
    private List<MusicData> NormalMusicDataList = new List<MusicData>();
    private List<MusicData> HardMusicDataList = new List<MusicData>();

    public List<MusicData> CurrentDisplayMusicDataList { get; private set; } = new List<MusicData>();

    public enum Difficulty
    {
        None,
        Easy,
        Normal,
        Hard
    }

    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.None;

    public event Action OnDisplayMusicDataListChanged;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicDataList, nameof(MusicDataList))
        );
    }

    private void Start()
    {
        PrepareDataList();
    }

    private void PrepareDataList()
    {
        EasyMusicDataList.Clear();
        NormalMusicDataList.Clear();
        HardMusicDataList.Clear();

        foreach (MusicData track in MusicDataList.allTracks)
        {
            if (track.chartEasy != null) EasyMusicDataList.Add(track);
            if (track.chartNormal != null) NormalMusicDataList.Add(track);
            if (track.chartHard != null) HardMusicDataList.Add(track);
        }

        SetDifficulty(Difficulty.Easy);
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        if (difficulty == CurrentDifficulty) return;
        CurrentDifficulty = difficulty;

        CurrentDisplayMusicDataList = GetListForDifficulty(difficulty);

        OnDisplayMusicDataListChanged?.Invoke();
    }

    private List<MusicData> GetListForDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return EasyMusicDataList;
            case Difficulty.Normal:
                return NormalMusicDataList;
            case Difficulty.Hard:
                HardMusicDataList.Clear() ;
                return HardMusicDataList;
            default:
                return EasyMusicDataList;
        }
    }
}
