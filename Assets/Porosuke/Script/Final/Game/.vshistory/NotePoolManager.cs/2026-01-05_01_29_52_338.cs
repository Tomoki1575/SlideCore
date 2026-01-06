using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class NotePoolManager : MonoBehaviour
{
    [SerializeField]
    private List<RectTransform> LaneRects;
    [SerializeField]
    private GameObject NotePrefab;

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
    }

    private Queue<Prefab> PrefabPool;

    private const int DefaultPoolSize = 10;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (LaneRects[0], nameof(LaneRects)),
            (LaneRects[1], nameof(LaneRects)),
            (LaneRects[2], nameof(LaneRects)),
            (LaneRects[3], nameof(LaneRects)),
            (LaneRects[4], nameof(LaneRects)),
            (LaneRects[5], nameof(LaneRects)),
            (NotePrefab, nameof(NotePrefab))
            );
    }

    private void Start()
    {
        PrefabPool = new Queue<Prefab>();

        PreparePool();
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
        prefab.nc = prefab.obj.GetComponent<NoteComponent>();
        prefab.nc.SetAnchor(Vector2.up, Vector2.one);
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
        // レーンをセット
        prefab.obj.transform.SetParent(LaneRects[noteData.lane], false);
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
        PrefabPool.Enqueue(prefab);
    }
}
