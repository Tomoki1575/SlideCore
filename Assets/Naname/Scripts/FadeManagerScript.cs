using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManagerScript : MonoBehaviour
{
    private Canvas fadeCanvas;
    private Image fadePanel;
    private Coroutine coroutine;

    private Color panelColor = new Color(0, 0, 0, 1);
    private float fadeTime = 1f;

    public static FadeManagerScript Instance { get; private set; }

    private void Awake()
    {
        // シングルトンの処理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // canvasを生成
        GameObject canvasGenerate = new GameObject("FadeCanvas");
        fadeCanvas = canvasGenerate.AddComponent<Canvas>();
        canvasGenerate.AddComponent<CanvasScaler>();
        canvasGenerate.AddComponent<GraphicRaycaster>();

        // cancasGenerateはAwakeの一回でしか生成されないため、あらかじめDDOLにしておく
        DontDestroyOnLoad(canvasGenerate);

        // どのcanvasよりも手前に表示する
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 100;

        // panelを生成
        GameObject panelGenerate = new GameObject("FadePanel");
        fadePanel = panelGenerate.AddComponent<Image>();
        fadePanel.color = panelColor;

        // panelをcanvasの子供に設定
        panelGenerate.transform.SetParent(canvasGenerate.transform, false);

        // 生成したimageの大きさを画面全体の大きさになるように変更
        RectTransform rectTransform = fadePanel.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// フェードアウトの演出を挟む
    /// </summary>
    public Coroutine FadeOut()
    {
        if (IsFading)
            return coroutine;

        if (panelColor.a >= 1f)
            return null;

        coroutine = StartCoroutine(FadeOutCoroutine());
        return coroutine;
    }

    /// <summary>
    /// フェードインの演出を挟む
    /// </summary>
    public Coroutine FadeIn()
    {
        if (IsFading)
            return coroutine;

        if (panelColor.a <= 0f)
            return null;

        coroutine = StartCoroutine(FadeInCoroutine());
        return coroutine;
    }

    IEnumerator FadeOutCoroutine()
    {
        fadePanel.raycastTarget = true;

        while (panelColor.a < 1f)
        {
            // ラグいとフェードが一気に変わってしまうため、フェードの変化量に上限を設定
            panelColor.a += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f) / fadeTime;
            fadePanel.color = panelColor;
            yield return null;
        }

        panelColor.a = 1f;

        coroutine = null;
    }

    IEnumerator FadeInCoroutine()
    {
        fadePanel.raycastTarget = true;

        while (panelColor.a > 0f)
        {
            // ラグいとフェードが一気に変わってしまうため、フェードの変化量に上限を設定
            panelColor.a -= Mathf.Min(Time.unscaledDeltaTime, 1f / 30f) / fadeTime;
            fadePanel.color = panelColor;
            yield return null;
        }

        panelColor.a = 0f;

        fadePanel.raycastTarget = false;
        coroutine = null;
    }

    public bool IsFading => coroutine != null;
}
