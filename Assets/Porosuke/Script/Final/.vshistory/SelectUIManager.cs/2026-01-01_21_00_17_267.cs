using UnityEngine;

public class SelectUIManager : MonoBehaviour
{
    [SerializeField]
    private MusicDataManager MusicDataManagerClass;

    void Start()
    {
        
    }

    private void OnEasyButtonPressed()
    {
        if (MusicDataManagerClass != null) MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Easy);
    }

    private void OnNormalButtonPressed()
    {
        if (MusicDataManagerClass != null) MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Normal);
    }

    private void OnHardButtonPressed()
    {
        if (MusicDataManagerClass != null) MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Hard);
    }
}
