using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// このスクリプトをインスペクターにアタッチした際に、自動でFadeManagerScriptもアタッチされる
[RequireComponent(typeof(FadeManagerScript))]
public class SceneLoaderScript : MonoBehaviour
{
    private bool isLoading = false;

    public static SceneLoaderScript Instance { get; private set; }

    private void Awake()
    {
        // シングルトンの処理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (Instance != this)
            return;

        FadeManagerScript.Instance.FadeIn();
    }

    /// <summary>
    /// フェードアウト -> シーン読み込み -> フェードインを行う(遷移中にもう一度呼んでも無視される)
    /// </summary>
    public void LoadSceneWithFade(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadSceneWithFadeCoroutine(sceneName));
    }

    /// <summary>
    /// フェードアウト・フェードイン付きでシーンを切り替える
    /// </summary>
    private IEnumerator LoadSceneWithFadeCoroutine(string sceneName)
    {
        isLoading = true;

        // フェードアウト -> シーン読み込み -> 間を開けて -> フェードイン
        yield return FadeManagerScript.Instance.FadeOut();
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return FadeManagerScript.Instance.FadeIn();

        isLoading = false;
    }
}
