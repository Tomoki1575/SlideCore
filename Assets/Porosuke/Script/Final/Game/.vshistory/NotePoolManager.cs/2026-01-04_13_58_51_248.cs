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


}
