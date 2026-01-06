public static class OptionData
{
    // staticクラスなので、何かオブジェクトにアタッチする必要はありません。
    // OptionData.変数名 またはusing static OptionData して 変数名 でどこからでもアクセスできます。

    #region ユーザー設定
    /// <summary>
    /// 速度倍率1のノーツスピード（/s）
    /// </summary>
    public static float BaseNoteSpeed { get; set; } = 100;

    /// <summary>
    /// ノーツの開始オフセット（ピクセル）
    /// </summary>
    public static float NoteStartOffset { get; set; } = 0;

    /// <summary>
    /// 判定調整（ms）
    /// </summary>
    public static float JudgeOffset { get; set; } = 0;
    #endregion

    #region システム用
    /// <summary>
    /// レーンの縦長さ（ピクセル）
    /// </summary>
    public static float LaneLength { get; private set; } = 1000;

    /// <summary>
    /// 曲を再生開始するまでの待ち時間（ms）
    /// </summary>
    public static int MusicStartWaitTime { get; private set; } = 3;

    /// <summary>
    /// 曲の開始時間オフセット（ms）
    /// JSONから読みだされた値が使われる
    /// </summary>
    public static int MusicOffset { get; set; } = 0;

    public static int GetTotalOffset() => MusicStartWaitTime + MusicOffset;
    #endregion
}
