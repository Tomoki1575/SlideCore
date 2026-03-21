using UnityEngine;

public static class RequireCheck
{
    /// <summary>
    /// Inspector 設定必須の参照が null の場合に例外を投げる
    /// </summary>
    /// <param name="obj">チェック対象のオブジェクト</param>
    /// <param name="varName">変数名（nameof() 推奨）</param>
    /// <param name="owner">この変数を持つ MonoBehaviour</param>
    public static void ThrowIfNull(Object obj, string varName, MonoBehaviour owner)
    {
        if (obj == null)
        {
            string msg = $"[{owner.GetType().Name}] {varName} is missing! " +
                         $"Assign it in the Inspector before running.";
            Debug.LogError(msg, owner);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
        }
    }
    // 複数を一気にチェック可能
    public static void ThrowIfAnyNull(MonoBehaviour owner, params (Object obj, string name)[] checks)
    {
        foreach (var (obj, name) in checks)
            ThrowIfNull(obj, name, owner);
    }
}
