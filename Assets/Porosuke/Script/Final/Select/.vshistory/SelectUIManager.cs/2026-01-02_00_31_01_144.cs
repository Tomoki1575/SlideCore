using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectUIManager : MonoBehaviour
{
    [SerializeField]
    private MusicDataManager MusicDataManagerClass;
    [SerializeField]
    private ScrollRectPool ScrollRectPoolClass;

    // UIƒZƒbƒg
    [SerializeField]
    private TextMeshProUGUI NoMatchText;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicDataManagerClass, nameof(MusicDataManagerClass)),
            (ScrollRectPoolClass, nameof(ScrollRectPoolClass)),
            (NoMatchText, nameof(NoMatchText))
        );
    }


    void Start()
    {
        MusicDataManagerClass.OnDisplayMusicDataListChanged += OnDataListChanged;
        SetNoMatchTextVisible(false);
    }

    private void OnDataListChanged(List<MusicData> musicDataList)
    {
        ScrollRectPoolClass.SetupElements(musicDataList);
        bool bFailed = musicDataList.Count == 0 || ScrollRectPoolClass == null;
        SetNoMatchTextVisible(bFailed);
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
