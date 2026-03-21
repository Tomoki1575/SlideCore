using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicElement : MonoBehaviour
{
    [SerializeField]
    private Button BackButton;
    [SerializeField]
    private Image BackPanel;
    [SerializeField]
    private Image LevelImage;
    [SerializeField]
    private TextMeshProUGUI LevelText;
    [SerializeField]
    private Image JacketImage;
    [SerializeField]
    private TextMeshProUGUI TitleText;
    [SerializeField]
    private TextMeshProUGUI ArtistText;

    private const float DefaultBackPanelAlpha = 16 / 255f;
    private const float SelectBackPanelAlpha = 64 / 255f;

    public event Action<CellView> OnElementPressed;
    private CellView Cell;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (BackButton, nameof(BackButton)),
            (BackPanel, nameof(BackPanel)),
            (LevelImage, nameof(LevelImage)),
            (LevelText, nameof(LevelText)),
            (JacketImage, nameof(JacketImage)),
            (TitleText, nameof(TitleText)),
            (ArtistText, nameof(ArtistText))
        );

        BackButton.onClick.AddListener(OnButtonPressed);
    }

    public void SetElementData(MusicData musicData, MusicDataManager.Difficulty difficulty, Color levelColor)
    {
        // レベルの背景色を設定
        if (LevelImage.color != levelColor) LevelImage.color = levelColor;
        // レベルの数値を設定
        string level = string.Empty;
        switch (difficulty)
        {
            case MusicDataManager.Difficulty.Easy:
                level = musicData.levelEasy.ToString();
                break;
            case MusicDataManager.Difficulty.Normal:
                level = musicData.levelNormal.ToString();
                break;
            case MusicDataManager.Difficulty.Hard:
                level = musicData.levelHard.ToString();
                break;
            default:
                level = "--";
                break;
        }
        if (LevelText.text != level) LevelText.text = level;
        // ジャケット画像を設定
        if (JacketImage.sprite != musicData.jacketImage) JacketImage.sprite = musicData.jacketImage;
        // 曲名を設定
        if (TitleText.text != musicData.title) TitleText.text = musicData.title;
        // アーティスト名を設定
        if (ArtistText.text != musicData.artist) ArtistText.text = musicData.artist;
    }

    public void SetBackPanelAlPha(bool bSelect)
    {
        float alpha = bSelect ? SelectBackPanelAlpha : DefaultBackPanelAlpha;
        if(BackPanel.color.a != alpha)
        {
            Color c = BackPanel.color;
            c.a = alpha;
            BackPanel.color = c;
        }
    }

    private void OnButtonPressed()
    {
        // 自身についているセルを返す
        if (Cell == null) this.transform.GetComponent<CellView>();
        OnElementPressed?.Invoke(Cell);
    }
}
