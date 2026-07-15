using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

[System.Serializable]
public class NotesData
{
    public float HitTime;
    public int Lane;
    public NoteType noteType;
    public bool bIsRight;
}

public class NoteGenerator : MonoBehaviour
{
    public static NoteGenerator Instance { get; private set; }

    [Header("設定")]
    [SerializeField] private GameObject tapNotePrefab;       // タップ用
    [SerializeField] private GameObject slideLeftPrefab;     // 左スライド用
    [SerializeField] private GameObject slideRightPrefab;    // 右スライド用
    [SerializeField] private GameObject noiseNotePrefab;     // ノイズ用
    [SerializeField] private MusicSelection musicSelection;

    private float spawnOffsetTime;

    [Header("譜面データ")]
    public List<NotesData> NotesToSpawn = new List<NotesData>();

    private float laneSpacing = 250f; // レーン同士の間隔

    private int currentNotesIndex = 0;

    // 各レーンに「今画面に存在するノーツのMoveScript」を並び順通りに格納する
    public List<MoveScript>[] laneNotesLists = new List<MoveScript>[6];

    private void Awake()
    {
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

        // JSONからノーツデータを読み込む
        TextAsset chart = musicSelection.GetChart();
        JsonDataConverter.JsonData jsonData = JsonDataConverter.LoadJson(chart);

        TimeConverter.SetSignatureDataList(jsonData.signatures);

        foreach (JsonDataConverter.NoteData note in jsonData.notes)
        {
            float hitTime = (float)TimeConverter.ConvertBeatToReal(note.beat, jsonData.bpms) - (jsonData.meta.offset / 1000f - 0.4f);

            NotesToSpawn.Add(new NotesData
            {
                HitTime   = hitTime,
                Lane      = note.lane,
                noteType = note.type,
                bIsRight  = note.bIsRight
            });
        }

        // HitTimeが早い順にならんでいないといけない為、早い順にリストを並び替える
        NotesToSpawn.Sort((a, b) => a.HitTime.CompareTo(b.HitTime));
    }

    void Update()
    {
        // 全てのノーツを出し終わっていたら何もしない
        if (currentNotesIndex >= NotesToSpawn.Count)
            return;

        // 現在の曲の時間が「出現させるべき時間（叩く時間 - 先読み時間）」を過ぎたら
        // 同時押しノーツwhileにした
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
        GameObject prefabToSpawn = tapNotePrefab;

        switch (data.noteType)
        {
            case NoteType.Tap:
                prefabToSpawn = tapNotePrefab;
                break;
            case NoteType.Slide:
                prefabToSpawn = data.bIsRight ? slideRightPrefab : slideLeftPrefab;
                break;
            case NoteType.Noise:
                prefabToSpawn = noiseNotePrefab;
                break;
            case NoteType.Hold:
                prefabToSpawn = tapNotePrefab;
                break;
        }

        GameObject newNote = Instantiate(prefabToSpawn, transform);

        newNote.transform.localScale = Vector3.one;

        MoveScript moveScript = newNote.GetComponent<MoveScript>();

        if (moveScript != null)
        {
            // HitTime変数を参照し、ノーツが叩かれるべき時間を決める
            moveScript.HitTime = data.HitTime;
            moveScript.MyNotesType = data.noteType;
            moveScript.IsRight = data.bIsRight;

            // lane変数を参照し、レーンを決める
            float xPos = (data.Lane - 2.5f) * laneSpacing;
            RectTransform rect = newNote.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(xPos, 0);
            moveScript.Lane = data.Lane;

            // Z座標のデータを消す
            Vector3 fixedPos = rect.localPosition;
            fixedPos.z = 0f;
            rect.localPosition = fixedPos;

            // ノーツの座標をいじったため、一度ノーツの座標を計算しなおす
            moveScript.RefreshPosition();

            if (moveScript.Lane >= 0 && moveScript.Lane < 6)
            {
                laneNotesLists[moveScript.Lane].Add(moveScript);
            }
        }
    }
}