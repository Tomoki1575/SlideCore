using UnityEngine;
using TMPro;

public class FPSDisplayScript : MonoBehaviour
{
    [Header("何秒ごとに表示を更新するか")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("表示先のTextをアタッチ")]
    [SerializeField] private TextMeshProUGUI text;

    private int frames;
    private float elapsedTime;      // 経過時間
    private float maxDelta;         // その経過時間の中のフレームで、最も飛んだ時間

    private double refreshRate;

    private void Start()
    {
        refreshRate = Screen.currentResolution.refreshRateRatio.value;
    }

    private void Update()
    {
        frames++;
        elapsedTime += Time.unscaledDeltaTime;
        maxDelta = Mathf.Max(maxDelta, Time.unscaledDeltaTime);

        if (elapsedTime < updateInterval) return;

        float fps = frames / elapsedTime;

        text.text = $"fps : {fps:F1} fps\nmax : {maxDelta * 1000f:F1}ms\nHz : {refreshRate:F0}\ntarget : {Application.targetFrameRate}\nvSync : {QualitySettings.vSyncCount}";

        frames = 0;
        elapsedTime = 0f;
        maxDelta = 0;
    }
}
