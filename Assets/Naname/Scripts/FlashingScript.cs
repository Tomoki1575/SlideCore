using TMPro;
using UnityEngine;

public class FlashingScript : MonoBehaviour
{
    [SerializeField] private float blinkingSpeed = 3f;
    private float speed = 1.0f;
    private float time;
    private TMP_Text text;

    void Start()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    void Update()
    {
        text.color = GetAlphaColor(text.color);
    }

    Color GetAlphaColor(Color color)
    {
        time += Time.deltaTime * blinkingSpeed * speed;
        color.a = Mathf.Abs(Mathf.Sin(time));

        return color;
    }
}
