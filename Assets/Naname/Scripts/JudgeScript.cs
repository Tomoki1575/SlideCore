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

    void Update()
    {
        deltaTime = notesTime - CountUpScript.msGameTime;

        deltaTimeText.text = $"Delta time : {Mathf.FloorToInt(deltaTime)} ms";

        if (IsPerfect())
        {
            judgmentResult = RESULT.PERFECT;

            Debug.Log(judgmentResult);
        }

        else if (IsGreat())
        {
            judgmentResult = RESULT.GREAT;

            Debug.Log(judgmentResult);
        }
  

        else if (IsBad())
        {
            judgmentResult = RESULT.BAD;

            Debug.Log(judgmentResult);
        }

        else if (IsMiss())        
            judgmentResult = RESULT.MISS;        

        judgmentResultText.text = $"Judgment Result : {judgmentResult}";
    }

    bool IsActiveNotes() =>  //ここにそのレーン内で最も判定ラインに近いものをアクティブ、そうでないものを非アクティブにする
        deltaTime < 1000;       

    bool IsPerfect() =>
        Mathf.Abs(deltaTime) <= perfectGracePeriod && Keyboard.current.jKey.wasPressedThisFrame && IsActiveNotes();

    bool IsGreat() =>
        Mathf.Abs(deltaTime) <= greatGracePeriod && Keyboard.current.jKey.wasPressedThisFrame && IsActiveNotes();

    bool IsBad() =>
        Mathf.Abs(deltaTime) <= badGracePeriod && Keyboard.current.jKey.wasPressedThisFrame && IsActiveNotes();

    bool IsMiss() =>
        IsActiveNotes();
}
