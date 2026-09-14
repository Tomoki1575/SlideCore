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
        Instance = this;

        // canvasを生成
        GameObject canvasGenerate = new GameObject("FadeCanvas");
        fadeCanvas = canvasGenerate.AddComponent<Canvas>();
        canvasGenerate.AddComponent<CanvasScaler>();
        canvasGenerate.AddComponent<GraphicRaycaster>();

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

    // フェードアウトの演出を挟む
    public void FadeOut()
    {
        if (panelColor.a == 0)
        {
            coroutine = StartCoroutine(FadeOutCoroutine());
        }
    }

    // フェードインの演出を挟む
    public void FadeIn()
    {
        if (panelColor.a == 1)
        {
            coroutine = StartCoroutine(FadeInCoroutine());
        }
    }

    IEnumerator FadeOutCoroutine()
    {
        fadePanel.raycastTarget = true;


        while (panelColor.a < 1f)
        {
            // ToDo : 0.05秒ごとに更新だとちょっとカクついて見えるかも?
            yield return new WaitForSeconds(0.05f);
            panelColor.a += 1f / (fadeTime * 20);
            fadePanel.color = panelColor;
        }

        panelColor.a = 1f;

        StopCoroutine(coroutine);
        coroutine = null;
    }

    IEnumerator FadeInCoroutine()
    {
        fadePanel.raycastTarget = true;

        while (panelColor.a > 0f)
        {
            yield return new WaitForSeconds(0.05f);
            panelColor.a -= 1f / (fadeTime * 20);
            fadePanel.color = panelColor;
        }

        panelColor.a = 0f;

        fadePanel.raycastTarget = false;
        StopCoroutine(coroutine);
        coroutine = null;
    }

    public bool IsFading => coroutine != null;
}
