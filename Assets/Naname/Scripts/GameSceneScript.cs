using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    Preparing,
    Playing,
    Paused,
    Finishing
};

public class GameSceneScript : MonoBehaviour
{
    public GameState State { get; private set; } = GameState.Preparing;

    public static GameSceneScript Instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button retryButton;


    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        retryButton.onClick.AddListener(PushGotoSelectSceneButton);
    }

    private void OnDisable()
    {
        retryButton.onClick.RemoveListener(PushGotoSelectSceneButton);
    }

    private void Start()
    {
        FadeManagerScript.Instance.FadeIn();


        // 曲のパッケージイラストなどを表示
    }


    private void Update()
    {
        switch (State)
        {
            case GameState.Preparing:
                {
                    if (!FadeManagerScript.Instance.IsFading)
                        ChangeState(GameState.Playing);

                    break;
                }

            case GameState.Playing:
                {
                    // 
                    break;
                }

            case GameState.Paused:
                {

                    break;
                }
        }

    }

    public void OnPauseToggle()
    {
        // ポーズ画面の処理
        if (pauseMenu == null)
        {
            Debug.LogError("[Unassigned] ポーズメニューのUIが割り当てられていません。", this);
            return;
        }

        pauseMenu.gameObject.SetActive(true);

        ChangeState(GameState.Paused);
    }

    // ステートを変更する
    private void ChangeState(GameState next)
    {
        if (next == State)
            return;

        State = next;
        Debug.Log("[State] 現在のステート : " + State);
    }

    private void PushGotoSelectSceneButton()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
