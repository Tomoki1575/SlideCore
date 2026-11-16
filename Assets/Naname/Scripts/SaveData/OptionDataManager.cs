using System.IO;
using UnityEngine;

public static class OptionDataManager
{
    // 最初のシーンが読み込まれる前（AwakeやStartよりも前）に、シーン上にオブジェクトを配置していなくても指定したメソッドを自動で呼び出せる属性
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadOnStartup()
    {
        Load();
        Debug.Log("[SaveData] OptionDataがロードされました");
    }

    // セーブファイルの形式バージョン。項目を増やしたら上げる。
    private const int SaveVersion = 2;

    private static string FilePath => Path.Combine(Application.persistentDataPath, "OptionSaveData.dat");

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

                writer.Write(PlayerOptionsScript.NotesSpeedSetting);
                writer.Write(PlayerOptionsScript.MusicOffsetSetting);
                writer.Write(PlayerOptionsScript.BGMVolumeSetting);
                writer.Write(PlayerOptionsScript.SEVolumeSetting);
                writer.Write(PlayerOptionsScript.IsTimingFeedbackMode);
            }
        }

        catch (IOException e)
        {
            Debug.LogWarning($"[SaveData] OptionDataのセーブに失敗しました:{e.Message}");
        }
    }

    public static void Load()
    {
        // ファイルがない(初回起動時の)場合、既定値のままでいいため早期return
        if (!File.Exists(FilePath)) return;

        try
        {
            using (FileStream stream = File.Open(FilePath, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // 先頭のバージョンを見て、どう読むかを決める
                int version = reader.ReadInt32();

                if (version > SaveVersion)
                {
                    Debug.LogWarning($"[SaveData] OptionDataにおいて未知のバージョン{version}です。既定値で起動します。");
                    return;
                }

                // ①一旦全部読み切る（変数として書き出すだけで、実際のオプションに適応はしていない。）
                float notesSpeed = ReadFloatSince(reader, version, 1, 2.5f);
                float musicOffset = ReadFloatSince(reader, version, 1, 0f);
                float bgmVolume = ReadFloatSince(reader, version, 1, 100f);
                float seVolume = ReadFloatSince(reader, version, 1, 100f);
                bool isTimingFeedbackMode = ReadBoolSince(reader, version, 2, false);

                // ②ここまで来たら成功。まとめてオプションに反映する。
                PlayerOptionsScript.ApplyLoadedValues(notesSpeed, musicOffset, bgmVolume, seVolume, isTimingFeedbackMode);

                Debug.Log($"[SaveData] Optiondataをロードしました Ver.{version} / オフセット:{musicOffset} / 速度:{notesSpeed} / BGM:{bgmVolume} / SE:{seVolume} / Fast/Late表示:{isTimingFeedbackMode}");
            }
        }

        catch (IOException e)
        {
            Debug.LogWarning($"[SaveData] OptionDataのロードに失敗しました:{e.Message}");
        }
    }

    /// <summary>
    /// [バイナリ] から [C#のfloat変数] に変換する。<br/>その際にversionがsince未満(この記録を作るより前のバージョン)なら、読まずに既定値を返す。<br/>
    /// [重要] : 呼ぶ順番はSaveの書き込み順と一致していること。
    /// </summary>
    private static float ReadFloatSince(BinaryReader reader, int version, int since, float defaultValue)
    {
        if (version < since)
            return defaultValue;

        return reader.ReadSingle();
    }

    /// <summary>
    /// [バイナリ] から [C#のbool変数] に変換する。<br/>その際にversionがsince未満(この記録を作るより前のバージョン)なら、読まずに既定値を返す。<br/>
    /// [重要] : 呼ぶ順番はSaveの書き込み順と一致していること。
    /// </summary>
    private static bool ReadBoolSince(BinaryReader reader, int version, int since, bool defaultValue)
    {
        if (version < since)
            return defaultValue;

        return reader.ReadBoolean();
    }
}
