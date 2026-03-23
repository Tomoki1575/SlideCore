using System;
using System.Collections.Generic;

public static class GameDataManager
{
    public class SpeedScaleData
    {
        public double time;
        public float speedScale;
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

    public static List<NoteData> HoldStartNoteList { get; private set; }

    public static int HoldStartNoteListSize => HoldStartNoteList.Count;

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
        SpeedScaleList = new List<SpeedScaleData>();
        NoteList = new List<NoteData>();
        HoldStartNoteList = new List<NoteData>();
        GameWaitSec = 0;
        OffsetSec = 0;
        MusicStartWaitSec = 0;
    }

    private static void SetupDataList(JsonDataConverter.JsonData jsonData)
    {
        TimeConverter.SetSignatureDataList(jsonData.signatures);

        // Jsonの速度倍率リストから、ゲーム用速度倍率リストを作成する
        foreach(JsonDataConverter.BpmData bpmData in jsonData.bpms)
        {
            SpeedScaleData speedScaleData = new SpeedScaleData();
            speedScaleData.time = TimeConverter.ConvertBeatToReal(bpmData.beat, jsonData.bpms);
            speedScaleData.speedScale = bpmData.speedScale;
            SpeedScaleList.Add(speedScaleData);
        }

        // Jsonのノーツリストから、ゲーム用ノーツリストを作成する
        Dictionary<NoteData, JsonDataConverter.NoteData> holdPairDictionary = new Dictionary<NoteData, JsonDataConverter.NoteData>();
        foreach(JsonDataConverter.NoteData noteData in jsonData.notes)
        {
            NoteData gameNoteData = new NoteData();
            gameNoteData.finishTime = TimeConverter.ConvertBeatToReal(noteData.beat, jsonData.bpms);
            gameNoteData.lane = noteData.lane;
            gameNoteData.type = noteData.type;
            gameNoteData.bSpecial = noteData.bSpecial;
            gameNoteData.pairNoteData = null;
            // この時点ではペアとなるHold または HoldEndが作られていない可能性があるので、辞書に登録しておく
            if (noteData.pairNoteData != null)
            {
                holdPairDictionary[gameNoteData] = noteData.pairNoteData;
                if (gameNoteData.type == NoteType.Hold) HoldStartNoteList.Add(gameNoteData);
            }
            gameNoteData.bIsRight = noteData.bIsRight;
            NoteList.Add(gameNoteData);
        }
        // ペアをセットする
        for(int i = 0; i < HoldStartNoteListSize; ++i)
        {
            NoteData holdStartNote = HoldStartNoteList[i];
            // Startのペア（End）をインデックスから求めてセットする
            int pairIndex = holdPairDictionary[holdStartNote].pairIndex;
            // ペアインデックスと同じ場所に、Game版ペアノーツもあるはずである
            NoteData holdEndData = HoldStartNoteList[pairIndex];

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

        if(NoteListListSize > 0)
        {
            // Editorでの時間基準で、最初のノーツの生成時間について考える
            double generateTime = CalcGenerateTime(NoteList[0].finishTime);
            // 生成時間が負の値の場合、更に長く待つ必要がある可能性がある
            if(generateTime < 0)
            {
                double genAbs = Math.Abs(generateTime);
                if (genAbs > GameWaitSec) GameWaitSec = genAbs;
            }
        }

        if (OffsetSec >= 0) MusicStartWaitSec = GameWaitSec + OffsetSec;
    }

    private static void CalcDataTime()
    {
        // 現状のデータのTimeはエディター基準になっている

        // まず、速度倍率ノーツをGameWaitだけ後ろにずらす
        foreach(SpeedScaleData speedScaleData in SpeedScaleList)
        {
            speedScaleData.time += GameWaitSec;
        }

        // 同様にノーツもずらしつつ、生成開始時間を計算
        foreach(NoteData noteData in NoteList)
        {
            noteData.finishTime += GameWaitSec;

        }
    }

    // Utility
    private static double CalcGenerateTime(double finishTime)
    {
        double remainingDistance = OptionData.LaneLength;
        double currentTime = finishTime;

        // 後ろから区間を消費する
        for (int i = SpeedScaleListSize - 1; i >= 0; i--)
        {
            double sectionStart = SpeedScaleList[i].time;
            double sectionEnd = (i + 1 < SpeedScaleListSize)
                ? SpeedScaleList[i + 1].time
                : double.PositiveInfinity;

            if (currentTime <= sectionStart)
                continue;

            double usableEnd = Math.Min(currentTime, sectionEnd);
            double usableDuration = usableEnd - sectionStart;

            double speed = OptionData.BaseNoteSpeed * SpeedScaleList[i].speedScale;
            double maxDistance = speed * usableDuration;

            if (remainingDistance <= maxDistance)
            {
                // 区間途中で完結
                double neededTime = remainingDistance / speed;
                return usableEnd - neededTime;
            }

            // 区間をフルで使う
            remainingDistance -= maxDistance;
            currentTime = sectionStart;
        }

        // まだ距離が残っている場合：
        // 最初の倍率を −∞ まで適用する
        {
            var first = SpeedScaleList[0];
            double speed = OptionData.BaseNoteSpeed * first.speedScale;
            double neededTime = remainingDistance / speed;
            return currentTime - neededTime;
        }
    }
}
