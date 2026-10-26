using UnityEngine;
using UnityEngine.UI;

public class TouchEffectScript : MonoBehaviour
{
    [Header("波紋が消えるまでの時間")]
    [SerializeField] private float duration = 0.25f;

    [Header("波紋の生成時の大きさ")]
    [SerializeField] private float startScale = 7f;

    [Header("波紋が消える直前の大きさ")]
    [SerializeField] private float endScale = 9f;

    private float elapsed;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / Mathf.Max(duration, 0.0001f);
        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

        Color color = image.color;
        color.a = Mathf.Lerp(1, 0, t);
        image.color = color;

        if(t >= 1f)
            Destroy(gameObject);
    }
}
