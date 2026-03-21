using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

// セル専用コンポーネント
// ElementPrefab に必ずアタッチされる
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

    private ObjectPool<GameObject> ElementPool;
    private HashSet<CellView> VisibleCells = new HashSet<CellView>();
    private List<MusicData> MusicDataList = new List<MusicData>();
    private bool bMissigCompoent = false;

    // Snap 用
    private bool bNeedSnap = false;
    private int SnapIndex = 0;

    // ユーザー操作状態
    private bool IsPointerDown;
    private float LastScrollTime;
    private bool WasInteractingLastFrame = true;

    private const float SlipTime = 0.15f;
    private const int DefaultElementHeight = 135;
    private const int SelectElementHeight = 190;

    private void Awake()
    {
        if (ElementPrefab == null || ScrollRectComp == null || ViewportRect == null)
        {
            bMissigCompoent = true;
            Debug.LogError("Some Component is Missing");
            return;
        }

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
    }

    // 失敗したらFalseを返す
    public bool SetupElements(List<MusicData> musicDataList)
    {
        if (bMissigCompoent) return false;
        MusicDataList = musicDataList;
        ScrollRectComp.RefillCells();
        return true;
    }

    // データをセルに反映
    void LoopScrollDataSource.ProvideData(Transform trans, int index)
    {
        if (MusicDataList == null || MusicDataList.Count == 0) return;

        Debug.Log("Update Data");
        var cell = trans.GetComponent<CellView>();
        cell.Index = index; // index をセルにセット

        int dataIndex = (int)Mathf.Repeat(index, MusicDataList.Count);
        trans.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Data {dataIndex}";
        trans.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Virtual {index}";
    }

    // LoopScrollRect がセルを要求した時
    GameObject LoopScrollPrefabSource.GetObject(int index)
    {
        GameObject go = ElementPool.Get();

        // CellView がまだ付いていなければ追加
        var cell = go.GetComponent<CellView>();
        if (cell == null)
        {
            cell = go.AddComponent<CellView>();
            cell.LayoutElement = go.GetComponent<LayoutElement>(); // LayoutElement をキャッシュ
        }

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
        // ScrollRectの要素決定後に処理を行う
        if (IsUserInteracting())
        {
            // 非操作 -> 操作 遷移時の1度だけ
            if (!WasInteractingLastFrame)
            {
                ResetVisibleElementsHeight();
                WasInteractingLastFrame = true;
            }
            return;
        }
        else if (WasInteractingLastFrame)
        {
            Debug.Log("Calc Center");
            // 操作 -> 非操作 遷移時の1度だけ
            WasInteractingLastFrame = false;
            if (VisibleCells.Count == 0) return;

            // ビューポート中心（ワールド座標）
            Vector3 viewportCenter = ViewportRect.TransformPoint(ViewportRect.rect.center);

            // 画面の中心に最も近い要素を求める
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
                    SetElementHeight(cell, SelectElementHeight);
                else
                    SetElementHeight(cell, DefaultElementHeight);
            }

            // Snap は次フレームで行う
            SnapIndex = centerIndex - 3;
            bNeedSnap = true;
        }
    }

    void Update()
    {
        // 中央スナップ要求があったら、1度だけ再描画する
        if (!bNeedSnap) return;
        bNeedSnap = false;
        ScrollRectComp.RefillCells(SnapIndex);
        Debug.Log("Snap Center");
    }

    private void ResetVisibleElementsHeight()
    {
        foreach (var cell in VisibleCells)
            SetElementHeight(cell, DefaultElementHeight);
    }

    private void SetElementHeight(CellView cell, int height)
    {
        if (cell == null) return;
        if(cell.LayoutElement.preferredHeight != height)
            cell.LayoutElement.preferredHeight = height;
    }

    public bool IsUserInteracting()
    {
        // ドラッグしている（動かしていなくても判定するためにPointerDown）
        if (IsPointerDown) return true;
        // 最後にスクロールしてから一定時間経っていない
        if (Time.unscaledTime - LastScrollTime < SlipTime) return true;
        return false;
    }

    // イベントハンドラー
    public void OnPointerDown(BaseEventData eventData) => IsPointerDown = true;
    public void OnPointerUp(BaseEventData eventData) => IsPointerDown = false;
    public void OnScroll(BaseEventData eventData) => LastScrollTime = Time.unscaledTime;
}
