using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum RESULT
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

    [SerializeField] private float notesTime = 10000f;
    [SerializeField] private int laneNumber = 3;

    private KeyControl[] laneKeys;

    private float deltaTime;

    private float perfectGracePeriod = 50;
    private float greatGracePeriod = 100;
    private float badGracePeriod = 180;
    private float activeNotePeriod = 300;

    private RESULT judgmentResult = RESULT.NONE;

    public GameObject tapNotes;

    void Awake()
    {
        laneKeys = new KeyControl[6]
        {
            Keyboard.current.sKey,
            Keyboard.current.dKey,
            Keyboard.current.fKey,
            Keyboard.current.jKey,
            Keyboard.current.kKey,
            Keyboard.current.lKey
        };
    }

    private void Update()
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

    private bool IsPerfect() =>
        Mathf.Abs(deltaTime) <= perfectGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber];

    private bool IsGreat() =>
        Mathf.Abs(deltaTime) <= greatGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber];

    private bool IsBad() =>
        Mathf.Abs(deltaTime) <= badGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber];

    private bool IsMiss()
    {
        bool keyDown = laneKeys[laneNumber].wasPressedThisFrame;
        bool laneActive = IsActiveNotes() && LaneScript.isActiveLane[laneNumber];
        float adt = Mathf.Abs(deltaTime);

        if (deltaTime < -activeNotePeriod) return true;

        //if (keyDown && laneActive && adt > badGracePeriod && adt <= activeNotePeriod) return true;

        return false;
    }

    private bool IsActiveNotes() =>  //ここにそのレーン内で最も判定ラインに近いものをアクティブ、そうでないものを非アクティブにする(遥か先にあるのに押して反応してしまうという理不尽を防ぐ)
        deltaTime < activeNotePeriod;

    private void DebugPanel_DeltaTimeOutput()
    {
        deltaTimeText.text = $"Delta time : {Mathf.FloorToInt(deltaTime)} ms";
    }

    private void DebugPanel_JudgmentResultOutput()
    {
        judgmentResultText.text = $"Judgment Result : {judgmentResult}";
    }

    private void DestroyThisObject()
    {
        Destroy(this.gameObject);
    }
}
