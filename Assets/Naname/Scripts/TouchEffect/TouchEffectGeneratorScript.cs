using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchEffectGeneratorScript : MonoBehaviour
{
    [Header("波紋をアタッチ")]
    [SerializeField] private RectTransform rippleEffect;

    private RectTransform canvasRect;

    private static TouchEffectGeneratorScript instance;

    private const string GameSceneName = "GameScene";
    private bool canGenerate = true;

    private void Awake()
    {
        // シングルトンの処理
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        // canvasを生成
        GameObject canvasGenerate = new GameObject("TouchEffectCanvas");
        Canvas touchEffectCanvas = canvasGenerate.AddComponent<Canvas>();
        canvasGenerate.AddComponent<CanvasScaler>();

        // 表示優先度を設定
        touchEffectCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        touchEffectCanvas.sortingOrder = 150;

        // エフェクトのジェネレーターと今回生成したキャンバスで親子関係を結ぶ
        canvasGenerate.transform.SetParent(this.transform, false);

        canvasRect = canvasGenerate.GetComponent<RectTransform>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (!canGenerate) return;

        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
            return;

        // 2D平面において、今タッチされた場所を代入
        Vector2 screenPos = Pointer.current.position.ReadValue();

        // スクリーン座標を、Canvas上のローカル座標に変換する
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPos))
            return;

        RectTransform obj = Instantiate(rippleEffect, canvasRect);
        obj.anchoredPosition = localPos;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        canGenerate = (scene.name != GameSceneName);
    }
}
