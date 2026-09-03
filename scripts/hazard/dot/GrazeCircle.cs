using Godot;

public partial class GrazeCircle : Sprite2D
{
    private Tween _grazeTween;             // 渐隐动画 Tween 对象
    private AudioStreamPlayer2D _grazeSnd; // 擦弹音效
    private bool _isGrazed = false;        // 是否擦弹
    private const float EXIT_DELAY = 0.05f;
    private const float FADE_TIME = 0.25f;
    private static readonly Color TRANSPARENT = new(1, 1, 1, 0); // 透明
    private static readonly Color OPAQUE = new(1, 1, 1, 1);      // 不透明

    public override void _Ready()
    {
        // 获取子节点 Area2D
        var grazeArea = GetNode<Area2D>("Area2D");
        // 连接信号
        grazeArea.BodyEntered += OnGrazeCircleEntered;
        grazeArea.BodyExited  += OnGrazeCircleExited;
        _grazeSnd = GetNode<AudioStreamPlayer2D>("GrazeSnd");
		// 不透明设置为 0
		Modulate = TRANSPARENT;
        // 隐藏以降低渲染压力
        Visible = false;
    }

    // 方法：玩家是否进入擦弹圈
    private void OnGrazeCircleEntered(Node2D body)
    {
        if (body is PlyVelocity)
        {
            _isGrazed = true;
            _grazeSnd.Play();
            PlayGhostEffect();
        }
    }

    // 方法：玩家是否退出擦弹圈
    private void OnGrazeCircleExited(Node2D body)
    {
        if (body is PlyVelocity)
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