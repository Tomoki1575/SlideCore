using UnityEngine;

public static class FrameRateManager
{
    private const bool DefaultUseVSync = true;
    private const int MaxFPS = 120;
    private const int FallbackFPS = 60;

    // 起動時に1回だけ呼ばれる
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyOnStartup()
    {
        ApplyFrameRateSetting(DefaultUseVSync, GetTargetFPSForThisDisplay());
        Debug.Log($"[FrameRate] 起動時に適用しました vSync:{QualitySettings.vSyncCount} / target:{Application.targetFrameRate}");
    }

    /// <summary>
    /// 端末に合わせた目標FPSを返す。<br/>
    /// 安定して出せるのは「リフレッシュレートの約数」だけなので、MaxFPS以下になるような最大の約数を選ぶ。<br/>
    /// 例: 90Hz → 90 ／ 144Hz → 72（144は上限超え）／ 取得失敗 → FallbackFPS
    /// </summary>
    private static int GetTargetFPSForThisDisplay()
    {
        int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

        // 取得に失敗した場合、60FPSとする
        if (refreshRate <= 0)
            return FallbackFPS;

        // 端末のFPSがMaxFPSより大きい場合は、MaxFPS以下にする為に約数を求めてreturn
        // MaxFPS以下の場合はそのままreturn
        int divisor = 1;

        while (refreshRate / divisor > MaxFPS)
            divisor++;

        return refreshRate / divisor;
    }

    /// <summary>
    /// 垂直同期とFPS上限を設定する。<br/>
    /// ちなみに Android / iOS では垂直同期が常に効いているが、<br/>
    /// その場合は第一引数の useVSync は常に無視され、第二引数の targetFPS のみが効く。
    /// </summary>
    public static void ApplyFrameRateSetting(bool useVSync, int targetFPS)
    {
        // PC版においてvSyncCountが0以外の時、targetFrameRateは無視されるため、必ず両方を設定する。
        QualitySettings.vSyncCount = useVSync ? 1 : 0;
        Application.targetFrameRate = targetFPS;
    }
}
