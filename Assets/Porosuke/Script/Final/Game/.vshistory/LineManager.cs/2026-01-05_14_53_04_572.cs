using System;
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

    private Color HoldLineColor = new Color(1.000f, 0.753f, 0.251f, 1f);

    private const int SameLineThickness = 10;

    private Color SameLineColor = new Color(1f, 1f, 1f, 1f);

    // Vectorは、x=minLane、y=maxLaneを意味する
    private Dictionary<double, Vector2Int> SamePositionDictinary;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (LineDrawerClass, nameof(LineDrawerClass)),
            (LineDrawerRect, nameof(LineDrawerRect))
            );

        DrewHoldStartNoteHash = new HashSet<NoteData>();
        SamePositionDictinary = new Dictionary<double, Vector2Int>();
    }

    public void DrawLines()
    {
        // 1フレーム終わったら呼ぶ
        AddSameLines();
        LineDrawerClass.DrawLines();
    }

    public void ClearLines()
    {
        // 処理の最初に呼ぶ
        LineDrawerClass.ClearLines();
        DrewHoldStartNoteHash.Clear();
        SamePositionDictinary.Clear();
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
        LineDrawerClass.AddLine(GetStartPosition(noteData.lane, startPosition),
                                (float)(startPosition - endPosition),
                                LineDrawer.LineOrientation.Vertical,
                                HoldLineThickness,
                                HoldLineColor);
        // 描画済みリストに追加
        DrewHoldStartNoteHash.Add(noteData.type == NoteType.Hold ? noteData : noteData.pairNoteData);
    }

    private Vector2 GetStartPosition(int lane, double y)
    {
        return new Vector2(GetPositionXByLane(lane), (float)y);
    }

    private int GetPositionXByLane(int lane)
    {
        return NotePoolManager.GetPositionXByLane((int)LineDrawerRect.rect.width, lane);
    }

    public void RegistSamePosition(NoteData noteData, double position)
    {
        int lane = noteData.lane;

        if(SamePositionDictinary.TryGetValue(position, out Vector2Int laneData))
        {
            // 既にPositionは登録されていた
            // ->min/maxLaneの更新が必要な可能性がある
            // 最小更新
            if (lane < laneData.x) laneData.x = lane;
            // 最大更新
            if (lane > laneData.y) laneData.y = lane;
            SamePositionDictinary[position] = laneData;
        }
        else
        {
            // まだ登録されていないので登録
            // laneはとりあえず同値
            SamePositionDictinary.Add(position, new Vector2Int(lane, lane));
        }
    }

    private void AddSameLines()
    {
        // 全同時押しデータが集まったので、線の追加をする
        foreach(KeyValuePair<double, Vector2Int> sameData in SamePositionDictinary)
        {
            double position = sameData.Key;
            Vector2Int laneData = sameData.Value;
            // 2つ以上の値が登録されていることを意味する
            if(laneData.x != laneData.y)
            {
                // 指定のpositionにおいて、minLane-maxLaneの横線を引く
                LineDrawerClass.AddLine(GetStartPosition(laneData.x, position),
                                        GetPositionXByLane(laneData.y) - GetPositionXByLane(laneData.x),
                                        LineDrawer.LineOrientation.Horizontal,
                                        SameLineThickness,
                                        SameLineColor);
            }
        }
    }
}
