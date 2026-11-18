using Godot;

public partial class PlyAccel : PlyBase
{
	// ===== 导出参数 =====
	[Export] // 加速度（px/s²），若满足 Accel > Damping × _speedMax，速率能在不离地条件下触及上限
	public float Accel { get; set; } = 2400f;
	[Export] // 地面阻尼系数（1/s），触地时施加指数衰减；离地时阻尼为 0（惯性保持）
	public float Damping { get; set; } = 8f;

	protected override void Initialize()
	{
		base.Initialize();
		_speedMax = 400f;
	}

	public override void _PhysicsProcess(double delta)
	{
		// 输入方向（每帧瞬时量，随算随用，不设字段）
		Vector2 dirInput = Vector2.Zero;
		if (Input.IsActionPressed("ui_left"))  dirInput.X -= 1;
		if (Input.IsActionPressed("ui_right")) dirInput.X += 1;
		if (Input.IsActionPressed("ui_up"))    dirInput.Y -= 1;
		if (Input.IsActionPressed("ui_down"))  dirInput.Y += 1;
		if (Input.IsActionJustPressed("confirm&jump"))  Jump();
		dirInput = dirInput.Normalized(); // 零向量归一化安全返回零，无需特判

		Velocity = ComputeVel(dirInput, delta);
		MoveAndSlide();
	}

	// 方法：计算速度（阻尼 → 加速 → 截断，三步顺序固定）
	private Vector2 ComputeVel(Vector2 dirInput, double delta)
	{
		// 阻尼：触地时施加指数衰减，离地（跳跃）时跳过
		if (!_isOffGround)
		{
			Velocity *= Mathf.Exp(-Damping * (float)delta);
		}

		// 加速：方向键提供推力（空中同样生效，仅无阻尼对抗）
		Velocity += dirInput * Accel * (float)delta;

		// 截断：模长超出上限则拉回上限，方向保留，满速时输入仅改变方向不增速
		// 满速转向率 = Accel / _speedMax（rad/s），如需调整满速灵活性调此比值
		float speed = Velocity.Length();
		if (speed > _speedMax)
		{
			Velocity = Velocity / speed * _speedMax;
		}
		return Velocity;
	}
}
