using TMPro;
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
    private ObjectPool<GameObject> _pool;

    private void Start()
    {
        // オブジェクトプールを作成
        _pool = new ObjectPool<GameObject>(
            // オブジェクト生成処理
            () => Instantiate(_prefab),
            // オブジェクトがプールから取得される時の処理
            o => o.SetActive(true),
            // オブジェクトがプールに戻される時の処理
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
        trans.GetChild(0).GetComponent<TextMeshProUGUI>().text = index.ToString();
    }

    GameObject LoopScrollPrefabSource.GetObject(int index)
    {
        // オブジェクトプールからGameObjectを取得
        return _pool.Get();
    }

    void LoopScrollPrefabSource.ReturnObject(Transform trans)
    {
        // オブジェクトプールにGameObjectを返却
        _pool.Release(trans.gameObject);
    }
}