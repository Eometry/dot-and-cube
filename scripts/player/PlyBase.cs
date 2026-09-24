using Godot;

public partial class PlyBase : CharacterBody2D
{
	[Export] // 初始生成坐标
	protected Vector2 SpawnPoint = new(384f, 384f);
	protected int _hp = 5;                  // 生命值
	protected bool _isInvincible = false;   // 是否无敌
	protected float _speed = 275.0f;        // 即时速率
	protected float _speedMax;              // 最大速率
	protected Tween _plyTween;              // 受击动画 Tween 对象
	protected AudioListener2D _sndReceiver; // 2D 音效接收器
	protected static readonly Color OPAQUE = new(1, 1, 1, 1);     // 不透明
	protected static readonly Color TRANSPARENT= new(1, 1, 1, 0); // 透明
	protected static readonly Color HURT = new(1, 0.3f, 0.3f, 1); // 暗红

	public override void _Ready()
	{
		Initialize();
	}

	// 方法：初始化（由 _Ready() 调用一次）
	protected virtual void Initialize()
	{
		// 获取子节点 InnerArea 作为受击碰撞箱
		var hurtBox = GetNode<Area2D>("InnerArea");
		hurtBox.AreaEntered += OnPlyHurt;
	}

	// 方法：处理玩家受击事件
	private void OnPlyHurt(Node area)
	{
		if ((area.Name != "HazardArea") || _isInvincible) return;
		GD.Print("玩家扣血！！！");
		// 获得 1.5 秒的无敌时间
		_isInvincible = true;
		InvincibleVfx();
		GetTree().CreateTimer(1.5).Timeout += () =>
		{
    		_isInvincible = false;
		};
	}
	
	// 方法：进入无敌时间的视觉效果
	private void InvincibleVfx()
	{
		_plyTween = CreateTween();
		for (int i = 0; i < 29; i++)
		{
		_plyTween.TweenProperty(this, "modulate", TRANSPARENT, 0.025);
		_plyTween.TweenProperty(this, "modulate", HURT, 0.025);
		}
		_plyTween.TweenProperty(this, "modulate", OPAQUE, 0.05);
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
