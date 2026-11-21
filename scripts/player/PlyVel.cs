using Godot;

public partial class PlyVel : PlyBase
{
	protected override void Initialize()
	{
		base.Initialize();
		_speedMax = 200f;
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
		dirInput = dirInput.Normalized() * _speedMax;

		Velocity = dirInput;
		MoveAndSlide();
	}
}
