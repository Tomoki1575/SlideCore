using System;
using System.Collections.Generic;

public static class GameDataManager
{
    public class SpeedSection
    {
        public double time;
        public double speedScale;
        public double cumulativeDistance;   // 前計算値

        public double speed => OptionData.BaseNoteSpeed * speedScale;

        public double DistanceAt(double _time)  => cumulativeDistance + speed * (_time - time);
    }

    public class NoteData
    {
        public double generateTime;  // 生成する時間
        public double finishTime;    // 判定ラインに来る時間
        public double destroyTime;   // 破棄する時間
        public int lane;            // 所属レーン
        public NoteType type;
        public bool bSpecial;
        public NoteData pairNoteData;
        public bool bIsRight;
        public bool bHasJudged;
        public SoundManager.LoopHandle loopHandle;
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

    public static List<SpeedSection> SpeedSectionList { get; private set; }

    public static int SpeedSectionListSize => SpeedSectionList.Count;

    // 前処理用List
    private static List<NoteData> NoteList;

    private static int NoteListSize => NoteList.Count;

    // ゲーム用キュー
    public static Queue<NoteData> NoteQueue { get; private set; }

    public static int NoteQueueSize => NoteQueue.Count;

    public static double GameWaitSec {  get; private set; }

    public static double OffsetSec {  get; private set; }

    public static double MusicStartWaitSec { get; private set; }

    public static void ConvertJsonDataToGameData(JsonDataConverter.JsonData jsonData)
    {
        Init();
        // jsonDataのデータでリストを作成する（この時点ではGen / Destがまだ不定）
        SetupDataList(jsonData);
        // 各待ち時間を計算する
        CalcWaitSec(jsonData.meta.offset);
        // ノーツ・速度倍率ノーツの正確な生成時間、判定時間、破棄時間を計算する
        CalcDataTime();
    }

    private static void Init()
    {
        SpeedSectionList = new List<SpeedSection>();
        NoteList = new List<NoteData>();
        GameWaitSec = 0;
        OffsetSec = 0;
        MusicStartWaitSec = 0;
    }

    private static void SetupDataList(JsonDataConverter.JsonData jsonData)
    {
        TimeConverter.SetSignatureDataList(jsonData.signatures);

        // Jsonの速度倍率リストから、ゲーム用速度倍率区間リストを作成する
        foreach(JsonDataConverter.BpmData bpmData in jsonData.bpms)
        {
            SpeedSection speedSectionData = new SpeedSection();
            speedSectionData.time = TimeConverter.ConvertBeatToReal(bpmData.beat, jsonData.bpms);
            speedSectionData.speedScale = bpmData.speedScale;
            SpeedSectionList.Add(speedSectionData);
        }

        // Jsonのノーツリストから、ゲーム用ノーツリストを作成する
        // Game用HoldとJsonHoldの対応辞書
        Dictionary<NoteData, JsonDataConverter.NoteData> holdDataDictionary = new Dictionary<NoteData, JsonDataConverter.NoteData>();
        List<NoteData> holdStartNoteList = new List<NoteData>();
        foreach(JsonDataConverter.NoteData noteData in jsonData.notes)
        {
            NoteData gameNoteData = new NoteData();
            gameNoteData.generateTime = 0;
            gameNoteData.finishTime = TimeConverter.ConvertBeatToReal(noteData.beat, jsonData.bpms);
            gameNoteData.destroyTime = 0;
            gameNoteData.lane = noteData.lane;
            gameNoteData.type = noteData.type;
            gameNoteData.bSpecial = noteData.bSpecial;
            gameNoteData.pairNoteData = null;
            // この時点ではペアとなるHold または HoldEndが作られていない可能性があるので、辞書に登録しておく
            if (noteData.pairNoteData != null)
            {
                holdDataDictionary[gameNoteData] = noteData;
                if (gameNoteData.type == NoteType.Hold) holdStartNoteList.Add(gameNoteData);
            }
            gameNoteData.bIsRight = noteData.bIsRight;
            gameNoteData.bHasJudged = false;
            gameNoteData.loopHandle = null;
            NoteList.Add(gameNoteData);
        }
        // ペアをセットする
        for(int i = 0; i < holdStartNoteList.Count; ++i)
        {
            NoteData holdStartNote = holdStartNoteList[i];
            // Startのペア（End）をインデックスから求めてセットする
            int pairIndex = holdDataDictionary[holdStartNote].pairIndex;
            // ペアインデックスと同じ場所に、Game版ペアノーツもあるはずである
            NoteData holdEndData = NoteList[pairIndex];

            UnityEngine.Debug.Log($"[index:{i}] Start{holdStartNote.generateTime} End{holdEndData.generateTime}");
            holdStartNote.pairNoteData = holdEndData;
            holdEndData.pairNoteData = holdStartNote;
        }
    }

