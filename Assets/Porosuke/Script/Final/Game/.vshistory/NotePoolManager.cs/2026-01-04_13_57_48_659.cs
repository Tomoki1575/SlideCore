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
            (LaneRects[0], nameof(LaneRects[0])),
            (LaneRects[1], nameof(LaneRects[1])),
            (LaneRects[2], nameof(LaneRects[2])),
            (LaneRects[3], nameof(LaneRects[3])),
            (LaneRects[4], nameof(LaneRects[4])),
            (LaneRects[5], nameof(LaneRects[5])),
            (NotePrefab, nameof(NotePrefab))
            );
    }


}
