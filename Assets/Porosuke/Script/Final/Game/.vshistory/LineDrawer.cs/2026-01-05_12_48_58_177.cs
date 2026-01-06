using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineDrawer : Graphic
{
    public enum LineOrientation { Horizontal, Vertical }

    [System.Serializable]
    private struct LineInfo
    {
        public Vector2 start;
        public float length;
        public LineOrientation orientation;
        public float thickness;
        public Color color;
    }

    private List<LineInfo> Lines = new List<LineInfo>();

    private Rect DrawRect;

    protected override void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();
        DrawRect = new Rect(rect.anchoredPosition, rect.sizeDelta);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 rectSize = DrawRect.size;
        Vector2 rectHalf = rectSize / 2f; // 中心原点変換用

        foreach (var line in Lines)
        {
            // 左上原点（下が +Y）→ 内部用の左下原点（上が +Y）に変換
            Vector2 start = line.start;
            start.y = rectSize.y - start.y;

            Vector2 end = start;
            if (line.orientation == LineOrientation.Horizontal) end.x += line.length;
            else end.y += line.length;

            Vector2 normal = (line.orientation == LineOrientation.Horizontal)
                             ? new Vector2(0, line.thickness * 0.5f)
                             : new Vector2(line.thickness * 0.5f, 0);

            // 1. 左下基準で頂点作成
            Vector2 v1 = start + normal;
            Vector2 v2 = start - normal;
            Vector2 v3 = end - normal;
            Vector2 v4 = end + normal;

            // 2. Rect 内に Clamp
            v1.x = Mathf.Clamp(v1.x, 0, rectSize.x);
            v1.y = Mathf.Clamp(v1.y, 0, rectSize.y);
            v2.x = Mathf.Clamp(v2.x, 0, rectSize.x);
            v2.y = Mathf.Clamp(v2.y, 0, rectSize.y);
            v3.x = Mathf.Clamp(v3.x, 0, rectSize.x);
            v3.y = Mathf.Clamp(v3.y, 0, rectSize.y);
            v4.x = Mathf.Clamp(v4.x, 0, rectSize.x);
            v4.y = Mathf.Clamp(v4.y, 0, rectSize.y);

            // 3. 中央原点変換（左下基準 → 中心基準）
            v1 -= rectHalf;
            v2 -= rectHalf;
            v3 -= rectHalf;
            v4 -= rectHalf;

            // 4. Graphic の中心補正は不要（上記で中央原点になっている場合）

            int startIndex = vh.currentVertCount;
            vh.AddVert(v1, line.color, Vector2.zero);
            vh.AddVert(v2, line.color, Vector2.zero);
            vh.AddVert(v3, line.color, Vector2.zero);
            vh.AddVert(v4, line.color, Vector2.zero);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }
    }

    public void AddLine(Vector2 start, float length, LineOrientation orientation, float thickness, Color color)
    {
        Lines.Add(new LineInfo
        {
            start = start,
            length = length,
            orientation = orientation,
            thickness = thickness,
            color = color
        });
    }

    public void DrawLines()
    {
        SetVerticesDirty();
    }

    public void ClearLines()
    {
        Lines.Clear();
    }
}
