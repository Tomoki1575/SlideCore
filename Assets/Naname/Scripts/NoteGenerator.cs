using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NotesData
{
    public float hitTime;
    public float lane;
}

public class NoteGenerator : MonoBehaviour
{
    [Header("設定")]
    public GameObject notePrefab;
    private float spawnOffsetTime;

    [Header("譜面データ")]
    public List<NotesData> notesToSpawn = new List<NotesData>();

    private float laneSpacing = 250f; // レーン同士の間隔

    private int currentNotesIndex = 0;

    void Start()
    {
        // 判定ラインより4500高い位置で画面から見え始めるため、そこの直前で生成
        // 視野角や画面範囲を変えた場合は、この4500を変える
        spawnOffsetTime = 4500f / MoveScript.scrollSpeed;

        notesToSpawn.Add(new NotesData { hitTime = 8.0f, lane = 2 });
        notesToSpawn.Add(new NotesData { hitTime = 9.0f, lane = 3 });
        notesToSpawn.Add(new NotesData { hitTime = 9.0f, lane = 1 });
        notesToSpawn.Add(new NotesData { hitTime = 11.0f, lane = 4 });

        // hitTimeが早い順にならんでいないといけない為、早い順にリストを並び替える 
        notesToSpawn.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));
    }

    void Update()
    {
        // 全てのノーツを出し終わっていたら何もしない
        if (currentNotesIndex >= notesToSpawn.Count) 
            return;

        // 次に出番が来ているノーツのデータを見る
        NotesData nextNotes = notesToSpawn[currentNotesIndex];

        // 現在の曲の時間が「出現させるべき時間（叩く時間 - 先読み時間）」を過ぎたら
        // 最初ここif文で作ってたんだけど、よく考えたらそれだと同時押しノーツが来た時ずれそうだなと思ってwhileにした
        while (currentNotesIndex < notesToSpawn.Count && MusicManagerScript.songTime >= notesToSpawn[currentNotesIndex].hitTime - spawnOffsetTime)
        {
            SpawnNote(notesToSpawn[currentNotesIndex]);
            currentNotesIndex++; // 次のノーツへ進み、再びwhileの条件をチェック
        }
    }

    /// <summary>
    /// Listとして詰め込まれたノーツ情報を元に、ノーツの生成する位置や時間を確定させる関数。
    /// </summary>
    private void SpawnNote(NotesData data)
    {
        GameObject newNote = Instantiate(notePrefab, transform);

        newNote.transform.localScale = Vector3.one;

        MoveScript moveScript = newNote.GetComponent<MoveScript>();

        if (moveScript != null)
        {
            // hitTime変数を参照し、ノーツが叩かれるべき時間を決める
            moveScript.hitTime = data.hitTime;

            // lane変数を参照し、レーンを決める
            float xPos = (data.lane - 2.5f) * laneSpacing;
            RectTransform rect = newNote.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(xPos, 0);
            moveScript.lane = (int)data.lane;

            // Z座標のデータを消す
            Vector3 fixedPos = rect.localPosition;
            fixedPos.z = 0f;
            rect.localPosition = fixedPos;

            // ノーツの座標をいじったため、一度ノーツの座標を計算しなおす
            moveScript.RefreshPosition();
        }

        //Debug.Log($"ノーツを生成 = hitTime: {data.hitTime}, レーン: {data.lane}");
    }
}
