using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

// このコードは以下サイトの完全コピペ
// https://light11.hatenadiary.com/entry/2022/05/16/201949#Loop-Scroll-Rect%E3%81%A8%E3%81%AF

[RequireComponent(typeof(LoopScrollRect))]
[DisallowMultipleComponent]
public sealed class Example : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    [SerializeField] private GameObject _prefab;

    public int totalCount = -1;
    public int dataCount = 100;

    private ObjectPool<GameObject> _pool;

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

        // 要素ごとに高さを変える
        var height = dataIndex % 2 == 0 ? 50 : 100;
        trans.GetComponent<LayoutElement>().preferredHeight = height;

        trans.GetChild(0).GetComponent<TextMeshProUGUI>().text = dataIndex.ToString();
    }

    GameObject LoopScrollPrefabSource.GetObject(int index)
    {
        return _pool.Get();
    }

    void LoopScrollPrefabSource.ReturnObject(Transform trans)
    {
        _pool.Release(trans.gameObject);
    }
}