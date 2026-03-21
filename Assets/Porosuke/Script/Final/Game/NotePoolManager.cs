using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class NotePoolManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform LaneRect;
    [SerializeField]
    private RectTransform HoldEndLaneRect;
    [SerializeField]
    private GameObject TapNotePrefab;
    [SerializeField]
    private GameObject HoldNotePrefab;
    [SerializeField]
    private GameObject HoldEndNotePrefab;
    [SerializeField]
    private GameObject SlideNotePrefab;
    [SerializeField]
    private GameObject NoiseNotePrefab;

    // ノーツのデータとオブジェクトをセットで管理
    public struct Note
    {
        public NoteData noteData;
        public Prefab prefab;
    }

    // GameObjectから毎回NoteComponentをGetComponentしなくていいように、まとめておく
    public struct Prefab
    {
        public GameObject obj;
        public NoteComponent nc;
        public NoteType type;
    }

    private Queue<Prefab> TapPrefabPool;

    private Queue<Prefab> HoldPrefabPool;

    private Queue<Prefab> HoldEndPrefabPool;

    private Queue<Prefab> SlidePrefabPool;

    private Queue<Prefab> NoisePrefabPool;

    private const int DefaultPoolSize = 10;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (LaneRect, nameof(LaneRect)),
            (HoldEndLaneRect, nameof(HoldEndLaneRect)),
            (TapNotePrefab, nameof(TapNotePrefab)),
            (HoldNotePrefab, nameof(HoldNotePrefab)),
            (HoldEndNotePrefab, nameof(HoldEndNotePrefab)),
            (SlideNotePrefab, nameof(SlideNotePrefab)),
            (NoiseNotePrefab, nameof(NoiseNotePrefab))
            );
    }

    private void Start()
    {
        TapPrefabPool = new Queue<Prefab>();
        HoldPrefabPool = new Queue<Prefab>();
        HoldEndPrefabPool = new Queue<Prefab>();
        SlidePrefabPool = new Queue<Prefab>();
        NoisePrefabPool = new Queue<Prefab>();
        PreparePool();
    }

    private void PreparePool()
    {
        // 最初にある程度生成しておく
        for (int i = 0; i < DefaultPoolSize; ++i)
        {
            ReleaseNote(CreatePrefab(NoteType.Tap));
            ReleaseNote(CreatePrefab(NoteType.Hold));
            ReleaseNote(CreatePrefab(NoteType.HoldEnd));
            ReleaseNote(CreatePrefab(NoteType.Slide));
            ReleaseNote(CreatePrefab(NoteType.Noise));
        }
    }

    private Prefab CreatePrefab(NoteType type)
    {
        Prefab prefab;
        switch (type)
        {
            case NoteType.Tap:
                prefab.obj = Instantiate(TapNotePrefab);
                break;
            case NoteType.Hold:
                prefab.obj = Instantiate(HoldNotePrefab);
                break;
            case NoteType.HoldEnd:
                prefab.obj = Instantiate(HoldEndNotePrefab);
                break;
            case NoteType.Slide:
                prefab.obj = Instantiate(SlideNotePrefab);
                break;
            case NoteType.Noise:
                prefab.obj = Instantiate(NoiseNotePrefab);
                break;
            default:
                prefab.obj = Instantiate(TapNotePrefab);
                break;
        }
        prefab.type = type;
        prefab.nc = prefab.obj.GetComponent<NoteComponent>();
        // HoldEndだけは別のRectに描画する（描画順の関係）
        prefab.obj.transform.SetParent(type == NoteType.HoldEnd ? HoldEndLaneRect : LaneRect, false);
        prefab.nc.SetAnchor(Vector2.up, Vector2.up);
        return prefab;
    }

    public Note MakeNote(NoteData noteData, Prefab prefab)
    {
        return new Note
        {
            noteData = noteData,
            prefab = prefab
        };
    }

    // プール管理
    public Prefab GetNote(NoteData noteData)
    {
        Prefab prefab;
        if (GetPoolByType(noteData.type).Count > 0)
        {
            // プールが余ってるなら借りてくる
            prefab = GetPoolByType(noteData.type).Dequeue();
        }
        else
        {
            // プールが空なら新規生成
            prefab = CreatePrefab(noteData.type);
        }
        // Xをセット
        prefab.nc.SetPositionX(GetPositionXByLane((int)LaneRect.rect.width, noteData.lane));
        // 位置を一番上に
        prefab.nc.SetPositionY(0);
        // 見た目をセット
        prefab.nc.SetNoteView(noteData.bSpecial, noteData.bIsRight);
        prefab.obj.SetActive(true);
        return prefab;
    }

    public void ReleaseNote(Prefab prefab)
    {
        // 返却
        prefab.obj.SetActive(false);
        GetPoolByType(prefab.type).Enqueue(prefab);
    }

    private Queue<Prefab> GetPoolByType(NoteType type)
    {
        switch (type)
        {
            case NoteType.Tap:
                return TapPrefabPool;
            case NoteType.Hold:
                return HoldPrefabPool;
            case NoteType.HoldEnd:
                return HoldEndPrefabPool;
            case NoteType.Slide:
                return SlidePrefabPool;
            case NoteType.Noise:
                return NoisePrefabPool;
            default:
                return TapPrefabPool;
        }
    }

    public static int GetPositionXByLane(int rectWidth, int lane)
    {
        int laneWidth = rectWidth / OptionData.LaneNum;

        return (laneWidth / 2) + (laneWidth * lane);
    }
}
