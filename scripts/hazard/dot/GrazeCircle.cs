using Godot;

public partial class GrazeCircle : Sprite2D
{
    private Tween _grazeTween;              // 渐隐动画 Tween 对象
    private AudioStreamPlayer2D _grazeSnd;  // 擦弹音效
    private RandomNumberGenerator _rng;     // 音高随机数
    private bool _isGrazed = false;         // 是否擦弹
    private const float EXIT_DELAY = 0.05f; // 从擦弹碰撞结束到渐隐动画开始之前的延迟时间
    private const float FADE_TIME = 0.25f;  // 渐隐动画的持续时间
    private const float PITCH_MIN = 0.80f;  // 最小随机音高
    private const float PITCH_MAX = 1.25f;  // 最大随机音高
    private static readonly Color TRANSPARENT = new(1, 1, 1, 0); // 透明
    private static readonly Color OPAQUE = new(1, 1, 1, 1);      // 不透明

    public override void _Ready()
    {
        // 获取子节点 GrazeArea 作为擦弹碰撞箱
        var grazeArea = GetNode<Area2D>("GrazeArea");
        // 连接 Area2D 信号
        grazeArea.BodyEntered += OnGrazeCircleEntered;
        grazeArea.BodyExited  += OnGrazeCircleExited;
        // 获取子节点 GrazeSnd 并加固其 PanningStrength 属性
        _grazeSnd = GetNode<AudioStreamPlayer2D>("GrazeSnd");
        _grazeSnd.PanningStrength = 80.0f;
		// 初始不透明度设置为 0
		Modulate = TRANSPARENT;
        // 隐藏以降低渲染压力
        Visible = false;
        // 初始化随机数生成器
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
    }

    // 方法：处理玩家进入擦弹圈事件
    private void OnGrazeCircleEntered(Node2D body)
    {
        if (body is PlyBase)
        {
            _isGrazed = true;
            // 设定随机音高并播放
            _grazeSnd.PitchScale = _rng.RandfRange(PITCH_MIN, PITCH_MAX);
            _grazeSnd.Play();

            PlayGhostEffect();
        }
    }

    // 方法：处理玩家退出擦弹圈事件
    private void OnGrazeCircleExited(Node2D body)
    {
        if (body is PlyBase)
        {
           _isGrazed = false;
           // 等待 0.05 秒后判断
           GetTree().CreateTimer(EXIT_DELAY).Timeout += () =>
           {
               if (!_isGrazed) ExitGhostEffect();
           }; 
        }
    }

    // 方法：触发渐隐动画
    private void PlayGhostEffect()
    {
        Visible = true;
        // 终止之前的渐隐 Tween 动画（如有）
        _grazeTween?.Kill();
        // 立即设置不透明度为 100%
        Modulate = OPAQUE;
    }

    // 方法：退出渐隐动画
    private void ExitGhostEffect()
    {
        _grazeTween = CreateTween();
        _grazeTween.TweenProperty(this, "modulate", TRANSPARENT, FADE_TIME);
        // Tween 播放完毕后隐藏
        _grazeTween.TweenCallback(Callable.From(() => Visible = false));
    }
}
