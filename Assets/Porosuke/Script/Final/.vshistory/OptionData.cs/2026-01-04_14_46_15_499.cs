public static class OptionData
{
    // staticクラスなので、何かオブジェクトにアタッチする必要はありません。
    // OptionData.変数名 またはusing static OptionData して 変数名 でどこからでもアクセスできます。

    #region ユーザー設定
    public static float UserNoteSpeed { get; set; } = 10;
    /// <summary>
    /// 速度倍率1のノーツスピード（/s）
    /// </summary>
    public static float BaseNoteSpeed => UserNoteSpeed * 20;

    /// <summary>
    /// ノーツの開始オフセット（0~100%）
    /// </summary>
    public static int NoteStartOffset { get; set; } = 0;

    /// <summary>
    /// 判定調整（ms）
    /// </summary>
    public static float JudgeOffset { get; set; } = 0;

    public static float BGMVolume { get; set; } = 1;

    public static float SEVolume { get; set; } = 1;
    #endregion

    #region システム用
    /// <summary>
    /// レーンの縦長さ（ピクセル）
    /// </summary>
    public static int LaneLength { get; private set; } = 900;

    /// <summary>
    /// 判定線から完全な終端までの長さ（ピクセル）
    /// </summary>
    public static int LaneAfterLength { get; private set; } = 100;

    /// <summary>
    /// 曲を再生開始するまでのデフォルト待ち時間（s）
    /// </summary>
    public static float MusicStartWaitTime { get; private set; } = 3;

    /// <summary>
    /// 4分音符1つのTicks量（NoteEditorと同じものを用いてください）
    /// </summary>
    public static int TicksPerQuarter { get; private set; } = 960;
    #endregion
}
