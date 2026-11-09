using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class TouchTestScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private TextMeshProUGUI text;

    [Header("画面に残す行数")]
    [SerializeField] private int lineCount = 8;

    private InputAction touchPressAction;

    private readonly List<string> lines = new List<string>();

    private void Awake()
    {
        touchPressAction = actions.FindAction("TouchPress");

        if (touchPressAction == null)
            Debug.LogError($"[TouchTest] TouchPressが見つかりません。");
    }

    private void OnEnable()
    {
        if (touchPressAction == null)
            return;

        touchPressAction.Enable();
        touchPressAction.performed += OnTouchPressed;
    }

    private void OnDisable()
    {
        if (touchPressAction == null)
            return;

        touchPressAction.performed -= OnTouchPressed;
        touchPressAction.Disable();
    }

    private void OnTouchPressed(CallbackContext context)
    {
        // PassThroghは「指を離した瞬間」にも呼ばれるので、押した瞬間だけ呼ぶ
        if (!context.ReadValueAsButton()) return;

        // イベントが記録された時間と、今この関数が動いている時刻を調べる
        double eventTime = context.time;
        double now = Time.realtimeSinceStartupAsDouble;
        double delay = (now - eventTime) * 1000.0;

        // context.control は「押された」というボタン。その親がタッチ1本ぶんの情報を持っている
        TouchControl touch = context.control.parent as TouchControl;

        string info;

        if (touch != null)
        {
            Vector2 pos = touch.position.ReadValue();
            info = $"{touch.name} id:{touch.touchId.ReadValue()} pos({pos.x:F0},{pos.y:F0})";
        }

        else
            info = $"{context.control.name} 位置の取得に失敗";

        // 新しい行を先頭に入れて、古い行を捨てる
        lines.Insert(0, $"{info}  delay {delay:F2}ms");

        if (lines.Count > lineCount)
            lines.RemoveAt(lines.Count - 1);

        text.text = string.Join("\n", lines);
    }

    // Inspector の OnClick から呼ばれる
    public void OnPushGotoTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
