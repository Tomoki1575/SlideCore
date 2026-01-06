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

    public event Action OnShowLoadPanelFinished;

    private void Awake()
    {
        
    }

    public void ShowLoadPanel(MusicSelection musicSelection)
    {
        // UIセット
        JacketImage.sprite = musicSelection.musicData.jacketImage;
        DifficultyText.text = musicSelection.difficulty.ToString();
        TitleText.text = musicSelection.musicData.title;
        ArtistText.text = musicSelection.musicData.artist;

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        // 即表示
        LoadCanvasGroup.alpha = 1;
        LoadCanvasGroup.interactable = false;
        LoadCanvasGroup.blocksRaycasts = true;

        // 1秒待機
        yield return new WaitForSeconds(1f);

        // 1秒フェードアウト
        float time = 0f;
        const float fadeDuration = 1f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            LoadCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }

        LoadCanvasGroup.alpha = 0f;
        LoadCanvasGroup.blocksRaycasts = false;

        OnShowLoadPanelFinished?.Invoke();
    }
}
