using UnityEngine;

[CreateAssetMenu(menuName = "RhythmGame/MusicData")]
public class MusicData : ScriptableObject
{
    public Sprite jacketImage;
    public string title;
    public string artist;

    public AudioClip audioClip;

    public int levelEasy;
    public TextAsset chartEasy;
    public int levelNormal;
    public TextAsset chartNormal;
    public int levelHard;
    public TextAsset chartHard;
}
