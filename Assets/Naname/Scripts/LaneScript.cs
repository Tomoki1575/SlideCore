using UnityEngine;
using UnityEngine.InputSystem;

public class LaneScript_Minimal : MonoBehaviour
{
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inActiveMaterial;

    public GameObject LaneNumber0, LaneNumber1, LaneNumber2, LaneNumber3, LaneNumber4, LaneNumber5;

    int[] a = new int[4] { 1, 2, 3, 4 };

    int[] canInputLane = new int[4];

    GameObject[] lanes;

    bool[] isActiveLane = new bool[6];

    void Awake()
    {
        lanes = new[] { LaneNumber0, LaneNumber1, LaneNumber2, LaneNumber3, LaneNumber4, LaneNumber5 };
        Recalc();
    }

    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = Wrap6(a[i] + 1);
            }

            Recalc();
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = Wrap6(a[i] - 1);
            }

            Recalc();
        }
    }

    void Recalc()
    {
        for (int i = 0; i < 4; i++)
        {
            canInputLane[i] = Wrap6(a[i]);
        }

        for (int lane = 0; lane < lanes.Length; lane++)
        {
            bool can = false;

            for (int i = 0; i < canInputLane.Length; i++)
            {
                if (canInputLane[i] == lane)
                {
                    can = true;
                    break;
                }
            }

            var r = lanes[lane].GetComponent<Renderer>();

            if (can)
            {
                r.material = activeMaterial;

                isActiveLane[lane] = true;
            }        

            else
            {
                r.material = inActiveMaterial;

                isActiveLane[lane] = false;
            }          
        }
    }

    int Wrap6(int x) => ((x % 6) + 6) % 6;
}
