using UnityEngine;
using UnityEngine.InputSystem;

public class DebugPanelOutputScript : MonoBehaviour
{
    public GameObject debugPanel;

    private void Update()
    {
#if UNITY_EDITOR
        // [DebugCommand] < / + d > : デバッグパネルの表示・非表示を切り替え
        if (Keyboard.current.slashKey.wasPressedThisFrame && Keyboard.current.dKey.isPressed)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
#endif
    }
}