    private static void CalcWaitSec(int offsetMs)
    {
        OffsetSec = offsetMs / 1000d;
        MusicStartWaitSec = OptionData.MusicStartWaitTime;

        // フェーズ1：offsetとデフォルトのStartWaitから、StartWaitを仮決め
        // 正方向にoffsetの方が大きいなら、StartWaitも合わせる
        if (OffsetSec > MusicStartWaitSec) MusicStartWaitSec = OffsetSec;

        GameWaitSec = MusicStartWaitSec - OffsetSec;

        if(NoteListSize > 0)
        {
            // フェーズ2：Editorでの時間基準で、最初のノーツの生成時間について考える
            PrecomputeDistance();
            double generateTime = CalcGenerateTime(NoteList[0].finishTime, OptionData.LaneLength);
            // 生成時間が負の値の場合、更に長く待つ必要がある可能性がある
            if(generateTime < 0)
            {
                double genAbs = Math.Abs(generateTime);
                if (genAbs > GameWaitSec) GameWaitSec = genAbs;
            }
        }

        MusicStartWaitSec = GameWaitSec + OffsetSec;
    }

    private static void CalcDataTime()
    {
        // 現状のデータのTimeはエディター基準になっている

        // まず、速度倍率区間の時間をGameWaitだけ後ろにずらす（最初以外）
        for(int i = 0; i < SpeedSectionListSize; ++i)
        {
            if (i == 0) continue;

            SpeedSectionList[i].time += GameWaitSec;
        }
        // 再計算
        PrecomputeDistance();
        // 同様にノーツもずらしつつ、生成開始時間を計算
        foreach(NoteData noteData in NoteList)
        {
            noteData.finishTime += GameWaitSec;
            noteData.generateTime = CalcGenerateTime(noteData.finishTime, OptionData.LaneLength);
            noteData.destroyTime = CalcDestoryTime(noteData.finishTime, OptionData.LaneAfterLength);
        }
        // 全ての前処理が終わったら、キューにデータを移す
        NoteQueue = new Queue<NoteData>(NoteList);
        NoteList = null;
    }

    // Utility
    // 事前計算で高速化
    private static void PrecomputeDistance()
    {
        List<SpeedSection> sections = SpeedSectionList;

        double cum = 0f;

        for (int i = 0; i < sections.Count; i++)
        {
            SpeedSection speedSection = sections[i];
            // 以前の値を保持
            speedSection.cumulativeDistance = cum;

            if (i + 1 < sections.Count)
            {
                double dt = sections[i + 1].time - sections[i].time;
                cum += OptionData.BaseNoteSpeed * sections[i].speedScale * dt;
            }
        }
    }

    // 指定時刻の区間を返す
    public static SpeedSection GetSectionAtTime(double time)
    {
        List<SpeedSection> sections = SpeedSectionList;

        int lo = 0;
        int hi = sections.Count - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (sections[mid].time <= time)
                lo = mid + 1;
            else
                hi = mid - 1;
        }

        // hi が該当区間（time < 最初の StartTime でも 0 になる）
        return sections[Math.Max(0, hi)];
    }

    // 指定時刻の累積距離を返す
    public static double GetDistanceAtTime(double time)
    {
        SpeedSection sec = GetSectionAtTime(time);
        return sec.DistanceAt(time);
    }

    private static double CalcDestoryTime(double finishTime, double distance)
    {
        List<SpeedSection> sections = SpeedSectionList;

        double startDist = GetDistanceAtTime(finishTime);
        double targetDist = startDist + distance;

        for (int i = sections.Count - 1; i >= 0; i--)
        {
            SpeedSection sec = sections[i];
            if (targetDist >= sec.cumulativeDistance)
            {
                double d = targetDist - sec.cumulativeDistance;
                return sec.time + d / sec.speed;
            }
        }

        // 過去無限適用（理論上必ず到達）
        SpeedSection first = sections[0];
        return first.time + targetDist / first.speed;
    }

    private static double CalcGenerateTime(double finishTime, double distance)
    {
        List<SpeedSection> sections = SpeedSectionList;

        double goalDist = GetDistanceAtTime(finishTime);
        double targetDist = goalDist - distance;

        for (int i = sections.Count - 1; i >= 0; i--)
        {
            SpeedSection sec = sections[i];
            if (targetDist >= sec.cumulativeDistance)
            {
                double d = targetDist - sec.cumulativeDistance;
                return sec.time + d / sec.speed;
            }
        }

        SpeedSection first = sections[0];
        return first.time + targetDist / first.speed;
    }

    public static double GetPositionAtTime(double generateTime, double currentTime)
    {
        return -(GetDistanceAtTime(currentTime) - GetDistanceAtTime(generateTime));
    }
}
