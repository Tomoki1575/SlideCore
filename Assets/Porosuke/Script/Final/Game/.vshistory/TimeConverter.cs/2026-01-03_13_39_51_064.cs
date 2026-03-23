using System;
using System.Collections.Generic;
using System.Numerics;
using static JsonDataConverter;

public static class TimeConverter
{
    // 小節番号をインデックスとしたSigData配列キャッシュ
    private static SignatureData[] MeasureCacheArray;

    // ticksのキャッシュ
    // Index0から、denominator 1, 2, 4, 8, 16, 32, 64
    private static int[] TicksCacheArray;

    private static List<SignatureData> SignatureDataList;

    private const int MeasureCacheSize = 500;

    // コンストラクタ
    static TimeConverter()
    {
        MeasureCacheArray = new SignatureData[MeasureCacheSize];

        // ticksキャッシュ
        TicksCacheArray = new int[7];
        for (int n = 0; n <= 6; n++)
        {
            int deno = 1 << n; // 2^n
            TicksCacheArray[n] = (OptionData.TicksPerQuarter * 4) / deno;
        }
    }

    public static void SetSignatureDataList(List<SignatureData> signatureDataList)
    {
        SignatureDataList = signatureDataList;

        // Set時点でキャッシュしておく
        int sigIndex = 0;
        for (int m = 0; m < MeasureCacheSize; m++)
        {
            // 次の拍子区間に進む
            if (sigIndex + 1 < SignatureDataList.Count && SignatureDataList[sigIndex + 1].measure <= m)
                sigIndex++;

            MeasureCacheArray[m] = SignatureDataList[sigIndex];
        }
    }

    /// <summary>
    /// 与えられた小節が属する拍子を返します。
    /// </summary>
    /// <param name="measure"></param>
    /// <returns></returns>
    private static SignatureData GetSignatureAtMeasure(int measure)
    {
        // キャッシュにあれば即返す
        if (measure >= 0 && measure < MeasureCacheArray.Length)
        {
            return MeasureCacheArray[measure];
        }

        // 指定小節が属する拍子を返す
        int left = 0;
        int right = SignatureDataList.Count - 1;
        SignatureData result = SignatureDataList[0];

        while (left <= right)
        {
            int mid = (left + right) / 2;
            var sig = SignatureDataList[mid];

            int sigMeasure = sig.measure;

            if (sigMeasure == measure)
            {
                result = sig;
                break;
            }
            else if (sigMeasure < measure)
            {
                result = sig; // measure以下の最大を記録
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
    }

    // -------------------- Converter --------------------

    /// <summary>
    /// 与えられたBeatDataを、総Tickで返します。
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int ConvertBeatToTotalTicks(BeatData target)
    {
        // BeatDataを累積Tickに変換
        int totalTicks = 0;

        // 小節ごとの拍数を累積
        for (int m = 1; m < target.measure; m++)
        {
            SignatureData sig = GetSignatureAtMeasure(m);
            totalTicks += sig.numerator * GetTicksPerBeat(sig);
        }

        // 現在小節内の拍とTickを加算
        totalTicks += (target.beat - 1) * GetTicksPerBeat(target.measure);
        totalTicks += target.tick;

        return totalTicks;
    }

    /// <summary>
    /// 与えられたBeatDataを、RealTimeに変換します。
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static double ConvertBeatToReal(BeatData target, List<BpmData> bpmList)
    {
        int targetTick = ConvertBeatToTotalTicks(target);

        BigInteger totalNumerator = 0;       // 秒の分子
        BigInteger totalDenominator = 1;     // 秒の分母

        int prevTick = ConvertBeatToTotalTicks(bpmList[0].beat);
        int prevBpm = bpmList[0].bpm;

        for (int i = 1; i < bpmList.Count; i++)
        {
            int bpmChangeTick = ConvertBeatToTotalTicks(bpmList[i].beat);

            int deltaTicks;
            if (targetTick < bpmChangeTick)
            {
                deltaTicks = targetTick - prevTick;
                AddInterval(ref totalNumerator, ref totalDenominator, deltaTicks, prevBpm);
                return RationalToTimeSpan(totalNumerator, totalDenominator).TotalSeconds;
            }
            else
            {
                deltaTicks = bpmChangeTick - prevTick;
                AddInterval(ref totalNumerator, ref totalDenominator, deltaTicks, prevBpm);

                prevTick = bpmChangeTick;
                prevBpm = bpmList[i].bpm;
            }
        }

        // 最後の区間
        int remainingTicks = targetTick - prevTick;
        AddInterval(ref totalNumerator, ref totalDenominator, remainingTicks, prevBpm);

        return RationalToTimeSpan(totalNumerator, totalDenominator).TotalSeconds;
    }

    /// <summary>
    /// 区間の時間を分子/分母として加算
    /// seconds = deltaTicks * 60 / (bpm * TicksPerQuarter)
    /// </summary>
    private static void AddInterval(ref BigInteger numerator, ref BigInteger denominator, int deltaTicks, int bpm)
    {
        BigInteger intervalNumerator = deltaTicks * 60; // 秒の分子
        BigInteger intervalDenominator = bpm * OptionData.TicksPerQuarter;

        // 分数の足し算： a/b + c/d = (a*d + b*c) / (b*d)
        numerator = numerator * intervalDenominator + denominator * intervalNumerator;
        denominator *= intervalDenominator;

        // 約分して桁数を抑える
        BigInteger gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
        if (gcd > 1)
        {
            numerator /= gcd;
            denominator /= gcd;
        }
    }

    /// <summary>
    /// 有理数をTimeSpanに変換
    /// </summary>
    private static TimeSpan RationalToTimeSpan(BigInteger numerator, BigInteger denominator)
    {
        // 秒 → TimeSpan.Ticks（100ns）
        BigInteger totalTicks = numerator * TimeSpan.TicksPerSecond / denominator;
        return TimeSpan.FromTicks((long)totalTicks);
    }

    // Utility
    // Getter(複数パターン)

    private static int GetTicksPerBeat(SignatureData signatureData)
    {
        int n = 0;
        int temp = signatureData.denominator;
        while (temp > 1)
        {
            temp >>= 1; // 右に1ビットシフト（2で割る）
            n++;
        }
        return TicksCacheArray[n];
    }

    private static int GetTicksPerBeat(int measure)
    {
        return GetTicksPerBeat(GetSignatureAtMeasure(measure));
    }
}
