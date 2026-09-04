using Godot;

public partial class PlyBase : CharacterBody2D
{
	[Export] // 初始生成坐标
	protected Vector2 SpawnPoint = new(384f, 384f);
	protected float _speed = 275.0f;        // 通常速率
	protected float _speedMax;              // 最大速率
	protected AudioListener2D _sndReceiver; // 2D 音效接收器

	public override void _Ready()
	{
		Initialize();
	}

	// 方法：初始化（由 _Ready() 调用一次）
	protected virtual void Initialize()
	{
		/*
		===== 子类重写此虚方法，实现初始化逻辑 =====
		*/
	}
	
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Vector2.Zero;

		if (Input.IsActionPressed("ui_left"))  velocity.X -= 1;
		if (Input.IsActionPressed("ui_right")) velocity.X += 1;
		if (Input.IsActionPressed("ui_up"))    velocity.Y -= 1;
		if (Input.IsActionPressed("ui_down"))  velocity.Y += 1;

		if (velocity != Vector2.Zero)
		{
			velocity = velocity.Normalized() * _speed;
		}

		Velocity = velocity;

		MoveAndSlide();
	}
}
