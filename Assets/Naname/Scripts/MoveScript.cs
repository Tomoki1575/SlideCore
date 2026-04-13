using UnityEngine;

public class MoveScript : MonoBehaviour
{
    public float hitTime;
    public int lane;

    public static float scrollSpeed = 6000f;
    private float judgmentLineY = -1900f;

    private RectTransform rectTransform;

    // 理由：NotesGenerator が生成した直後にすぐ RectTransform を使えるようにするため
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 現在ノーツがあるべき場所を計算し、その座標にノーツを置く関数。「ノーツを動かす関数」とも言える。
    /// </summary>
    public void RefreshPosition()
    {
        if (rectTransform == null) 
            return;

        // timeRemaining ←残り何秒で判定ラインに到達すべきかを記録する変数
        float timeRemaining = hitTime - MusicManagerScript.songTime;

        // 「距離 ＝ 時間 × 速さ」 より、現在ノーツがあるべき座標を計算
        float noteYPos = judgmentLineY + (timeRemaining * scrollSpeed);

        // 座標を更新
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, noteYPos);
    }

    void Update()
    {
        RefreshPosition();

        // timeRemaining ←残り何秒で判定ラインに到達すべきかを記録する変数
        float timeRemaining = hitTime - MusicManagerScript.songTime;
    }
}