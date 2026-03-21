using UnityEngine;

[CreateAssetMenu(menuName = "Game/MusicSelection")]
public class MusicSelection : ScriptableObject
{
    public MusicData musicData;
    public MusicDataManager.Difficulty difficulty;

    public TextAsset GetChart()
    {
        return difficulty switch
        {
            MusicDataManager.Difficulty.Easy => musicData.chartEasy,
            MusicDataManager.Difficulty.Normal => musicData.chartNormal,
            MusicDataManager.Difficulty.Hard => musicData.chartHard,
            _ => null
        };
    }
}
