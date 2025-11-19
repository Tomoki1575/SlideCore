using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static NoteDataManager;

public enum Result
{
    Perfect,
    Great,
    Bad,
    Miss,
    None
}

public class JudgeScript : MonoBehaviour
{
    public NoteData note;

    [Header("text")]
    public TextMeshProUGUI deltaTimeText;
    public TextMeshProUGUI judgmentResultText;

    [Header("notesVariable")]
    //public float generateTime;  // 生成する時間
    [SerializeField] private float notesTime = 10000f;
    [SerializeField] private int laneNumber = 3;
    [SerializeField] private NoteDataManager.NoteType noteType = NoteDataManager.NoteType.None;
    //public bool bSpecial;       // 特殊ノーツか
    //public float duration;      // Holdノーツのみで使用、それ以外では0
    public bool bIsRight;       // Slideノーツのみで使用、それ以外ではfalse

    private Result judgmentResult = Result.None;

    private KeyControl[] laneKeys;

    private float deltaTime;

    [Header("judge")]
    [SerializeField] private float PerfectGracePeriod = 50;
    [SerializeField] private float GreatGracePeriod = 100;
    [SerializeField] private float BadGracePeriod = 180;
    [SerializeField] private float activeNotePeriod = 300;

    public GameObject TapNotes;

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
        calcDeltaTime();

        switch (noteType)
        {
            case NoteDataManager.NoteType.Tap:                
                TapNoteJudge();
                break;
        }

        DebugPanel_Output();
    }

    private void TapNoteJudge()
    {
        if (IsPerfect())
        {
            judgmentResult = Result.Perfect;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }

        else if (IsGreat())
        {
            judgmentResult = Result.Great;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }


        else if (IsBad())
        {
            judgmentResult = Result.Bad;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }

        else if (IsMiss())
        {
            judgmentResult = Result.Miss;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }
    }

    private KeyControl GetSlideKey(NoteData note)
    {
        // 左右スライドでキーを変えるなら
        return note.bIsRight
            ? Keyboard.current.rightArrowKey   // 右スライド
            : Keyboard.current.leftArrowKey;   // 左スライド
    }

    private void SlideNoteJudge()
    {
        KeyControl a = note.bIsRight ? Keyboard.current.rightArrowKey : Keyboard.current.leftArrowKey;

        if (Mathf.Abs(deltaTime) <= PerfectGracePeriod
            && a.wasPressedThisFrame
            && IsActiveNotes()
            && LaneScript.isActiveLane[note.lane])
        {
            judgmentResult = Result.Perfect;

            Destroy(this.gameObject);
        }


        else if (IsGreat())
        {
            judgmentResult = Result.Great;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }


        else if (IsBad())
        {
            judgmentResult = Result.Bad;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }

        else if (IsMiss())
        {
            judgmentResult = Result.Miss;

            Debug.Log(judgmentResult);

            Destroy(this.gameObject);
        }
    }

    private void calcDeltaTime()=>    
        deltaTime = notesTime - CountUpScript.msGameTime;    

    private bool IsPerfect() =>
        Mathf.Abs(deltaTime) <= PerfectGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber];

    private bool IsGreat() =>
        Mathf.Abs(deltaTime) <= GreatGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber];

    private bool IsBad() =>
        Mathf.Abs(deltaTime) <= BadGracePeriod && laneKeys[laneNumber].wasPressedThisFrame && IsActiveNotes() && LaneScript.isActiveLane[laneNumber];

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
