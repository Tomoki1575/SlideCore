using UnityEngine;
using System.Collections.Generic;
using System;
using static OptionData;

public class NoteDataManager : MonoBehaviour
{
    // 構造体やENUMは NoteDataManager.名前 または
    // using static NoteDatamanager して 名前　でどこからでもアクセスできます。

    public struct SpeedScaleData
    {
        public float time;
        public float speedScale;
    }

    public struct NoteData
    {
        public float generateTime;  // 生成する時間
        public float finishTime;    // 判定ラインに来る時間
        public int lane;            // 所属レーン
        public NoteType type;
        public bool bSpecial;       // 特殊ノーツか
        public float duration;      // Holdノーツのみで使用、それ以外では0
        public bool bIsRight;       // Slideノーツのみで使用、それ以外ではfalse
    }

    public enum NoteType
    {
        None,
        Tap,
        Hold,
        Slide,
        Noise
    }

    public struct MetaData
    {
        public string title;
        public string artist;
        public string chartAuthor;
        public string Description;
        public Difficulty difficulty;
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
        Master
    }

    private struct DistanceData
    {
        public float startTime;     // この区間の開始時刻
        public float multiplier;    // 区間倍率
        public float accumulated;   // ここまでに進んだ累積距離
    };

    // 変数
    private List<SpeedScaleData> SpeedScaleList;
    private List<NoteData> NoteList;
    private MetaData ChartMetaData;
    private List<DistanceData> DistanceList;
    private float SongLength;

    void Awake()
    {
        SpeedScaleList = new List<SpeedScaleData>();
        NoteList = new List<NoteData>();
        DistanceList = new List<DistanceData>();
        SongLength = 0;
    }

    // Json→Dataは未実装

    /// <summary>
    /// 適当なデータを入れておく、仮の初期化
    /// </summary>
    /// <param name="AudioLength"></param>
    public void TempInit(float AudioLength)
    {
        SongLength = AudioLength;

        SpeedScaleList.Add(MakeSpeedScaleData(0, 1));
        SpeedScaleList.Add(MakeSpeedScaleData(10, 2));
        SpeedScaleList.Add(MakeSpeedScaleData(30, 0.5f));

        BuildDistanceTable();

        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 0, 0, NoteType.Tap, false, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 2, 3, NoteType.Tap, true, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 8, 1, NoteType.Tap, false, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 14, 4, NoteType.Tap, false, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 18, 2, NoteType.Tap, true, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 20, 0, NoteType.Tap, false, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 26, 0, NoteType.Tap, false, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 38, 5, NoteType.Tap, false, 0, false));
        NoteList.Add(MakeNoteData(GetTotalOffsetSec() + 42, 5, NoteType.Tap, false, 0, false));
    }

    private SpeedScaleData MakeSpeedScaleData(float time, float speedScale)
    {
        return new SpeedScaleData
        {
            time = time, 
            speedScale = speedScale 
        };
    }

    private NoteData MakeNoteData(float generateTime, int lane, NoteType type, bool bSpecial, float duration, bool bIsRight)
    {
        return new NoteData
        {
            generateTime = generateTime,
            finishTime = CalcReachTime(generateTime),
            lane = Math.Clamp(lane, 0, 5),
            type = type,
            bSpecial = bSpecial,
            duration = type == NoteType.Hold ? duration : 0,
            bIsRight = type == NoteType.Slide ? bIsRight : false
        };
    }

    /// <summary>
    /// 曲全体の累積距離テーブルを作成（Startなどで1回だけ呼ぶ）
    /// </summary>
    private void BuildDistanceTable()
    {
        DistanceList = new List<DistanceData>();
        float acc = 0f;

        for (int i = 0; i < SpeedScaleList.Count; i++)
        {
            float start = SpeedScaleList[i].time;
            float mul = SpeedScaleList[i].speedScale;
            float end = (i + 1 < SpeedScaleList.Count)
                            ? SpeedScaleList[i + 1].time
                            : GetTotalOffsetSec() + SongLength;

            DistanceList.Add(new DistanceData
            {
                startTime = start,
                multiplier = mul,
                accumulated = acc
            });

            acc += (end - start) * BaseNoteSpeed * mul;
        }
    }

    /// <summary>
    /// time秒時点での累積距離を返す
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private float DistanceAtSec(float time)
    {
        if (DistanceList == null || DistanceList.Count == 0)
            return 0f;

        int lo = 0;
        int hi = DistanceList.Count - 1;
        int idx = 0;

        // 二分探索で time 以下の最新区間を探す
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (DistanceList[mid].startTime <= time)
            {
                idx = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        DistanceData d = DistanceList[idx];
        return d.accumulated + (time - d.startTime) * BaseNoteSpeed * d.multiplier;
    }

    /// <summary>
    /// ノーツ生成時刻から判定バー到達時間を計算
    /// </summary>
    /// <param name="generateTime"></param>
    /// <returns></returns>
    private float CalcReachTime(float generateTime)
    {
        float lo = generateTime;
        float hi = generateTime + 10f * LaneLength / BaseNoteSpeed;

        while (hi - lo > 0.0001f)
        {
            float mid = 0.5f * (lo + hi);
            if (DistanceAtSec(mid) - DistanceAtSec(generateTime) >= LaneLength)
                hi = mid;
            else
                lo = mid;
        }
        return hi;
    }

    /// <summary>
    /// ノーツの現在位置を取得（生成時刻を0とした位置）
    /// </summary>
    /// <param name="generateTime"></param>
    /// <param name="currentTime"></param>
    /// <returns></returns>
    public float PositionAt(float generateTime, float currentTime)
    {
        return -(DistanceAtSec(currentTime) - DistanceAtSec(generateTime));
    }

    // Getter
    public List<SpeedScaleData> GetSpeedScaleList()
    {
        return SpeedScaleList;
    }

    public float GetCurrentSpeedScale(float currentTime)
    {
        if(SpeedScaleList == null || SpeedScaleList.Count == 0) return 0;


        // 2分木探索で高速化
        int lo = 0;
        int hi = SpeedScaleList.Count - 1;
        int mid = 0;

        while (lo <= hi)
        {
            mid = (lo + hi) / 2;
            if (SpeedScaleList[mid].time <= currentTime)
                lo = mid + 1;
            else
                hi = mid - 1;
        }
        if (hi < 0) return SpeedScaleList[0].speedScale;
        return SpeedScaleList[hi].speedScale;
    }

    public List<NoteData> GetNoteList()
    {
        return NoteList;
    }

    public MetaData GetChartMetaData()
    {
        return ChartMetaData;
    }
}
