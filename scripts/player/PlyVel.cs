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
		Vector2 velocity = Vector2.Zero;

		if (Input.IsActionPressed("ui_left"))  velocity.X -= 1;
		if (Input.IsActionPressed("ui_right")) velocity.X += 1;
		if (Input.IsActionPressed("ui_up"))    velocity.Y -= 1;
		if (Input.IsActionPressed("ui_down"))  velocity.Y += 1;
		if (Input.IsActionJustPressed("confirm&jump")) Jump();

		if (velocity != Vector2.Zero)
		{
			velocity = velocity.Normalized() * _speedMax;
		}

		Velocity = velocity;

		MoveAndSlide();
	}
}
