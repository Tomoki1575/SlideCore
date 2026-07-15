using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class TitleInputScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;

    private System.Action<CallbackContext> titleToSelectHandler;

    private void OnEnable()
    {
        actions.Enable();

        titleToSelectHandler = ctx => TitleToSelectTap();
        actions.FindAction("TitleToSelect").performed += titleToSelectHandler;
    }

    private void OnDisable()
    {
        actions.FindAction("TitleToSelect").performed -= titleToSelectHandler;

        actions.Disable();
    }

    private void TitleToSelectTap()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
