using UnityEngine;

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

	private float resultDelay = 1.5f;

	private void Awake()
	{
		Instance = this;
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
					if (MusicManagerScript.Instance.IsMusicStartReady && !FadeManagerScript.Instance.IsFading)
					{
						MusicManagerScript.Instance.StartSong();
						ChangeState(GameState.Playing);
					}

					break;
				}

			case GameState.Playing:
				{
					// 「曲が流れてからの経過時間」が「曲の長さ ＋ 余韻」を過ぎたらリザルトへ
					if (MusicManagerScript.Instance.CurrentSongTime >= MusicManagerScript.Instance.SongLength + resultDelay)
					{
						SceneLoaderScript.Instance.LoadSceneWithFade("ResultScene");
						ChangeState(GameState.Finishing);
					}

					break;
				}

			case GameState.Paused:
				{
					break;
				}

			case GameState.Finishing:
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

		MusicManagerScript.Instance.PauseSong();

		ChangeState(GameState.Paused);
	}

	/// <summary>
	/// ログを出しながらステートを変更する(多重判定対策済み)
	/// </summary>
	private void ChangeState(GameState next)
	{
		// 多重判定の対策
		if (next == State)
			return;

		State = next;
		Debug.Log("[State] 現在のステート : " + State);
	}

	/// <summary>
	/// 曲をやり直すボタンが押された(ボタンオブジェクトのインスペクターから設定)
	/// </summary>
	public void OnRetryGame()
	{
		SceneLoaderScript.Instance.LoadSceneWithFade("GameScene");
	}

	/// <summary>
	/// 曲選択画面に戻るボタンが押された(ボタンオブジェクトのインスペクターから設定)
	/// </summary>
	public void OnPushGotoSelectSceneButton()
	{
		SceneLoaderScript.Instance.LoadSceneWithFade("SelectScene");
	}

	public void OnContinueGame()
	{
		// ToDo : 修正

		pauseMenu.gameObject.SetActive(false);

		ChangeState(GameState.Playing);
	}
}
