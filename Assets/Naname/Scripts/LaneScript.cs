using UnityEngine;
using UnityEngine.UI;

public class LaneScript : MonoBehaviour
{
    public static LaneScript Instance { get; private set; }

    [Header("判定アクティブ時の色")]
    [SerializeField] private Color activeColor = Color.white;

    [Header("判定非アクティブ時の色")]
    [SerializeField] private Color inActiveColor = Color.gray;

    private Image[] allLaneImages;

    [Header("各レーンのImageをアタッチ")]
    [SerializeField] private Image Lane0, Lane1, Lane2, Lane3, Lane4, Lane5;

    public static bool[] isActiveLane = new bool[6];

    private int[] canInputLane = new int[4] { 1, 2, 3, 4 };

    private void Awake()
    {
        Instance = this;

        allLaneImages = new[] { Lane0, Lane1, Lane2, Lane3, Lane4, Lane5 };

        ApplyActiveLanes();
    }

    /// <summary>
    /// レーンをスライドさせる
    /// </summary>
    public void SlideLane(bool isRight)
    {
        if (isRight)
        {
            if (isActiveLane[5]) return;   // 右端が既にアクティブ＝これ以上右へは壁

            for (int i = 0; i < canInputLane.Length; i++)
                canInputLane[i] += 1;
        }
        else
        {
            if (isActiveLane[0]) return;   // 左端が既にアクティブ＝これ以上左へは壁

            for (int i = 0; i < canInputLane.Length; i++)
                canInputLane[i] -= 1;
        }

        ApplyActiveLanes();
    }

    /// <summary>
    /// 全レーンの状態と見た目を、現在の入力可能レーンに合わせて更新する
    /// </summary>
    private void ApplyActiveLanes()
    {
        for (int lane = 0; lane < allLaneImages.Length; lane++)
        {
            bool isActive = IsLaneActive(lane);

            isActiveLane[lane] = isActive;
            ChangeLaneColor(lane, isActive);
        }
    }

    /// <summary>
    /// レーンがアクティブかどうか判断する
    /// </summary>
    private bool IsLaneActive(int lane)
    {
        for (int i = 0; i < canInputLane.Length; i++)
        {
            if (canInputLane[i] == lane)
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// レーンがアクティブかどうかを見て、色を変える
    /// </summary>
    private void ChangeLaneColor(int lane, bool isActive)
    {
        if (isActive)
            allLaneImages[lane].color = activeColor;

        else
            allLaneImages[lane].color = inActiveColor;
    }
}
