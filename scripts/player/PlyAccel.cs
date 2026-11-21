using Godot;

public partial class PlyAccel : PlyBase
{
	// ===== 导出参数 =====
    [Export] // 加速度 (px/s²)，若满足 Accel > Damping × _speedMax，速率能在不离地条件下触及上限
    public float Accel { get; set; } = 800f;
    [Export] // 地面阻尼系数 (Hz)，触地时施加指数衰减；离地时阻尼为 0
    public float Damping { get; set; } = 2.5f;

    protected override void Initialize()
    {
        base.Initialize();
        _speedMax = 400f;
    }

    public override void _PhysicsProcess(double delta)
    {
        // 输入方向
        Vector2 dirInput = Vector2.Zero;
        if (Input.IsActionPressed("ui_left"))  dirInput.X -= 1;
        if (Input.IsActionPressed("ui_right")) dirInput.X += 1;
        if (Input.IsActionPressed("ui_up"))    dirInput.Y -= 1;
        if (Input.IsActionPressed("ui_down"))  dirInput.Y += 1;
        if (Input.IsActionJustPressed("confirm&jump"))  Jump();
        // 向量的归一化（零向量归一化安全返回零，无需特判）
        dirInput = dirInput.Normalized();

        Velocity = ComputeVel(dirInput, delta);
        MoveAndSlide();
    }

    // 方法：计算速度（阻尼 → 加速 → 截断）
    private Vector2 ComputeVel(Vector2 dirInput, double delta)
    {
        // 阻尼：触地时施加指数衰减，离地时为 0 即跳过
        if (!_isOffGround)
        {
            // 由微分方程 dv / dt = -kv 推导出 v = v₀ e^(-kt)
            Velocity *= Mathf.Exp(-Damping * (float)delta);
        }

        // 加速：方向键提供推力
        Velocity += dirInput * Accel * (float)delta;

        // 截断：速度模长超出上限则拉回上限，方向保留，满速时输入仅改变方向不增速
        // 满速转向率 = Accel / _speedMax（rad/s)
        float speed = Velocity.Length();
        if (speed > _speedMax)
        {
            Velocity = Velocity * _speedMax / speed;
        }
        return Velocity;
    }
}
