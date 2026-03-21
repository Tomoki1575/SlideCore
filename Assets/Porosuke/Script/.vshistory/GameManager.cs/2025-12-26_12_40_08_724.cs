using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private struct MusicData
    {
        public AudioClip music;
        public TextAsset chart;
    }

    [SerializeField] private List<MusicData> MusicDataList;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
