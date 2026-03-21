using System;
using System.Collections.Generic;

public static class JsonDataConverter
{
    [Serializable]
    public struct BeatData
    {
        public int measure;
        public int beat;
        public int tick;
    }
    [Serializable]
    public class BpmData
    {
        public BeatData beat;
        public int bpm;
        public float speedScale;
    }
    [Serializable]
    public class SignatureData
    {
        public BeatData beat;
        public int numerator;
        public int denominator;
        public int measure => beat.measure;
    }
    [Serializable]
    public class NoteData
    {
        public BeatData beat;
        public int lane;
        public NoteType type;
        public bool bSpecial;
        public NoteData pairNoteData;
        public int pairIndex;   // JSON‚É‘‚«o‚·Û‚ÉQÆ‚ğ•Û‚·‚é‚½‚ß‚Ì‚à‚Ì
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
    [Serializable]
    public class MetaData
    {
        public string title;
        public string artist;
        public string chartAuthor;
        public string Description;
        public string difficulty;
        public string musicName;
        public string musicPath;
        public int offset;
    }

    [Serializable]
    public class JsonData
    {
        public MetaData meta;
        public List<BpmData> bpms;
        public List<SignatureData> signatures;
        public List<NoteData> notes;
    }
}
