using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct MusicData
    {
        public AudioClip music;
        public TextAsset chart;
    }

    [SerializeField] private MusicData[] MusicDataList;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
