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
    [HideInInspector]
    public int Index;                // このセルが表す virtual index
    [HideInInspector]
    public LayoutElement LayoutElement; // キャッシュ済み LayoutElement
    [HideInInspector]
    public MusicElement MusicElement;

    private void Awake()
    {
        // 一度だけ取得してキャッシュ
        LayoutElement = GetComponent<LayoutElement>();
        MusicElement = GetComponent<MusicElement>();
    }
}

[RequireComponent(typeof(LoopScrollRect))]
[DisallowMultipleComponent]
public sealed class ScrollRectPool : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    [SerializeField]
    private GameObject ElementPrefab;
    [SerializeField]
    private LoopScrollRect ScrollRectComp;
    [SerializeField]
    private RectTransform ViewportRect;

    private ObjectPool<GameObject> ElementPool;
    private HashSet<CellView> VisibleCells = new HashSet<CellView>();
    private List<MusicData> MusicDataList = new List<MusicData>();

    // ユーザー操作状態
    private bool IsPointerDown;
    private float LastScrollTime;
    private bool WasInteractingLastFrame = true;

    private const float SlipTime = 0.15f;
    private const int DefaultElementHeight = 135;
    private const int SelectElementHeight = 190;
    private const int AboveCenterNum = 3;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (ElementPrefab, nameof(ElementPrefab)),
            (ScrollRectComp, nameof(ScrollRectComp)),
            (ViewportRect, nameof(ViewportRect))
        );

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
    public void SetupElements(List<MusicData> musicDataList)
    {
        MusicDataList = musicDataList;
        if (musicDataList.Count == 0) ScrollRectComp.totalCount = 0;
        else ScrollRectComp.totalCount = -1;
        ScrollRectComp.RefillCells();
    }

    // データをセルに反映
    void LoopScrollDataSource.ProvideData(Transform trans, int index)
    {
        if (MusicDataList.Count == 0) return;

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

            // 先頭を設定
            ScrollRectComp.RefillCells(centerIndex - AboveCenterNum);

            // RefllCells 後に高さを正しく再セット
            foreach (var cell in VisibleCells)
            {
                if (cell.Index == centerIndex) // 中央セル
                    SetElementHeight(cell, SelectElementHeight);
                else
                    SetElementHeight(cell, DefaultElementHeight);
            }
        }
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
