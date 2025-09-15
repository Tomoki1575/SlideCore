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

    private float perfectGracePeriod = 300;
    private float greatGracePeriod = 600;
    private float badGracePeriod = 1000;

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
        Mathf.Abs(deltaTime) <= perfectGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber] == true;

    private bool IsGreat() =>
        Mathf.Abs(deltaTime) <= greatGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber] == true;

    private bool IsBad() =>
        Mathf.Abs(deltaTime) <= badGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber] == true;

    private bool IsMiss() =>
        (laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber] == true) || deltaTime < -1000;

    private bool IsActiveNotes() =>  //ここにそのレーン内で最も判定ラインに近いものをアクティブ、そうでないものを非アクティブにする(遥か先にあるのに押して反応してしまうという理不尽を防ぐ)
        deltaTime < 1000;

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
