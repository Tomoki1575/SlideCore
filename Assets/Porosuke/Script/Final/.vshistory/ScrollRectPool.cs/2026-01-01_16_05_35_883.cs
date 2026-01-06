using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

// このコードは以下サイトの完全コピペ
// https://light11.hatenadiary.com/entry/2022/05/16/201949#Loop-Scroll-Rect%E3%81%A8%E3%81%AF

[RequireComponent(typeof(LoopScrollRect))]
[DisallowMultipleComponent]
public sealed class ScrollRectPool : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    [SerializeField] private GameObject _prefab;

    public int totalCount = -1;
    public int dataCount = 100;

    private ObjectPool<GameObject> _pool;

    private HashSet<RectTransform> visibleItems = new();

    private void Start()
    {
        _pool = new ObjectPool<GameObject>(
            () => Instantiate(_prefab),
            o => o.SetActive(true),
            o =>
            {
                o.transform.SetParent(transform);
                o.SetActive(false);
            });

        var scrollRect = GetComponent<LoopScrollRect>();
        scrollRect.prefabSource = this;
        scrollRect.dataSource = this;
        scrollRect.totalCount = totalCount;
        scrollRect.RefillCells();
    }

    void LoopScrollDataSource.ProvideData(Transform trans, int index)
    {
        var dataIndex = (int)Mathf.Repeat(index, dataCount);

        trans.GetChild(0).GetComponent<TextMeshProUGUI>().text = dataIndex.ToString();
    }

    GameObject LoopScrollPrefabSource.GetObject(int index)
    {
        var go = _pool.Get();
        visibleItems.Add(go.transform as RectTransform);
        return go;
    }

    void LoopScrollPrefabSource.ReturnObject(Transform trans)
    {
        visibleItems.Remove(trans as RectTransform);
        _pool.Release(trans.gameObject);
    }

    void LateUpdate()
    {
        RectTransform centerItem = null;
        float minDist = float.MaxValue;

        foreach (var item in visibleItems)
        {
            Vector3 itemCenter =
                item.TransformPoint(item.rect.center);

            float dist = Mathf.Abs(itemCenter.y - 450);

            if (dist < minDist)
            {
                minDist = dist;
                centerItem = item;
            }
        }

        foreach (var item in visibleItems)
        {
            item.GetComponent<LayoutElement>().preferredHeight =
                item == centerItem ? 300 : 200;
        }
    }
}