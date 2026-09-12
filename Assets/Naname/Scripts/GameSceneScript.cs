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
}
