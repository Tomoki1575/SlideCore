using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    [SerializeField]
    private MusicDataManager Manager;
    [SerializeField]
    private MusicDataManager.Difficulty Difficulty;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (Manager != null)
        {
            Manager.SetDifficulty(Difficulty);
        }
    }
}
