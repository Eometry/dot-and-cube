using Godot;

public partial class PlyPos : PlyBase
{
    // ===== 导出参数 =====
    public enum InputModes
    {
        Mouse,
        Keyboard
    }
    [Export] // 瞬移输入方式
    public InputModes InputMode { get; set; } = InputModes.Mouse;
    [Export] // 可瞬移范围半径（px）
    public float TeleportRadius { get; set; } = 128f;
    [Export] // 键盘模式下标识的移动速度（px/s）
    public float MarkerSpeed { get; set; } = 128f;
    [Export] // 瞬移冷却时间（s）
    public float CooldownTime { get; set; } = 0.5f;
    [Export] // 键盘模式下触发跳跃的标识邻域半径（px）：标识与玩家距离小于此值时 Z 键为跳跃
    public float JumpEpsilon { get; set; } = 16f;

    // ===== 内部变量 =====
    private AnimatedSprite2D _marker; // 瞬移标识
    private Vector2 _markerPos;       // 标识世界坐标
    private float _cooldown;          // 冷却剩余时间（≤ 0 为就绪）

    protected override void Initialize()
    {
        base.Initialize();
        _marker = GetNode<AnimatedSprite2D>("TpCursor");
        // 脱离父级变换：瞬移改写 GlobalPosition 时，标识不被父级位移带走
        _marker.TopLevel = true;
        _markerPos = GlobalPosition;
        _cooldown = 0f;
        _marker.Play(); // 初始即启动帧循环动画
        if (InputMode == InputModes.Mouse) Input.MouseMode = Input.MouseModeEnum.Hidden;
    }

    public override void _PhysicsProcess(double delta)
    {
        // 冷却计时（单时钟，与 _jumpProgress 同构）
        if (_cooldown > 0f)
        {
            _cooldown -= (float)delta;
            if (_cooldown <= 0f) OnCooldownEnd();
        }
        UpdateMarker(delta);
    }

    // 方法：更新瞬移标识
    private void UpdateMarker(double delta)
    {
        if (_cooldown > 0f) return;   // 冷却期间标识消失

        switch (InputMode)
        {
            case InputModes.Mouse:
                // 标识跟随光标；LimitLength 一并完成圆内钳制与圆外截断到连线交点
                _markerPos = GlobalPosition
                    + (GetGlobalMousePosition() - GlobalPosition).LimitLength(TeleportRadius);
                break;

            case InputModes.Keyboard:
                // 8 向步进（单键正方向、双键对角）
                Vector2 dirInput = Vector2.Zero;
                if (Input.IsActionPressed("ui_left"))  dirInput.X -= 1;
                if (Input.IsActionPressed("ui_right")) dirInput.X += 1;
                if (Input.IsActionPressed("ui_up"))    dirInput.Y -= 1;
                if (Input.IsActionPressed("ui_down"))  dirInput.Y += 1;
                _markerPos += dirInput.Normalized() * MarkerSpeed * (float)delta;
                // 限制在瞬移范围圆内
                _markerPos = GlobalPosition
                    + (_markerPos - GlobalPosition).LimitLength(TeleportRadius);
                break;
        }
        _marker.GlobalPosition = _markerPos;
        _marker.Visible = true;
    }

    // 方法：冷却结束，按模式重置标识位置（鼠标回光标处，键盘回新玩家坐标）
    private void OnCooldownEnd()
    {
        _marker.Play(); // 就绪：开始动画帧循环
        _markerPos = InputMode == InputModes.Mouse
            ? GlobalPosition + (GetGlobalMousePosition() - GlobalPosition).LimitLength(TeleportRadius)
            : GlobalPosition;
    }

    // 方法：跳跃键双语义
    // 鼠标模式：Z 键仅离地跳跃（瞬移仅限点击），不受标识位置影响
    // 键盘模式：键盘模式：冷却中或标识距玩家不足 JumpEpsilon px → 跳跃；否则瞬移
    protected override void OnJumpKey()
    {
        if (InputMode == InputModes.Mouse || _cooldown > 0f || _markerPos.DistanceTo(GlobalPosition) < JumpEpsilon)
        {
            Jump();
        }
        else Teleport();
    }

    // 方法：处理鼠标点击瞬移（键盘模式的 Z 键由 OnJumpKey 覆写处理）
    public override void _UnhandledInput(InputEvent @event)
    {
        if (InputMode == InputModes.Mouse && @event.IsActionPressed("teleport"))
        {
            if (_cooldown <= 0f) Teleport();
            return;
        }
        base._UnhandledInput(@event);   // 其余事件（含 Z 键）交回基类
    }

    // 方法：执行瞬移（与跳跃状态互相独立：空中可瞬移，冷却中可跳跃）
    private void Teleport()
    {
        GlobalPosition = _markerPos;
	    _cooldown = CooldownTime;
	    _marker.Stop(); // 停止动画帧循环
	    _marker.Visible = false;
    }

    public override void _ExitTree()
    {
        // 离开场景（切关卡/回主菜单）时恢复系统光标，避免全局状态泄漏到 UI 界面
        if (InputMode == InputModes.Mouse) Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}
