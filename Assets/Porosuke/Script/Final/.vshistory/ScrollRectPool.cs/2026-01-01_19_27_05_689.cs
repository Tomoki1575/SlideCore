using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

// セル専用コンポーネント
// ElementPrefab に必ずアタッチしておくこと
public class CellView : MonoBehaviour
{
    [HideInInspector] public int Index;                // このセルが表す virtual index
    [HideInInspector] public LayoutElement LayoutElement; // キャッシュ済み LayoutElement

    private void Awake()
    {
        // 一度だけ取得してキャッシュ
        LayoutElement = GetComponent<LayoutElement>();
    }
}

[RequireComponent(typeof(LoopScrollRect))]
[DisallowMultipleComponent]
public sealed class ScrollRectPool : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    [SerializeField] private GameObject ElementPrefab;
    [SerializeField] private LoopScrollRect ScrollRectComp;
    [SerializeField] private RectTransform ViewportRect;

    int dataCount = 10;

    private ObjectPool<GameObject> ElementPool;
    private HashSet<CellView> VisibleCells = new HashSet<CellView>();

    // Snap 用
    private bool bNeedSnap = false;
    private int SnapIndex = 0;

    // ユーザー操作状態
    private bool IsPointerDown;
    private float LastScrollTime;
    private const float SlipTime = 0.15f;

    private void Start()
    {
        if (ElementPrefab == null || ScrollRectComp == null || ViewportRect == null) return;

        // プール初期化
        ElementPool = new ObjectPool<GameObject>(
            () => Instantiate(ElementPrefab),
            o => o.SetActive(true),
            o =>
            {
                o.transform.SetParent(transform);
                o.SetActive(false);
            });

        ScrollRectComp.prefabSource = this;
        ScrollRectComp.dataSource = this;
        ScrollRectComp.totalCount = -1;
        ScrollRectComp.RefillCells();
    }

    // データをセルに反映
    void LoopScrollDataSource.ProvideData(Transform trans, int index)
    {
        var cell = trans.GetComponent<CellView>();
        cell.Index = index; // index をセルにセット

        int dataIndex = (int)Mathf.Repeat(index, dataCount);
        trans.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Data {dataIndex}";
        trans.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Virtual {index}";
    }

    // LoopScrollRect がセルを要求した時
    GameObject LoopScrollPrefabSource.GetObject(int index)
    {
        GameObject go = ElementPool.Get();
        var cell = go.GetComponent<CellView>();
        VisibleCells.Add(cell);
        return go;
    }

    // LoopScrollRect がセルを返却した時
    void LoopScrollPrefabSource.ReturnObject(Transform trans)
    {
        var cell = trans.GetComponent<CellView>();
        VisibleCells.Remove(cell);
        ElementPool.Release(trans.gameObject);
    }

    void LateUpdate()
    {
        if (IsUserInteracting())
        {
            ResetVisibleSizes();
            return;
        }

        if (VisibleCells.Count == 0) return;

        // ビューポート中心（ワールド座標）
        Vector3 viewportCenter = ViewportRect.TransformPoint(ViewportRect.rect.center);

        CellView centerCell = null;
        float minDist = float.MaxValue;

        foreach (var cell in VisibleCells)
        {
            Vector3 cellCenter = cell.transform.TransformPoint(cell.GetComponent<RectTransform>().rect.center);
            float dist = Mathf.Abs(cellCenter.y - viewportCenter.y);
            if (dist < minDist)
            {
                minDist = dist;
                centerCell = cell;
            }
        }

        if (centerCell == null) return;

        int centerIndex = centerCell.Index;

        // セルサイズ更新
        foreach (var cell in VisibleCells)
        {
            if (cell.Index == centerIndex)
                cell.LayoutElement.preferredHeight = 190;
            else
                cell.LayoutElement.preferredHeight = 135;
        }

        // Snap は次フレームに回す
        SnapIndex = centerIndex - 3;
        bNeedSnap = true;
    }

    void Update()
    {
        if (!bNeedSnap) return;
        bNeedSnap = false;
        ScrollRectComp.RefillCells(SnapIndex);
    }

    // 全セルを通常サイズに戻す
    void ResetVisibleSizes()
    {
        foreach (var cell in VisibleCells)
        {
            cell.LayoutElement.preferredHeight = 135;
        }
    }

    // イベントハンドラー
    public void OnPointerDown(BaseEventData eventData) => IsPointerDown = true;
    public void OnPointerUp(BaseEventData eventData) => IsPointerDown = false;
    public void OnScroll(BaseEventData eventData) => LastScrollTime = Time.unscaledTime;

    bool IsUserInteracting()
    {
        if (IsPointerDown) return true;
        if (Time.unscaledTime - LastScrollTime < SlipTime) return true;
        return false;
    }
}
