using UnityEngine;

public class NoteComponent : MonoBehaviour
{
    [SerializeField]
    private CanvasRenderer InLineRenderer;
    [SerializeField]
    private RectTransform ParentRect;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (InLineRenderer, nameof(InLineRenderer)),
            (ParentRect, nameof(ParentRect))
            );
    }

    public void SetNoteView(bool bSpecial, bool bIsRight)
    {
        InLineRenderer.SetAlpha(bSpecial ? 1 : 0);
        ParentRect.rotation = Quaternion.Euler(0, 0, bIsRight ? 180 : 0);
    }

    public void SetPositionX(double x)
    {
        ParentRect.anchoredPosition = new Vector2((float)x, ParentRect.anchoredPosition.y);
    }

    public void SetPositionY(double y)
    {
        ParentRect.anchoredPosition = new Vector2(ParentRect.anchoredPosition.x, (float)y);
    }

    public void SetAnchor(Vector2 anchorMin, Vector2 anchorMax)
    {
        ParentRect.anchorMin = anchorMin;
        ParentRect.anchorMax = anchorMax;
    }
}
