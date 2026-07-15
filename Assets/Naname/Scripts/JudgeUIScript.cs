using TMPro;
using UnityEngine;

public class JudgeUIScript : MonoBehaviour
{
    [SerializeField] private RectTransform perfectUIPrefab;

    [SerializeField] private RectTransform greatUIPrefab;
    [SerializeField] private RectTransform greatFastUIPrefab;
    [SerializeField] private RectTransform greatLateUIPrefab;

    [SerializeField] private RectTransform goodUIPrefab;
    [SerializeField] private RectTransform goodFastUIPrefab;
    [SerializeField] private RectTransform goodLateUIPrefab;

    [SerializeField] private RectTransform missUIPrefab;
    [SerializeField] private RectTransform missFastUIPrefab;
    [SerializeField] private RectTransform missLateUIPrefab;

    [SerializeField] private RectTransform judgeUIParent;

    [SerializeField] private TextMeshProUGUI comboText;
    private int currentCombo = 0;


    public static JudgeUIScript Instance { get; private set; }

    private GameObject[] activeJudgeObjects = new GameObject[6];

    [SerializeField] private bool isTimingFeedbackMode = false;

    private void Awake()
    {
        Instance = this;

        if (comboText != null) comboText.text = "";
    }

    public void JudgeOutput(int lane, JudgeResult result, bool isLate)
    {
        if (activeJudgeObjects[lane] != null)
        {
            Destroy(activeJudgeObjects[lane]);
        }

        GameObject judgeUIObj = null;

        switch (result)
        {
            case JudgeResult.Perfect:
                judgeUIObj = Instantiate(perfectUIPrefab.gameObject, judgeUIParent);
                currentCombo++;
                break;

            case JudgeResult.Great:
                judgeUIObj = Instantiate((isTimingFeedbackMode ? (isLate ? greatLateUIPrefab : greatFastUIPrefab) : greatUIPrefab).gameObject, judgeUIParent);
                currentCombo++;
                break;

            case JudgeResult.Good:
                judgeUIObj = Instantiate((isTimingFeedbackMode ? (isLate ? goodLateUIPrefab : goodFastUIPrefab) : goodUIPrefab).gameObject, judgeUIParent);
                currentCombo++;
                break;

            case JudgeResult.Miss:
                judgeUIObj = Instantiate((isTimingFeedbackMode ? (isLate ? missLateUIPrefab : missFastUIPrefab) : missUIPrefab).gameObject, judgeUIParent);
                currentCombo = 0;
                break;
        }

        UpdateComboDisplay();

        activeJudgeObjects[lane] = judgeUIObj;

        RectTransform judgeUI = judgeUIObj.GetComponent<RectTransform>();

        switch (lane)
        {
            case 0:
                judgeUI.anchoredPosition = new Vector2(-550f, -180f);
                return;

            case 1:
                judgeUI.anchoredPosition = new Vector2(-330f, -180f);
                return;

            case 2:
                judgeUI.anchoredPosition = new Vector2(-110f, -180f);
                return;

            case 3:
                judgeUI.anchoredPosition = new Vector2(110f, -180f);
                return;

            case 4:
                judgeUI.anchoredPosition = new Vector2(330f, -180f);
                return;

            case 5:
                judgeUI.anchoredPosition = new Vector2(550f, -180f);
                return;
        }
    }

    private void UpdateComboDisplay()
    {
        if (comboText == null) return;

        if (currentCombo >= 2)
        {
            comboText.text = $"<size=50%>COMBO</size>\n{currentCombo}";

            comboText.transform.localScale = Vector3.one * 1.6f;
        }
        else
        {
            comboText.text = "";
        }
    }

    private void Update()
    {
        if (comboText != null && comboText.transform.localScale.x > 1.0f)
        {
            comboText.transform.localScale = Vector3.Lerp(comboText.transform.localScale, Vector3.one, Time.deltaTime * 15f);
        }
    }
}
