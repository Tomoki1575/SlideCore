using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ScoreRecord
{
    public int songID;
    public MusicDataManager.Difficulty difficulty;
    public int maxScore;
    public int maxCombo;
    public ClearBadge bestBadge;
}

public static class ScoreDataManager
{
    // 最初のシーンが読み込まれる前（AwakeやStartよりも前）に、シーン上にオブジェクトを配置していなくても指定したメソッドを自動で呼び出せる属性
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadOnStartup()
    {
        Load();
        Debug.Log("[ScoreData] ロードされました");
    }

    // セーブファイルの形式バージョン。項目を増やしたら上げる。
    private const int SaveVersion = 1;

    private static string FilePath => Path.Combine(Application.persistentDataPath, "ScoreSaveData.dat");

    private static List<ScoreRecord> records = new List<ScoreRecord>();

    public static void Save()
    {
        try
        {
            // ①ファイルを開き(無ければ作成し)
            using (FileStream stream = File.Open(FilePath, FileMode.Create))
            // ②変数係を繋ぐ。どちらも } で自動的に閉められる
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // ③値を順番に書いていく。この順番で読まれるため、最初にバージョンを書く。
                writer.Write(SaveVersion);
                writer.Write(records.Count);

                foreach (ScoreRecord r in records)
                {
                    writer.Write(r.songID);
                    writer.Write((int)r.difficulty);
                    writer.Write(r.maxScore);
                    writer.Write(r.maxCombo);
                    writer.Write((int)r.bestBadge);
                }
            }
        }

        catch (IOException e)
        {
            Debug.LogWarning($"[ScoreData] セーブに失敗しました:{e.Message}");
        }
    }

    public static void Load()
    {
        // ファイルが無い(初回起動の)場合、既定値のままでいいため早期return
        if (!File.Exists(FilePath)) return;

        try
        {
            using (FileStream stream = File.Open(FilePath, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // 先頭のバージョンを見て、どう読むか決める
                int version = reader.ReadInt32();

                if (version > SaveVersion)
                {
                    Debug.LogWarning($"[ScoreData] 未知のバージョン{version}です。規定値で起動します。");
                    return;
                }

                // 要素が何個あるのか見て、その回数回す
                int recordCount = reader.ReadInt32();

                // 読み終わるまで records には触らない。途中で失敗しても既存の記録を壊さないため
                List<ScoreRecord> loaded = new List<ScoreRecord>();

                for (int i = 0; i < recordCount; i++)
                {
                    loaded.Add(new ScoreRecord
                    {
                        songID = reader.ReadInt32(),
                        difficulty = (MusicDataManager.Difficulty)reader.ReadInt32(),
                        maxScore = reader.ReadInt32(),
                        maxCombo = reader.ReadInt32(),
                        bestBadge = (ClearBadge)reader.ReadInt32(),
                    });
                }

                records = loaded;
                Debug.Log($"[ScoreData] {records.Count}件の記録を読み込みました");
            }
        }

        catch (IOException e)
        {
            Debug.LogWarning($"[ScoreData] ロードに失敗しました:{e.Message}");
        }

    }

    /// <summary>
    /// 指定した曲・難易度の記録を返す。まだプレイしていなければnullを返す。
    /// </summary>
    public static ScoreRecord FindRecord(int songID, MusicDataManager.Difficulty difficulty)
    {
        foreach (ScoreRecord r in records)
        {
            if (r.songID == songID && r.difficulty == difficulty)
                return r;
        }

        return null;
    }

    /// <summary>
    /// プレイ済か未プレイか判定し、最高記録を適応する。
    /// </summary>
    public static void UpdateRecord(int songID, MusicDataManager.Difficulty difficulty, int score, int combo, ClearBadge badge)
    {
        if(songID == 0)
            return;        

        ScoreRecord record = FindRecord(songID, difficulty);

        // 初プレイなら、記録を新しく作ってリストに加える
        if (record == null)
        {
            record = new ScoreRecord
            {
                songID = songID,
                difficulty = difficulty,
                bestBadge = ClearBadge.Finish,   // 既定の0(AllPerfect)だと二度と更新されない
            };

            records.Add(record);
        }

        // 項目ごとに独立して最高を取る（称号は数値が小さいほど上位）
        if (score > record.maxScore) record.maxScore = score;
        if (combo > record.maxCombo) record.maxCombo = combo;
        if (badge < record.bestBadge) record.bestBadge = badge;
    }
}
