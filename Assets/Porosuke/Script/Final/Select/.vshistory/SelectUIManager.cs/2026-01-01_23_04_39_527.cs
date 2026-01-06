using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectUIManager : MonoBehaviour
{
    [SerializeField]
    private MusicDataManager MusicDataManagerClass;
    // UIƒZƒbƒg
    [SerializeField]
    private TextMeshProUGUI NoMatchText;

    void Start()
    {
        if (MusicDataManagerClass) MusicDataManagerClass.OnDisplayMusicDataListChanged += OnDataListChanged;
        SetNoMatchTextVisible(false);
    }

    private void OnDataListChanged(List<MusicData> musicDataList, int musicDataListSize)
    {
        if(musicDataListSize == 0)
        {
            SetNoMatchTextVisible(true);
        }
        else
        {
            SetNoMatchTextVisible(false);
        }
    }

    #region UI Setter
    private void SetNoMatchTextVisible(bool bVisible)
    {
        if (NoMatchText) NoMatchText.enabled = bVisible;
    }
    #endregion

    #region UI Events
    public void OnEasyButtonPressed()
    {
        if (MusicDataManagerClass != null) MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Easy);
    }

    public void OnNormalButtonPressed()
    {
        if (MusicDataManagerClass != null) MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Normal);
    }

    public void OnHardButtonPressed()
    {
        if (MusicDataManagerClass != null) MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Hard);
    }

    public void OnPlayButtonPressed()
    {

    }
    #endregion
}
