using UnityEngine;

public class VSyncScript : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 1;   // –ˆƒtƒŒ[ƒ€VSync
        Application.targetFrameRate = -1; // VSync‚É”C‚¹‚é
    }
}
