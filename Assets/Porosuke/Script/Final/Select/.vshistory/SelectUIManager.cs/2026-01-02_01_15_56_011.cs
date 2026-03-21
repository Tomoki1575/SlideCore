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
    [SerializeField]
    private ThemeColorDefinition ThemeColorDefinition;

    // UIƒZƒbƒg
    [SerializeField]
    private TextMeshProUGUI NoMatchText;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (MusicDataManagerClass, nameof(MusicDataManagerClass)),
            (ScrollRectPoolClass, nameof(ScrollRectPoolClass)),
            (ThemeColorDefinition, nameof(ThemeColorDefinition)),
            (NoMatchText, nameof(NoMatchText))
        );
    }


    void Start()
    {
        MusicDataManagerClass.OnDisplayMusicDataListChanged += OnDataListChanged;
        SetNoMatchTextVisible(false);
    }

    private void OnDataListChanged()
    {
        ScrollRectPoolClass.SetupElements(MusicDataManagerClass.CurrentDisplayMusicDataList,
                                            MusicDataManagerClass.CurrentDifficulty,
                                            GetThemeColorByDifficulty());

        SetNoMatchTextVisible(MusicDataManagerClass.CurrentDisplayMusicDataList.Count == 0);
    }

    private Color GetThemeColorByDifficulty()
    {
        switch (MusicDataManagerClass.CurrentDifficulty)
        {
            case MusicDataManager.Difficulty.Easy:
                return ThemeColorDefinition.EasyColor;
            case MusicDataManager.Difficulty.Normal:
                return ThemeColorDefinition.NormalColor;
            case MusicDataManager.Difficulty.Hard:
                return ThemeColorDefinition.HardColor;
            default:
                return ThemeColorDefinition.EasyColor;
        }
    }

    #region UI Setter
    private void SetNoMatchTextVisible(bool bVisible)
    {
        NoMatchText.enabled = bVisible;
    }
    #endregion

    #region UI Events
    public void OnEasyButtonPressed()
    {
        MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Easy);
    }

    public void OnNormalButtonPressed()
    {
        MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Normal);
    }

    public void OnHardButtonPressed()
    {
        MusicDataManagerClass.SetDifficulty(MusicDataManager.Difficulty.Hard);
    }

    public void OnPlayButtonPressed()
    {

    }
    #endregion
}
