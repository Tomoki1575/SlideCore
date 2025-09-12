using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

enum RESULT
{
    PERFECT,
    GREAT,
    BAD,
    MISS,
    NONE
}

public class JudgeScript : MonoBehaviour
{
    public TextMeshProUGUI deltaTimeText;
    public TextMeshProUGUI judgmentResultText;

    float deltaTime;
    float notesTime = 10000f;

    float perfectGracePeriod = 300;
    float greatGracePeriod = 600;
    float badGracePeriod = 1000;

    RESULT judgmentResult = RESULT.NONE;

    public GameObject tapNotes;

    void Update()
    {
        deltaTime = notesTime - CountUpScript.msGameTime;

        if (IsPerfect())
        {
            judgmentResult = RESULT.PERFECT;

            Debug.Log(judgmentResult);

            DestroyThisObject();
        }

        else if (IsGreat())
        {
            judgmentResult = RESULT.GREAT;

            Debug.Log(judgmentResult);

            DestroyThisObject();
        }


        else if (IsBad())
        {
            judgmentResult = RESULT.BAD;

            Debug.Log(judgmentResult);

            DestroyThisObject();
        }

        else if (IsMiss())
        {
            judgmentResult = RESULT.MISS;

            Debug.Log(judgmentResult);

            DestroyThisObject();
        }

        DebugPanel_DeltaTimeOutput();

        DebugPanel_JudgmentResultOutput();
    }

    bool IsPerfect() =>
        Mathf.Abs(deltaTime) <= perfectGracePeriod && Keyboard.current.jKey.wasPressedThisFrame && IsActiveNotes();

    bool IsGreat() =>
        Mathf.Abs(deltaTime) <= greatGracePeriod && Keyboard.current.jKey.wasPressedThisFrame && IsActiveNotes();

    bool IsBad() =>
        Mathf.Abs(deltaTime) <= badGracePeriod && Keyboard.current.jKey.wasPressedThisFrame && IsActiveNotes();

    bool IsMiss() =>
        (Keyboard.current.jKey.wasPressedThisFrame && IsActiveNotes()) || deltaTime < -1000;

    bool IsActiveNotes() =>  //ここにそのレーン内で最も判定ラインに近いものをアクティブ、そうでないものを非アクティブにする(遥か先にあるのに押して反応してしまうという理不尽を防ぐ)
        deltaTime < 1000;

    void DebugPanel_DeltaTimeOutput()
    {
        deltaTimeText.text = $"Delta time : {Mathf.FloorToInt(deltaTime)} ms";
    }

    void DebugPanel_JudgmentResultOutput()
    {
        judgmentResultText.text = $"Judgment Result : {judgmentResult}";
    }

    void DestroyThisObject()
    {
        Destroy(this.gameObject);
    }
}
