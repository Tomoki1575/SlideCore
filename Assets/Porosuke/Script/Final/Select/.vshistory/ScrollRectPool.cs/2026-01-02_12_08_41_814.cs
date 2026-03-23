using System;
using System.Collections.Generic;
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
    [HideInInspector]
    public MusicData MusicData;

    private void Awake()
    {
        // 一度だけ取得してキャッシュ
        LayoutElement = GetComponent<LayoutElement>();
        MusicElement = GetComponent<MusicElement>();
    }
}

[RequireComponent(typeof(LoopScrollRect))]
[DisallowMultipleComponent]
public sealed class ScrollRectPool : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
    [SerializeField]
    private GameObject ElementPrefab;
    [SerializeField]
    private LoopScrollRect ScrollRectComp;
    [SerializeField]
    private RectTransform ViewportRect;

    public MusicData LastCenterMusic { get; private set; } = null;
    public event Action OnSelectMusicDataChanged;

    // ローカル
    private ObjectPool<GameObject> ElementPool;
    private HashSet<CellView> VisibleCells = new HashSet<CellView>();
    private List<MusicData> MusicDataList = new List<MusicData>();
    private MusicDataManager.Difficulty Difficulty;
    private Color LevelColor;

    // ユーザー操作状態
    private bool IsPointerDown;
    private float LastScrollTime;
    private bool WasInteractingLastFrame = true;

    // 定数
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

    public void SetupElements(List<MusicData> musicDataList, MusicDataManager.Difficulty difficulty, Color levelColor)
    {
        MusicDataList = musicDataList;
        Difficulty = difficulty;
        LevelColor = levelColor;

        if (musicDataList.Count == 0) ScrollRectComp.totalCount = 0;
        else ScrollRectComp.totalCount = -1;

        if (LastCenterMusic == null) ScrollRectComp.RefillCells();

        SetCenterByMusicData(LastCenterMusic);
    }

    private void OnElementPressed(CellView cell)
    {
        if (cell == null) return;
        SetCenterByMusicData(cell.MusicData);
    }

    private void SetCenterByMusicData(MusicData musicData)
    {
        int centerVirtualIndex = 0; // デフォルトは先頭
        // 以前の中央データがある場合
        if (musicData != null && MusicDataList.Contains(musicData))
        {
            int dataIndex = MusicDataList.IndexOf(musicData);
            // virtual index = dataIndex + n * MusicDataList.Count の形でループ内で対応
            centerVirtualIndex = dataIndex; // ループで -AboveCenterNum するときに補正
        }
        // 強制再描画
        CalcAndSnapCenterElement(centerVirtualIndex);
    }

    // データをセルに反映
    void LoopScrollDataSource.ProvideData(Transform trans, int index)
    {
        if (MusicDataList.Count == 0) return;

        var cell = trans.GetComponent<CellView>();
        cell.Index = index; // index をセルにセット

        int dataIndex = (int)Mathf.Repeat(index, MusicDataList.Count);
        // データをセットする
        cell.MusicElement.SetElementData(MusicDataList[dataIndex], Difficulty, LevelColor);
        if (cell.MusicData != MusicDataList[dataIndex]) cell.MusicData = MusicDataList[dataIndex];
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
            cell.MusicElement.OnElementPressed += OnElementPressed;
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
                ResetVisibleElements();
                WasInteractingLastFrame = true;
            }
            return;
        }
        // 操作 -> 非操作 遷移時の1度だけ
        else if (WasInteractingLastFrame)
        {
            CalcAndSnapCenterElement();
        }
    }

    private void CalcAndSnapCenterElement(int virtualIndexToCenter = -1)
    {
        if (MusicDataList.Count == 0) return;
        WasInteractingLastFrame = false;

        // まず ScrollRect を仮想 index でリフィル
        if (virtualIndexToCenter >= 0)
        {
            ScrollRectComp.RefillCells(virtualIndexToCenter - AboveCenterNum);
        }

        CellView centerCell = null;
        // ビューポート中心（ワールド座標）
        Vector3 viewportCenter = ViewportRect.TransformPoint(ViewportRect.rect.center);

        // 画面の中心に最も近い要素を求める
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

        int centerIndex = centerCell.Index;

        // 先頭を設定
        ScrollRectComp.RefillCells(centerIndex - AboveCenterNum);

        // RefllCells 後に高さを正しく再セット
        foreach (var cell in VisibleCells)
        {
            if (cell.Index == centerIndex)
            {
                SetElementHeight(cell, SelectElementHeight);
                cell.MusicElement.SetBackPanelAlPha(true);
            }

            else
            {
                SetElementHeight(cell, DefaultElementHeight);
                cell.MusicElement.SetBackPanelAlPha(false);
            }
        }

        // 次回の SetupElements 用に中央データを保存
        int centerDataIndex = (int)Mathf.Repeat(centerIndex, MusicDataList.Count);
        if (LastCenterMusic != MusicDataList[centerDataIndex])
        {
            LastCenterMusic = MusicDataList[centerDataIndex];
            OnSelectMusicDataChanged?.Invoke();
        }
    }

    private void ResetVisibleElements()
    {
        foreach (var cell in VisibleCells)
        {
            SetElementHeight(cell, DefaultElementHeight);
            cell.MusicElement.SetBackPanelAlPha(false);
        }
    }

    private void SetElementHeight(CellView cell, int height)
    {
        if (cell == null) return;
        if (cell.LayoutElement.preferredHeight != height)
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
    public void OnPointerDown(PointerEventData eventData) => IsPointerDown = true;
    public void OnPointerUp(PointerEventData eventData) => IsPointerDown = false;
    public void OnScroll(PointerEventData eventData) => LastScrollTime = Time.unscaledTime;
}
