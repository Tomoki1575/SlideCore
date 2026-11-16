using UnityEngine;

public class TouchLayoutScript : MonoBehaviour
{
    [SerializeField] private RectTransform[] laneAreas;
    [SerializeField] private RectTransform slideLeftArea;
    [SerializeField] private RectTransform slideRightArea;

    public RectTransform[] LaneAreas => laneAreas;
    public RectTransform SlideLeftArea => slideLeftArea;
    public RectTransform SlideRightArea => slideRightArea;
}
