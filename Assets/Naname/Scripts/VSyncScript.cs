using UnityEngine;

public class VSyncScript : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;
    }
}
