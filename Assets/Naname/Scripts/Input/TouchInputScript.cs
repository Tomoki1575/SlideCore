using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static UnityEngine.InputSystem.InputAction;

public class TouchInputScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;

    [Header("WorldSpaceの変換に必要。GameSceneのMainCamera")]
    [SerializeField] private Camera targetCamera;

    [Header("判定を実行する側")]
    [SerializeField] private InputManagerScript inputManager;

    [Header("A案/B案のレイアウト")]
    [SerializeField] private TouchLayoutScript[] layouts;

    private InputAction touchPressAction;
    private TouchLayoutScript currentLayout;

    // どの指がどのレーンを押しているかを記録。
    // 指を離した時に、どのレーンのフラグを下ろすかを引くために持つ。
    // (容量を先に確保しておくと、追加と削除では何も確保されない(途中で足りなくなると内部配列を作り直してGCを踏む))
    private readonly Dictionary<int, int> laneByTouchId = new Dictionary<int, int>(16);

    private void Awake()
    {
        touchPressAction = actions.FindAction("TouchPress");

        if (touchPressAction == null)
            Debug.LogError("[TouchInput] TouchPressが見つかりません。");

        SetLayout(1);
    }

    private void OnEnable()
    {
        if (touchPressAction == null) return;

        touchPressAction.Enable();
        touchPressAction.performed += OnTouchPressChange;
    }

    private void OnDisable()
    {
        if (touchPressAction == null) return;

        touchPressAction.performed -= OnTouchPressChange;
        touchPressAction.Disable();
    }

    /// <summary>
    /// 画面が「押された」「離された」の二種類の状態があるが、その状態を管理する。
    /// </summary>
    private void OnTouchPressChange(CallbackContext context)
    {
        TouchControl touch = context.control.parent as TouchControl;

        if (touch == null) return;

        int touchId = touch.touchId.ReadValue();

        // [Memo] :
        // 現在InputActionsのアクションの種類をPassThroughにしているが、
        // PassThroughには「押された」「離された」の区別がない。
        // その為、ReadValueAsButton()で実際の値を見て、「押された」「離された」を判定している
        if (context.ReadValueAsButton())
            OnTouchBegan(touchId, touch.position.ReadValue(), context.time);

        else
            OnTouchReleased(touchId);
    }

    /// <summary>
    /// タッチ情報を変数に変換し、実際に判定する。<br/>
    /// 「プレイヤーがどこをタッチしたか、どの時刻に押されたか」などの情報を受け取り、<br/>InputManagerに送ることで判定している。
    /// </summary>
    private void OnTouchBegan(int touchId, Vector2 screenPos, double exactTime)
    {
        if (currentLayout == null) return;

        // 左スライドボタンが押された場合
        if (RectTransformUtility.RectangleContainsScreenPoint(currentLayout.SlideLeftArea, screenPos, targetCamera))
        {
            inputManager.OnLaneSlide(isRight: false, exactTime);
            return;
        }

        // 右スライドボタンが押された場合
        if (RectTransformUtility.RectangleContainsScreenPoint(currentLayout.SlideRightArea, screenPos, targetCamera))
        {
            inputManager.OnLaneSlide(isRight: true, exactTime);
            return;
        }

        // それ以外(レーン上かレーン外)が押された場合
        int lane = FindLane(screenPos);
        if (lane < 0) return;
        inputManager.OnLaneTap(lane, exactTime);

        // どの touchId がどの lane を抑えているか記録
        laneByTouchId[touchId] = lane;
    }

    /// <summary>
    /// その指が押していたレーンの判定を解除する。<br/>
    /// この時に、そのレーンを他の指が押していたら解除しない。
    /// </summary>
    private void OnTouchReleased(int touchId)
    {
        // そのtouchIdがどのレーンを押していたのかを見る（そもそもdictionaryにない場合は早期return）
        if (!laneByTouchId.TryGetValue(touchId, out int lane)) return;

        laneByTouchId.Remove(touchId);

        // まだほかの指が押している場合はreturn
        foreach(int otherLane in laneByTouchId.Values)
        {
            if (otherLane == lane) return;
        }

        inputManager.OnLaneRelease(lane);
    }

    /// <summary>
    /// 指定したタッチ位置が、どのレーンの内側に入っているか
    /// </summary>
    /// <returns>レーンの番号（そもそもレーンに当たってない場合は-1を返す）</returns>
    private int FindLane(Vector2 screenPos)
    {
        RectTransform[] areas = currentLayout.LaneAreas;

        // 6レーン分見て、タッチ位置がどのレーン内にあるか見る
        for (int i = 0; i < areas.Length; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(areas[i], screenPos, targetCamera))
                return i;
        }

        // そもそもタッチ位置が6レーン以外だったら-1を返す
        return -1;
    }

    /// <summary>
    /// スライドボタンのレイアウトを決定する。<br/>
    /// 以降のタッチ判定をそのレイアウトの矩形で行う。
    /// </summary>
    public void SetLayout(int index)
    {
        // layoutの何番目か。(範囲外なら早期return)
        if (layouts == null || index < 0 || index >= layouts.Length) return;

        // indexのレイアウトのみをアクティブにする
        for (int i = 0;i < layouts.Length; i++)
        {
            layouts[i].gameObject.SetActive(i == index);
        }

        currentLayout = layouts[index];
    }
}
