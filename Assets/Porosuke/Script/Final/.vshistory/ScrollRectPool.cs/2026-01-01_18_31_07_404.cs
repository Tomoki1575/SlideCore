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
    Dictionary<RectTransform, int> rectToIndex = new Dictionary<RectTransform, int>();

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

        trans.GetChild(0).GetComponent<TextMeshProUGUI>().text = dataIndex.ToString();

        var rect = trans as RectTransform;
        rectToIndex[rect] = index;
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
                    item.GetComponent<LayoutElement>().preferredHeight = 135;
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

        bool bPlusDist = false;
        float centerGap = 0;

        foreach (var item in VisibleElements)
        {
            Vector3 itemCenter =
                item.TransformPoint(item.rect.center);

            float dist = Mathf.Abs(itemCenter.y - viewportCenter.y);

            if (dist < minDist)
            {
                bPlusDist = Mathf.Sign(itemCenter.y - viewportCenter.y) == 1;
                minDist = dist;
                centerItem = item;
            }
        }

        Debug.Log(bPlusDist ? minDist : minDist * -1);

        int centerIndex = rectToIndex[centerItem];

        // サイズ確定
        foreach (var item in VisibleElements)
        {
            int index = rectToIndex[item];
            LayoutElement le = item.GetComponent<LayoutElement>();

            if (index == centerIndex + 1)
            {
                le.preferredHeight = 190;
                Debug.Log($"Set Big{index}");
            }
            else le.preferredHeight = 135;
        }

        Debug.Log($"これが中央>{rectToIndex[centerItem]}");
        // スナップ
        ScrollRectComp.RefillCells(rectToIndex[centerItem] - 3);
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