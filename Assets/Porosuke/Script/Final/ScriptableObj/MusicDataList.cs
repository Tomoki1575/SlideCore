using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RhythmGame/MusicDataList")]
public class MusicDataList : ScriptableObject
{
    public List<MusicData> allTracks;
}
