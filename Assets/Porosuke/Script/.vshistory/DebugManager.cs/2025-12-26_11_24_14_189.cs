using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NoteDataManager;
using static OptionData;

public class DebugManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform NoteParentRect;
    [SerializeField]
    private TextMeshProUGUI VirtualTimeText;
    [SerializeField]
    private TextMeshProUGUI MusicTimeText;
    [SerializeField]
    private TextMeshProUGUI IsPlayText;
    [SerializeField]
    private TextMeshProUGUI SpeedScaleText;
    [SerializeField]
    private TextMeshProUGUI NoteSpeedText;
    [SerializeField]
    private TextMeshProUGUI LaneLengthText;
    [SerializeField]
    private TextMeshProUGUI MusicWaitText;
    [SerializeField]
    private TextMeshProUGUI CurrentGenerateTimeText;
    [SerializeField]
    private TextMeshProUGUI CurrentFinishTimeText;
    [SerializeField]
    private AudioSource MusicSource;
    [SerializeField]
    private NoteDataManager NoteDataManagerClass;
    [SerializeField]
    private GameObject NotePrefab;

    // ノーツのデータとオブジェクトをセットで管理
    private struct Note
    {
        public NoteData noteData;
        public Prefab prefab;
    }

    // GameObjectから毎回RectTransformをGetComponentしなくていいように、まとめておく
    private struct Prefab
    {
        public GameObject obj;
        public RectTransform rect;
    }

    private List<NoteData> NonGenerateNoteDataList;
    private List<Note> ActiveNoteList;
    private Queue<Prefab> PrefabPool;
    private float VirtualTime;

    private const int DefaultPoolSize = 10;

    void Start()
    {
        if (NoteDataManagerClass && MusicSource) NoteDataManagerClass.TempInit(MusicSource.time);

        VirtualTime = 0;
        PrefabPool = new Queue<Prefab>();
        ActiveNoteList = new List<Note>();
        if (NoteDataManagerClass) NonGenerateNoteDataList = NoteDataManagerClass.GetNoteList();
        else Debug.LogError("Note Data Manager is null");
        PreparePool();

        SetText(NoteSpeedText, $"NoteSpeed: {BaseNoteSpeed}");
        SetText(LaneLengthText, $"LaneLength: {LaneLength}");
        SetText(MusicWaitText, $"MusicWait: {MusicStartWaitTime}");
    }

    private void PreparePool()
    {
        // 最初にある程度生成しておく
        for (int i = 0; i < DefaultPoolSize; ++i) ReleaseNote(CreatePrefab());
    }

    private Prefab CreatePrefab()
    {
        Prefab prefab;
        prefab.obj = Instantiate(NotePrefab);
        if (NoteParentRect) prefab.obj.transform.SetParent(NoteParentRect, false);
        prefab.rect = prefab.obj.GetComponent<RectTransform>();
        prefab.rect.anchorMin = new Vector2(0, 1);
        prefab.rect.anchorMax = new Vector2(1, 1);
        return prefab;
    }

    void Update()
    {
        ControlMusic();
        CheckAddNotes();
        MoveNotes();
        ApplyText();
    }

    private void SetText(TextMeshProUGUI tmp, string text)
    {
        if(tmp != null) tmp.text = text;
    }

    private void ApplyText()
    {
        // 見る価値無し
        if (MusicSource)
        {
            SetText(IsPlayText, $"IsPlay: {MusicSource.isPlaying}");
            SetText(MusicTimeText, $"MusicTime: {MusicSource.time}");
        }
        else
        {
            SetText(IsPlayText, "IsPlay: Source is null");
            SetText(MusicTimeText, "MusicTime: Source is null");
        }
        SetText(VirtualTimeText, $"VTime: {VirtualTime}");
        if (NoteDataManagerClass) SetText(SpeedScaleText, $"SpeedScale: {NoteDataManagerClass.GetCurrentSpeedScale(VirtualTime)}");
        if(ActiveNoteList.Count > 0)
        {
            SetText(CurrentGenerateTimeText, $"Generate: {ActiveNoteList[0].noteData.generateTime}");
            SetText(CurrentFinishTimeText, $"Finish: {ActiveNoteList[0].noteData.finishTime}");
        }
        else
        {
            SetText(CurrentGenerateTimeText, $"Generate: -");
            SetText(CurrentFinishTimeText, $"Finish: -");
        }
    }

    private void ControlMusic()
    {
        // 待機時間中はdeltaTimeで進める
        if(VirtualTime < MusicStartWaitTime)
        {
            VirtualTime += Time.deltaTime;
        }
        else
        {
            if (MusicSource)
            {
                // 再生して同期
                if (MusicSource.isPlaying == false)
                {
                    MusicSource.time = 0;
                    VirtualTime = MusicStartWaitTime + MusicSource.time;
                    MusicSource.Play();
                }
                else VirtualTime = MusicStartWaitTime + MusicSource.time;
            }
        }
    }

    private void CheckAddNotes()
    {
        // 未生成のノーツを走査する（後ろからなのは、Removeによるスキップを防止するため）
        for (int i = NonGenerateNoteDataList.Count - 1; i >= 0; i--)
        {
            NoteData noteData = NonGenerateNoteDataList[i];
            // 生成すべきノーツを見つけたら
            if (VirtualTime >= noteData.generateTime)
            {
                // Debug.Log("Add");
                NonGenerateNoteDataList.RemoveAt(i);
                ActiveNoteList.Add(MakeNote(noteData, GetNote()));
            }
        }
    }

    private void MoveNotes()
    {
        // 画面に表示されているノーツを走査する（後ろから）
        for (int i = ActiveNoteList.Count - 1; i >= 0; i--)
        {
            Note note = ActiveNoteList[i];

            // まだバー到達時間ではないなら
            if (VirtualTime < note.noteData.finishTime)
            {
                // 加算ではなく絶対算出により誤差・ズレを無くす
                note.prefab.rect.anchoredPosition = new Vector2(0, NoteDataManagerClass.PositionAt(note.noteData.generateTime, VirtualTime));
            }
            else
            {
                // Debug.Log("Remove");
                ActiveNoteList.RemoveAt(i);
                ReleaseNote(note.prefab);
            }
        }
    }

    private Note MakeNote(NoteData noteData, Prefab prefab)
    {
        return new Note
        {
            noteData = noteData,
            prefab = prefab
        };
    }

    // プール管理
    private Prefab GetNote()
    {
        if (NotePrefab == null || PrefabPool == null) return new Prefab();

        Prefab prefab;
        if (PrefabPool.Count > 0)
        {
            // プールが余ってるなら借りてくる
            prefab = PrefabPool.Dequeue();
        }
        else
        {
            // プールが空なら新規生成
            prefab = CreatePrefab();
        }
        // 位置を一番上に
        prefab.rect.anchoredPosition = new Vector2(0, 0);
        prefab.obj.SetActive(true);
        return prefab;
    }

    private void ReleaseNote(Prefab prefab)
    {
        // 返却
        prefab.obj.SetActive(false);
        PrefabPool.Enqueue(prefab);
    }
}
