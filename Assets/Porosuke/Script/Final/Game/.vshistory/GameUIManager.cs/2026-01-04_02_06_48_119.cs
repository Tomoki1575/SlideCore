using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI DefaultMusicWaitText;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (DefaultMusicWaitText, nameof(DefaultMusicWaitText))
            );
    }
}
