using Godot;

public partial class PlyBase : CharacterBody2D
{
	// ===== 导出参数 =====
	[Export] // 初始生成坐标
	protected Vector2 SpawnPoint { get; set; } = new(384f, 384f);
	[Export] // 跳跃持续时间（起跳到落回）
	protected float JumpTime { get; set; } = 1f;

	// ===== 内部变量 =====
	protected byte _hp = 3;                 // 生命值
	protected bool _isOffGround = false;    // 是否离地
	protected bool _isInvincible = false;   // 是否无敌
	protected float _speed = 275f;          // 即时速率
	protected float _speedMax;              // 最大速率
	protected float _jumpProgress = 0f;     // 跳跃进度，0 为起跳，1 为落回
	protected Tween _hurtTween;             // 受击动画 Tween 对象
	protected AudioListener2D _sndReceiver; // 2D 音效接收器
	protected static readonly Color OPAQUE = new(1, 1, 1, 1);      // 不透明
	protected static readonly Color TRANSPARENT = new(1, 1, 1, 0); // 透明
	protected static readonly Color HURT = new(1, 0.3f, 0.3f, 1);  // 暗红

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
		InvincibleVfx();
		GetTree().CreateTimer(1.5).Timeout += () => _isInvincible = false;
	}

	// 方法：无敌效果的视觉表现
	private void InvincibleVfx()
	{
		_hurtTween?.Kill();
		_hurtTween = CreateTween();
		for (int i = 0; i < 29; i++)
		{
			_hurtTween.TweenProperty(this, "modulate", TRANSPARENT, 0.025);
			_hurtTween.TweenProperty(this, "modulate", HURT, 0.025);
		}
		_hurtTween.TweenProperty(this, "modulate", OPAQUE, 0.05);
	}

	// 方法：跳跃
	private void Jump()
	{
		if (_isOffGround) return;
		_isOffGround = true;
		_jumpProgress = 0f;
	}

	// 方法：跳跃视觉表现
	// 缩放函数 s = 2 − (2p − 1)²，p ∈ [0,1] 为跳跃进度
	// 即 y = −4/t² · (x − t/2)² + 2 的归一化形式：起跳 1 → 最高点 2 → 落回 1
	private void JumpVfx(double delta)
	{
		_jumpProgress += (float)delta / JumpTime;
		if (_jumpProgress >= 1f)
		{
			Scale = Vector2.One;
			_isOffGround = false;
			return;
		}
		float apexOffset = 2f * _jumpProgress - 1f;      // 离顶点偏移：起跳 -1 → 顶点 0 → 落回 1
		float scaleValue = 2f - apexOffset * apexOffset; // Scale 缩放倍率：1 → 2 → 1
		Scale = new(scaleValue, scaleValue);
	}

	public override void _Process(double delta)
	{
		if (_isOffGround) JumpVfx(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Vector2.Zero;

		if (Input.IsActionPressed("ui_left"))  velocity.X -= 1;
		if (Input.IsActionPressed("ui_right")) velocity.X += 1;
		if (Input.IsActionPressed("ui_up"))    velocity.Y -= 1;
		if (Input.IsActionPressed("ui_down"))  velocity.Y -= 1;
		if (Input.IsActionJustPressed("confirm&jump")) Jump();

		if (velocity != Vector2.Zero)
		{
			velocity = velocity.Normalized() * _speed;
		}

		Velocity = velocity;

		MoveAndSlide();
	}
}