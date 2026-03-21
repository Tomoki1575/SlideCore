using UnityEngine;

public static class OptionData
{
    // staticクラスなので、何かオブジェクトにアタッチする必要はありません。
    // OptionData.変数名 またはusing static OptionData して 変数名 でどこからでもアクセスできます。

    #region ユーザー設定
    /// <summary>
    /// 速度倍率1のノーツスピード（/s）
    /// </summary>
    public static float BaseNoteSpeed = 100;

    /// <summary>
    /// ノーツの開始オフセット（ピクセル）
    /// </summary>
    public static float NoteStartOffset = 0;

    /// <summary>
    /// 判定調整（ms）
    /// </summary>
    public static float JudgeOffset = 0;
    #endregion

    #region システム用
    /// <summary>
    /// レーンの縦長さ（ピクセル）
    /// </summary>
    public static float LaneLength = 1000;

    /// <summary>
    /// 曲を再生開始するまでの待ち時間（s）
    /// </summary>
    public static float MusicStartWaitTime = 3;
    #endregion
}
