using UnityEngine;
using UnityEngine.InputSystem;
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
    public Image Lane0, Lane1, Lane2, Lane3, Lane4, Lane5;

    private int[] canInputLane = new int[4] { 1, 2, 3, 4 };

    public static bool[] isActiveLane = new bool[6];        // 判定時に役に立つ（このスクリプトではあまり使わない）

    private void Awake()
    {
        Instance = this;

        allLaneImages = new[] { Lane0, Lane1, Lane2, Lane3, Lane4, Lane5 };

        for (int i = 0; i < 4; i++)
        {
            canInputLane[i] = NormalizeLane(canInputLane[i]);
        }

        ApplyActiveLanes();
    }

    public void SlideLane(bool isRight)
    {
        if(isRight)
        {
            for (int i = 0; i < canInputLane.Length; i++)
            {
                canInputLane[i] = NormalizeLane(canInputLane[i] + 1);
            }

            ApplyActiveLanes();
        }

        else if(!isRight)
        {
            for (int i = 0; i < canInputLane.Length; i++)
            {
                canInputLane[i] = NormalizeLane(canInputLane[i] - 1);
            }

            ApplyActiveLanes();
        }
    }

    private void ApplyActiveLanes()
    {
        for (int lane = 0; lane < allLaneImages.Length; lane++)
        {
            bool isActive = false;

            for (int i = 0; i < canInputLane.Length; i++)
            {
                if (canInputLane[i] == lane)
                {
                    isActive = true; 
                    break; 
                }
            }

            if (isActive)
            {
                allLaneImages[lane].color = activeColor;
                isActiveLane[lane] = true;
            }

            else
            {
                allLaneImages[lane].color = inActiveColor;
                isActiveLane[lane] = false;
            }
        }
    }

    private int NormalizeLane(int laneNumber) 
        => ((laneNumber % 6) + 6) % 6;
    // アクティブレーンをずらした時にはみ出たものを6で割り、その余りを算出するための関数。
    // 例えば3,4,5,6レーンがアクティブである時、一つずらすと4,5,6,7レーンがアクティブとなる。
    // しかし7レーンなんてものは存在しない為、7 % 6をすることで1レーン目をアクティブにする。
}
