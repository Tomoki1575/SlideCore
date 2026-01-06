using System;
using UnityEngine;

public static class GameDataManager
{
    public class SpeedScaleData
    {
        public float time;
        public float speedScale;
    }

    public class NoteData
    {
        public float generateTime;  // ¶¬‚·‚éŠÔ
        public float finishTime;    // ”»’èƒ‰ƒCƒ“‚É—ˆ‚éŠÔ
        public float destroyTime;   // ”jŠü‚·‚éŠÔ
        public int lane;            // Š‘®ƒŒ[ƒ“
        public NoteType type;
        public bool bSpecial;
        public NoteData pairNoteData;
        public bool bIsRight;
    }

    [Serializable]
    public enum NoteType
    {
        None,
        Tap,
        Hold,
        HoldEnd,
        Slide,
        Noise
    }
}
