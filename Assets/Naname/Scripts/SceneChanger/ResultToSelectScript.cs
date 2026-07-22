using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultToSelectScript : MonoBehaviour
{
    [SerializeField] private Button resultToSelectButton;

    private void OnEnable()
    {
        resultToSelectButton.onClick.AddListener(OnResultToSelectButton);
    }

    private void OnDisable()
    {
        resultToSelectButton.onClick.RemoveListener(OnResultToSelectButton);
    }

    private void OnResultToSelectButton()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
