using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NotesData
{
    public float HitTime;
    public float Lane;
}

public class NoteGenerator : MonoBehaviour
{
    public static NoteGenerator Instance { get; private set; }

    [Header("設定")]
    [SerializeField] private GameObject notePrefab;
    private float spawnOffsetTime;

    [Header("譜面データ")]
    public List<NotesData> NotesToSpawn = new List<NotesData>();

    private float laneSpacing = 250f; // レーン同士の間隔

    private int currentNotesIndex = 0;

    // 各レーンに「今画面に存在するノーツのMoveScript」を並び順通りに格納します
    public List<MoveScript>[] laneNotesLists = new List<MoveScript>[6];

    private void Awake()
    {
        // シングルトンの初期化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 6レーン分のリストを初期化
        for (int i = 0; i < 6; i++)
        {
            laneNotesLists[i] = new List<MoveScript>();
        }
    }

    void Start()
    {
        // 判定ラインより4500高い位置で画面から見え始めるため、そこの直前で生成
        // 視野角や画面範囲を変えた場合は、この4500を変える
        spawnOffsetTime = 4500f / MoveScript.ScrollSpeed;

        NotesToSpawn.Add(new NotesData { HitTime = 3.9f, Lane = 2 });
        NotesToSpawn.Add(new NotesData { HitTime = 5.6f, Lane = 3 });
        NotesToSpawn.Add(new NotesData { HitTime = 8.0f, Lane = 2 });
        NotesToSpawn.Add(new NotesData { HitTime = 9.0f, Lane = 3 });
        NotesToSpawn.Add(new NotesData { HitTime = 9.0f, Lane = 1 });
        NotesToSpawn.Add(new NotesData { HitTime = 11.0f, Lane = 4 });

        // HitTimeが早い順にならんでいないといけない為、早い順にリストを並び替える 
        NotesToSpawn.Sort((a, b) => a.HitTime.CompareTo(b.HitTime));
    }

    void Update()
    {
        // 全てのノーツを出し終わっていたら何もしない
        if (currentNotesIndex >= NotesToSpawn.Count)
            return;

        // 現在の曲の時間が「出現させるべき時間（叩く時間 - 先読み時間）」を過ぎたら
        // 最初ここif文で作ってたんだけど、よく考えたらそれだと同時押しノーツが来た時ずれそうだなと思ってwhileにした
        while (currentNotesIndex < NotesToSpawn.Count && MusicManagerScript.SongTime >= NotesToSpawn[currentNotesIndex].HitTime - spawnOffsetTime)
        {
            SpawnNote(NotesToSpawn[currentNotesIndex]);
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
            // HitTime変数を参照し、ノーツが叩かれるべき時間を決める
            moveScript.HitTime = data.HitTime;

            // lane変数を参照し、レーンを決める
            float xPos = (data.Lane - 2.5f) * laneSpacing;
            RectTransform rect = newNote.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(xPos, 0);
            moveScript.Lane = (int)data.Lane;

            // Z座標のデータを消す
            Vector3 fixedPos = rect.localPosition;
            fixedPos.z = 0f;
            rect.localPosition = fixedPos;

            // ノーツの座標をいじったため、一度ノーツの座標を計算しなおす
            moveScript.RefreshPosition();

            // --- ★追加：生成したノーツを該当するレーンのリストに登録 ---
            if (moveScript.Lane >= 0 && moveScript.Lane < 6)
            {
                laneNotesLists[moveScript.Lane].Add(moveScript);
            }
        }
    }
}
