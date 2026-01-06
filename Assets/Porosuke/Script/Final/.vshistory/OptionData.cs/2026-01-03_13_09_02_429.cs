public static class OptionData
{
    // staticクラスなので、何かオブジェクトにアタッチする必要はありません。
    // OptionData.変数名 またはusing static OptionData して 変数名 でどこからでもアクセスできます。

    #region ユーザー設定
    public static float UserNoteSpeed { get; set; } = 10;
    /// <summary>
    /// 速度倍率1のノーツスピード（/s）
    /// </summary>
    public static float BaseNoteSpeed => UserNoteSpeed * 10;

    /// <summary>
    /// ノーツの開始オフセット（ピクセル）
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
    /// 曲を再生開始するまでのデフォルト待ち時間（s）
    /// </summary>
    public static float MusicStartWaitTime { get; private set; } = 3;
    #endregion
}
