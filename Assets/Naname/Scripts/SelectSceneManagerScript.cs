using UnityEngine;

public class SelectSceneManagerScript : MonoBehaviour
{
    public static SelectSceneManagerScript Instance {  get; private set; }

    [SerializeField] private GameObject optionPanel;

    public bool IsOptionOpen;

    private void Awake()
    {
        Instance = this;
    }

    public void OnOpenOption()
    {
        optionPanel.SetActive(true);
        IsOptionOpen = true;
    }

    public void OnCloseOption()
    {
        optionPanel.SetActive(false);
        IsOptionOpen = false;
    }
}
