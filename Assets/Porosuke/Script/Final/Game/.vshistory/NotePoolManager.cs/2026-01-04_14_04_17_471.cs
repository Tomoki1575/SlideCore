using System.Collections.Generic;
using UnityEngine;

public class NotePoolManager : MonoBehaviour
{
    [SerializeField]
    private List<RectTransform> LaneRects;
    [SerializeField]
    private GameObject NotePrefab;

    // ノーツのデータとオブジェクトをセットで管理
    private struct Note
    {
        public GameDataManager.NoteData noteData;
        public Prefab prefab;
    }

    // GameObjectから毎回RectTransformをGetComponentしなくていいように、まとめておく
    private struct Prefab
    {
        public GameObject obj;
        public RectTransform rect;
    }

    private List<Note> ActiveNoteList;

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
        ActiveNoteList = new List<Note>();

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
        prefab.rect = prefab.obj.GetComponent<RectTransform>();
        prefab.rect.anchorMin = new Vector2(0, 1);
        prefab.rect.anchorMax = new Vector2(1, 1);
        return prefab;
    }

    private Note MakeNote(GameDataManager.NoteData noteData, Prefab prefab)
    {
        return new Note
        {
            noteData = noteData,
            prefab = prefab
        };
    }

    // プール管理
    private Prefab GetNote(GameDataManager.NoteData noteData)
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
        prefab.obj.transform.SetParent(LaneRects[noteData.lane], false);
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
