using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanelManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup LoadCanvasGroup;
    [SerializeField]
    private Image JacketImage;
    [SerializeField]
    private TextMeshProUGUI DifficultyText;
    [SerializeField]
    private TextMeshProUGUI TitleText;
    [SerializeField]
    private TextMeshProUGUI ArtistText;
    [SerializeField]
    private bool bShort = false;

    public event Action OnShowLoadPanelFinished;

    private const float DisplayWaitSec = 3f;
    private const float FadeoutSec = 1f;

    private bool IsLoadDataFinished = false;
    private Coroutine ShowCoroutine;

    private void Awake()
    {
        RequireCheck.ThrowIfAnyNull(this,
            (LoadCanvasGroup, nameof(LoadCanvasGroup)),
            (JacketImage, nameof(JacketImage)),
            (DifficultyText, nameof(DifficultyText)),
            (TitleText, nameof(TitleText)),
            (ArtistText, nameof(ArtistText))
            );
    }

    public void ShowLoadPanel(MusicSelection musicSelection)
    {
        // UIセット
        JacketImage.sprite = musicSelection.musicData.jacketImage;
        DifficultyText.text = musicSelection.difficulty.ToString();
        TitleText.text = musicSelection.musicData.title;
        ArtistText.text = musicSelection.musicData.artist;

        IsLoadDataFinished = false;

        if(ShowCoroutine != null) StopCoroutine(ShowCoroutine);
        ShowCoroutine = StartCoroutine(ShowRoutine());
    }

    public void NotifyLoadFinished()
    {
        IsLoadDataFinished = true;
    }

    private IEnumerator ShowRoutine()
    {
        // 即表示
        LoadCanvasGroup.alpha = 1;
        LoadCanvasGroup.interactable = false;
        LoadCanvasGroup.blocksRaycasts = true;

        // 最低表示時間待ち
        yield return new WaitForSeconds(bShort ? 1f : DisplayWaitSec);

        // ロード完了待ち
        while (!IsLoadDataFinished)
        {
            yield return null;
        }

        // 1秒フェードアウト
        float time = 0f;

        while (time < FadeoutSec)
        {
            time += Time.deltaTime;
            LoadCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / FadeoutSec);
            yield return null;
        }

        LoadCanvasGroup.alpha = 0f;
        LoadCanvasGroup.blocksRaycasts = false;

        OnShowLoadPanelFinished?.Invoke();
    }
}
