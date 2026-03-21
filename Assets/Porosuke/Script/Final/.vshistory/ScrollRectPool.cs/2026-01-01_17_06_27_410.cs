using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

// このコードは以下サイトを参考にしました
// https://light11.hatenadiary.com/entry/2022/05/16/201949#Loop-Scroll-Rect%E3%81%A8%E3%81%AF

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
    [SerializeField]
    private RectTransform ContentRect;

    int dataCount = 10;

    private ObjectPool<GameObject> ElementPool;
    private HashSet<RectTransform> VisibleElements = new HashSet<RectTransform>();

    // 状態変数
    private bool IsPointerDown;
    private float LastScrollTime;
    private bool PreviousUpdated;

    private const float SlipTime = 0.15f;

    private void Start()
    {
        if (ElementPrefab == null || ScrollRectComp == null || ViewportRect == null) return;

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
        // -1を指定するとループになる
        ScrollRectComp.totalCount = -1;
        ScrollRectComp.RefillCells();
    }

    void LoopScrollDataSource.ProvideData(Transform trans, int index)
    {
        // ここでデータをセットする
        int dataIndex = (int)Mathf.Repeat(index, dataCount);
        trans.GetComponent<LayoutElement>().preferredHeight = 200;

        trans.GetChild(0).GetComponent<TextMeshProUGUI>().text = dataIndex.ToString();
    }

    GameObject LoopScrollPrefabSource.GetObject(int index)
    {
        // ScrollRectが要素を要求したときに呼ぶ
        GameObject go = ElementPool.Get();
        VisibleElements.Add(go.transform as RectTransform);
        return go;
    }

    void LoopScrollPrefabSource.ReturnObject(Transform trans)
    {
        // ScrollRectが要素を返すときに呼ぶ
        VisibleElements.Remove(trans as RectTransform);
        ElementPool.Release(trans.gameObject);
    }

    void LateUpdate()
    {
        if (IsUserInteracting())
        {
            // もし前フレームまでUpdateしていたなら、全部を200に戻さないと行けない
            if (PreviousUpdated)
            {
                foreach (var item in VisibleElements)
                {
                    item.GetComponent<LayoutElement>().preferredHeight = 200;
                }
                PreviousUpdated = false;
            }
            return;
        }
        if (PreviousUpdated) return;

        PreviousUpdated = true;
        RectTransform centerItem = null;
        float minDist = float.MaxValue;

        // Viewportの中心（ワールド）
        Vector3 viewportCenter =
            ViewportRect.TransformPoint(ViewportRect.rect.center);

        foreach (var item in VisibleElements)
        {
            Vector3 itemCenter =
                item.TransformPoint(item.rect.center);

            float dist = Mathf.Abs(itemCenter.y - viewportCenter.y);

            if (dist < minDist)
            {
                minDist = dist;
                centerItem = item;
            }
        }

        // サイズ確定
        foreach (var item in VisibleElements)
        {
            item.GetComponent<LayoutElement>().preferredHeight =
                item == centerItem ? 300 : 200;
        }

        // レイアウト即時反映
        LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRect);

        // スナップ
        SnapToCenter(centerItem);
    }

    private void SnapToCenter(RectTransform centerItem)
    {
        Vector3 viewportCenter =
            ViewportRect.TransformPoint(ViewportRect.rect.center);

        Vector3 itemCenter =
            centerItem.TransformPoint(centerItem.rect.center);

        float deltaY = viewportCenter.y - itemCenter.y;

        Vector2 pos = ContentRect.anchoredPosition;
        pos.y -= deltaY / ContentRect.lossyScale.y;
        ContentRect.anchoredPosition = pos;
    }


    // イベントハンドラー
    public void OnPointerDown(BaseEventData eventData)
    {
        IsPointerDown = true;
    }

    public void OnPointerUp(BaseEventData eventData)
    {
        IsPointerDown = false;
    }

    public void OnScroll(BaseEventData eventData)
    {
        LastScrollTime = Time.unscaledTime;
    }

    bool IsUserInteracting()
    {
        if (IsPointerDown) return true;

        // ホイール入力の余韻
        if (Time.unscaledTime - LastScrollTime < SlipTime)
            return true;

        return false;
    }
}