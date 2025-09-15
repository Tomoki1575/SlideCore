using UnityEngine;
using UnityEngine.InputSystem;

public class DebugPanelOutput : MonoBehaviour
{
    public GameObject debugPanel;

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && Keyboard.current.shiftKey.isPressed)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }
}