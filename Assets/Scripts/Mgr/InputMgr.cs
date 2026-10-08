using UnityEngine;

/// <summary>
/// 全局输入管理器 —— 支持改键、多设备、事件驱动
/// </summary>
public class InputMgr
{
    private static InputMgr instance=new InputMgr();
    public static InputMgr Instance=>instance ?? (instance = new InputMgr());

    [Header("移动设置")]
    public KeyCode forward = KeyCode.W;
    public KeyCode back = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;

    [Header("视角")]
    public float mouseSensitivity = 1f;

    [Header("战斗")]
    public KeyCode fire = KeyCode.Mouse0;
    public KeyCode reload = KeyCode.R;
    public KeyCode jump = KeyCode.Space;

    // 输入值（每帧都会更新）
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsFiring { get; private set; }
    public bool IsReloading { get; private set; }
    public bool IsJumping { get; private set; }

    private void Update()
    {
        UpdateMoveInput();
        UpdateLookInput();
        UpdateActionInput();
    }

    // 移动
    private void UpdateMoveInput()
    {
        float h = 0;
        float v = 0;

        if (Input.GetKey(forward)) v += 1;
        if (Input.GetKey(back)) v -= 1;
        if (Input.GetKey(left)) h -= 1;
        if (Input.GetKey(right)) h += 1;

        MoveInput = new Vector2(h, v).normalized;
    }

    // 视角
    private void UpdateLookInput()
    {
        float x = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float y = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        LookInput = new Vector2(x, y);
    }

    // 战斗动作
    private void UpdateActionInput()
    {
        IsFiring = Input.GetKey(fire);
        IsReloading = Input.GetKeyDown(reload);
        IsJumping = Input.GetKeyDown(jump);
    }
}