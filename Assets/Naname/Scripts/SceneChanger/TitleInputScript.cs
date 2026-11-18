using UnityEngine;

public class TitleInputScript : MonoBehaviour
{
    [Header("panelをアタッチ")]
    [SerializeField] private GameObject titlePanel;

    public void OnTitleToSelectTap()
    {
        SceneLoaderScript.Instance.LoadSceneWithFade("SelectScene");
    }

    public void OnOpenPanel()
    {
        if (titlePanel == null) return;
        titlePanel.SetActive(true);

        Debug.Log("開いた");
    }

    public void OnClosePanel()
    {
        if (titlePanel == null) return;
        titlePanel.SetActive(false);
    }
}
