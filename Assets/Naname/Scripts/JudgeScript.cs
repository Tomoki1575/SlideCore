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

public enum NotesType
{
    NONE,
    TAP,
    HOLD,
    SLIDE,
    NOISE
};

public class JudgeScript : MonoBehaviour
{
    [Header("text")]
    public TextMeshProUGUI deltaTimeText;
    public TextMeshProUGUI judgmentResultText;

    [Header("notesVariable")]
    [SerializeField] private float notesTime = 10000f;
    [SerializeField] private int laneNumber = 3;
    [SerializeField] private NotesType noteTipe = NotesType.NONE;
    private RESULT judgmentResult = RESULT.NONE;

    private KeyControl[] laneKeys;

    private float deltaTime;

    [Header("judge")]
    [SerializeField] private float perfectGracePeriod = 50;
    [SerializeField] private float greatGracePeriod = 100;
    [SerializeField] private float badGracePeriod = 180;
    [SerializeField] private float activeNotePeriod = 300;

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

            Destroy(this.gameObject);
        }

        else if (IsGreat())
        {
            judgmentResult = RESULT.GREAT;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }


        else if (IsBad())
        {
            judgmentResult = RESULT.BAD;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }

        else if (IsMiss())
        {
            judgmentResult = RESULT.MISS;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }

        DebugPanel_Output();
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

        return false;
    }

    private bool IsActiveNotes() =>  //ここにそのレーン内で最も判定ラインに近いものをアクティブ、そうでないものを非アクティブにする(遥か先にあるのに押して反応してしまうという理不尽を防ぐ)
        Mathf.Abs(deltaTime) < activeNotePeriod;

    private void DebugPanel_Output()
    {
        deltaTimeText.text = $"Delta time : {Mathf.FloorToInt(deltaTime)} ms";

        judgmentResultText.text = $"Judgment Result : {judgmentResult}";
    }
}
