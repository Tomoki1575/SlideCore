using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class LineManager : MonoBehaviour
{
    [SerializeField]
    private LineDrawer LineDrawerClass;
    [SerializeField]
    private RectTransform LineDrawerRect;

    private HashSet<NoteData> DrewHoldStartNoteHash;

    private const int HoldLineThickness = 10;

    private Color HoldLineColor = new Color(1.000f, 0.753f, 0.251f);

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (LineDrawerClass, nameof(LineDrawerClass)),
            (LineDrawerRect, nameof(LineDrawerRect))
            );

        DrewHoldStartNoteHash = new HashSet<NoteData>();
    }

    public void DrawLines()
    {
        // 1フレーム終わったら呼ぶ
        LineDrawerClass.DrawLines();
    }

    public void ClearLines()
    {
        // 処理の最初に呼ぶ
        LineDrawerClass.ClearLines();
        DrewHoldStartNoteHash.Clear();
    }

    public void AddHoldLine(NoteData noteData, double position, double currentTime)
    {
        if (noteData.type != NoteType.Hold && noteData.type != NoteType.HoldEnd) return;

        double startPosition = 0;
        double endPosition = 0;

        if(noteData.type == NoteType.Hold)
        {
            if (DrewHoldStartNoteHash.Contains(noteData)) return;

            // 既に計算済みの値を使うことで高速化
            startPosition = position;
            // ペア（End）の今の位置を求める
            endPosition = GetPositionAtTime(noteData.pairNoteData.generateTime, currentTime);
        }
        else
        {
            if (DrewHoldStartNoteHash.Contains(noteData.pairNoteData)) return;

            startPosition = GetPositionAtTime(noteData.pairNoteData.generateTime, currentTime);
            endPosition = position;
        }
        // End -> Startの線を引く
        LineDrawerClass.AddLine(GetStartPosition(noteData.lane, endPosition), 
                                (float)(endPosition - startPosition), 
                                LineDrawer.LineOrientation.Vertical, 
                                HoldLineThickness, 
                                HoldLineColor);
        // 描画済みリストに追加
        DrewHoldStartNoteHash.Add(noteData.type == NoteType.Hold ? noteData : noteData.pairNoteData);
    }

    private Vector2 GetStartPosition(int lane, double y)
    {
        return new Vector2(NotePoolManager.GetPositionXByLane((int)LineDrawerRect.rect.width, lane),
                            (float)y);
    }

    public void AddSameLine()
    {

    }
}
