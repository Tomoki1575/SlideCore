using System;
using System.Collections.Generic;
using UnityEngine;

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
        public int pairIndex;   // JSONに書き出す際に参照を保持するためのもの
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

    public static bool LoadJson(TextAsset textAsset)
    {
        JsonData jsonData = JsonUtility.FromJson<JsonData>(textAsset.text);

        // 変換に失敗
        if (jsonData == null) return false;

        // ペアインデックスからペアを求めてセットする
        for(int i = 0; i < jsonData.notes.Count; ++i)
        {
            NoteData note = jsonData.notes[i];
            if (note.pairIndex != -1) note.pairNoteData = jsonData.notes[note.pairIndex];
        }

        return true;
    }
}
