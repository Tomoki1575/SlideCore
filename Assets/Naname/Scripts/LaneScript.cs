using UnityEngine;
using UnityEngine.InputSystem;

public class LaneScript : MonoBehaviour
{
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inActiveMaterial;

    private GameObject[] allLanes;

    public GameObject LaneNumber0, LaneNumber1, LaneNumber2, LaneNumber3, LaneNumber4, LaneNumber5;

    private int[] a = new int[4] { 1, 2, 3, 4 };

    private int[] canInputLane = new int[4];

    public static bool[] isActiveLane = new bool[6];

    private void Awake()
    {
        allLanes = new[] { LaneNumber0, LaneNumber1, LaneNumber2, LaneNumber3, LaneNumber4, LaneNumber5 };
        Recalc();
    }

    private void Update()
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

    private void Recalc()
    {
        for (int i = 0; i < 4; i++)
        {
            canInputLane[i] = Wrap6(a[i]);
        }

        for (int lane = 0; lane < allLanes.Length; lane++)
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

            var r = allLanes[lane].GetComponent<Renderer>();

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

    private int Wrap6(int x) => ((x % 6) + 6) % 6;
}
