using UnityEngine;

[CreateAssetMenu(menuName = "RhythmGame/MusicData")]
public class MusicData : ScriptableObject
{
    public AudioClip audioClip;

    public TextAsset chartEasy;
    public TextAsset chartNormal;
    public TextAsset chartHard;
}
