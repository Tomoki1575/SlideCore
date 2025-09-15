using TMPro;
using UnityEngine;

public class CountUpScript : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    public static float gameTime;
    public static float msGameTime;

    private void Update()
    {
        gameTime = Time.time;

        msGameTime = Mathf.FloorToInt(gameTime * 1000);

        timerText.text = $"Elapsed time : {msGameTime} ms";
    }
}