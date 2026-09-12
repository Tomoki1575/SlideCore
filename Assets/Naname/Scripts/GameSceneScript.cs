using System.Runtime.CompilerServices;
using UnityEngine;

enum State
{
    Start,
    Play,
    Pause,
    End
};

public class GameSceneScript : MonoBehaviour
{
    private State state;

    public static GameSceneScript Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        state = State.Start;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Start:
                {
                    // 曲のパッケージイラストなどを表示
                    state = State.Play;
                    break;
                }
                

            case State.Play:
                {
                    // 
                    break;
                }

            case State.Pause:
                {

                    break;
                }
        }
    }

    public bool OnPauseToggle()
    {
        state = State.Pause;

        // ポーズ画面の処理

        return false;
    }
}
