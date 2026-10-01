using UnityEngine;
using TMPro;

public class ResultDisplayScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI badgeText;
    [SerializeField] private TextMeshProUGUI perfectText;
    [SerializeField] private TextMeshProUGUI greatText;
    [SerializeField] private TextMeshProUGUI goodText;
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private TextMeshProUGUI maxComboText;

    private void Start()
    {
        scoreText.text = ResultCounterScript.GetScore().ToString();
        rankText.text = ResultCounterScript.RankToText(ResultCounterScript.GetRank());
        badgeText.text = ResultCounterScript.BadgeToText(ResultCounterScript.GetBadge());

        perfectText.text = ResultCounterScript.CountPerfect.ToString();
        greatText.text = ResultCounterScript.CountGreat.ToString();
        goodText.text = ResultCounterScript.CountGood.ToString();
        missText.text = ResultCounterScript.CountMiss.ToString();

        maxComboText.text = ResultCounterScript.MaxCombo.ToString();


    }
}
