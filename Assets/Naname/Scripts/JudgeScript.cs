using UnityEngine;
using UnityEngine.InputSystem;

public class JudgeScript : MonoBehaviour
{
    private MoveScript moveScript;

    private float perfectWindow = 0.05f; // ±50ms
    private float greatWindow = 0.10f;   // ±100ms
    private float goodWindow = 0.25f;    // ±250ms
    private float missWindow = 0.4f;    // ±400ms（これ以降は自動でMiss）

    void Start()
    {

        moveScript = GetComponent<MoveScript>();
    }

    void Update()
    {
        // 判定ライン（0秒）からのズレを計算
        float timeUntilHit = moveScript.hitTime - MusicManagerScript.songTime;

        // A. スルー判定（通り過ぎたかどうか）
        // 絶対値ではなく、そのままの数値が -missWindow より小さくなった時（＝手遅れ）
        if (timeUntilHit < -missWindow)
        {
            Judge("Miss", true);
            return;
        }

        // 2. 入力チェック
        // 今のレーンが「判定ラインのスライド範囲内」に入っているか？
        if (LaneScript.isActiveLane[moveScript.lane])
        {
            CheckInput(timeUntilHit);
        }
    }

    private void CheckInput(float timeUntilHit)
    {
        float absTimeUntilHit = Mathf.Abs(timeUntilHit);


        // 自分がいるレーンのキーが押されたか
        if (GetLaneKeyDown(moveScript.lane))
        {
            bool isLate = false;

            if (timeUntilHit < 0)
            {
                isLate = true;
            }

            if (absTimeUntilHit <= perfectWindow) Judge("Perfect", isLate);

            else if (absTimeUntilHit <= greatWindow) Judge("Great", isLate);

            else if (absTimeUntilHit <= goodWindow) Judge("Good", isLate);

            else if (absTimeUntilHit <= missWindow) Judge("Miss", isLate);
        }
    }

    private void Judge(string result, bool isLate)
    {
        SoundEffectScript.Instance.TapNotesSound();
        // 本来はここでスコア加算やエフェクト生成を呼ぶ
        

        if (isLate)
            Debug.Log($"{result}(Late) (Lane: {moveScript.lane}, absTimeUntilHit: {Mathf.FloorToInt((moveScript.hitTime - MusicManagerScript.songTime) * 1000)}ms)");

        else if (!isLate)
            Debug.Log($"{result}(Fast) (Lane: {moveScript.lane}, absTimeUntilHit: {Mathf.FloorToInt((moveScript.hitTime - MusicManagerScript.songTime) * 1000)}ms)");

        Destroy(this.gameObject);
    }

    // キーボード入力の取得（以前のコードを参考に、InputSystem版）
    private bool GetLaneKeyDown(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0: return Keyboard.current.sKey.wasPressedThisFrame;
            case 1: return Keyboard.current.dKey.wasPressedThisFrame;
            case 2: return Keyboard.current.fKey.wasPressedThisFrame;
            case 3: return Keyboard.current.jKey.wasPressedThisFrame;
            case 4: return Keyboard.current.kKey.wasPressedThisFrame;
            case 5: return Keyboard.current.lKey.wasPressedThisFrame;
            default: return false;
        }
    }
}