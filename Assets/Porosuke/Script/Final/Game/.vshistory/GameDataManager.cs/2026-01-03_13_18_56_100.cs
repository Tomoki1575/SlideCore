using System.Collections.Generic;
using System;
using UnityEngine;

public static class GameDataManager
{
    public class SpeedScaleData
    {
        public float time;
        public float speedScale;
    }

    public class NoteData
    {
        public float generateTime;  // 生成する時間
        public float finishTime;    // 判定ラインに来る時間
        public float destroyTime;   // 破棄する時間
        public int lane;            // 所属レーン
        public NoteType type;
        public bool bSpecial;
        public NoteData pairNoteData;
        public bool bIsRight;
    }

    [Serializable]
    public enum NoteType
    {
        None,
        Tap,
        Hold,
        HoldEnd,
        Slide,
        Noise
    }

    public static List<SpeedScaleData> SpeedScaleList { get; private set; }

    public static int SpeedScaleListSize => SpeedScaleList.Count;

    public static List<NoteData> NoteList { get; private set; }

    public static int NoteListListSize => NoteList.Count;

    public static float GameWaitSec {  get; private set; }

    public static float OffsetSec {  get; private set; }

    public static float MusicStartWaitSec { get; private set; }

    public static void ConvertJsonDataToGameData(JsonDataConverter.JsonData jsonData)
    {
        Init(jsonData.meta.offset);
        // jsonDataのデータでリストを作成する
        SetupDataList(jsonData);
        // オフセットと曲開始前待ち時間から、各時間を試算する（フェーズ1）

    }

    private static void Init(int offset)
    {
        SpeedScaleList = new List<SpeedScaleData>();
        NoteList = new List<NoteData>();
        GameWaitSec = 0;
        OffsetSec = offset / 1000f;
        MusicStartWaitSec = OptionData.MusicStartWaitTime;
    }

    private static void SetupDataList(JsonDataConverter.JsonData jsonData)
    {
        // Jsonの速度倍率リストから、ゲーム用速度倍率リストを作成する

        // Jsonのノーツリストから、ゲーム用ノーツリストを作成する

    }
}
