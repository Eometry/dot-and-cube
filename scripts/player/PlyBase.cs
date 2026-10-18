using Godot;

public partial class PlyBase : CharacterBody2D
{
	[Export] // 初始生成坐标
	protected Vector2 SpawnPoint = new(384f, 384f);
	protected byte _hp = 5;                // 生命值
	protected byte _distOffGround = 0;     // 离地高度，默认 0 即触地，255 为最高离地
	protected bool _isOffGround = false;    // 是否离地
	protected bool _isInvincible = false;   // 是否无敌
	protected float _speed = 275.0f;        // 即时速率
	protected float _speedMax;              // 最大速率
	protected Tween _hurtTween;             // 受击动画 Tween 对象
	protected Tween _jumpTween;             // 跳跃动画 Tween 对象
	protected AudioListener2D _sndReceiver; // 2D 音效接收器
	protected const float FUNCTION_CONST = 4f / 65025f;           // 计算 scaleScale 的二次函数系数
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
		if ((area.Name != "HazardArea") || _isInvincible || _isOffGround) return;
		GD.Print("玩家扣血！！！");
		// 获得 1.5 秒的无敌时间
		_isInvincible = true;
		Invincible();
		GetTree().CreateTimer(1.5).Timeout += () =>	_isInvincible = false;
	}
	
	// 方法：进入无敌时间
	private void Invincible()
	{
		_hurtTween = CreateTween();
		for (int i = 0; i < 29; i++)
		{
		// BUG：执行所有 TweenProperty() 方法时必定同时瞬时发生，导致根本不呈现动画效果；但是以前这么写的没这个问题。
		_hurtTween.TweenProperty(this, "modulate", TRANSPARENT, 0.025);
		_hurtTween.TweenProperty(this, "modulate", HURT, 0.025);
		}
		_hurtTween.TweenProperty(this, "modulate", OPAQUE, 0.05);
		_hurtTween.Kill();
	}

	// 方法：跳跃
	private void Jump()
	{
		float scaleScale = 0f;
		_distOffGround = 0;
		_jumpTween.Kill();
		// 跳跃从起跳到落回用时 1.28 秒；_distOffGround 在此期间匀速增至最高点再折回
		// y = 1 + 4x (255 - x) / 65025
		for (_distOffGround = 0 ; _distOffGround < 255; _distOffGround++)
		{
			scaleScale = 1f + _distOffGround * (255 - _distOffGround) * FUNCTION_CONST;
			_jumpTween.TweenProperty(this, "scale", scaleScale, 0.0025);
		}
		for (_distOffGround = 255 ; _distOffGround > 0; _distOffGround--)
		{
			scaleScale = 1f + _distOffGround * (255 - _distOffGround) * FUNCTION_CONST;
			_jumpTween.TweenProperty(this, "scale", scaleScale, 0.0025);
		}
		_distOffGround = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Vector2.Zero;

		if (Input.IsActionPressed("ui_left"))  velocity.X -= 1;
		if (Input.IsActionPressed("ui_right")) velocity.X += 1;
		if (Input.IsActionPressed("ui_up"))    velocity.Y -= 1;
		if (Input.IsActionPressed("ui_down"))  velocity.Y += 1;
		if (Input.IsKeyPressed(Key.Z))         Jump();

		if (velocity != Vector2.Zero)
		{
			velocity = velocity.Normalized() * _speed;
		}

		Velocity = velocity;

		MoveAndSlide();
	}
}
