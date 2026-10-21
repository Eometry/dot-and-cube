using Godot;

public partial class DotEllipse : DotBase
{
    // ===== 导出参数 =====
    [Export] // 椭圆圆心坐标
    public Vector2 OrbitCenter { get; set; } = new(440f, 440f);
    [Export] // 横半轴
    public float XRadius { get; set; } = 120f;
    [Export] // 纵半轴
    public float YRadius { get; set; } = 120f;
    [Export] // 周期
    public float Period { get; set; } = 2f;
    [Export] // 初始角度（角度制）
    public float InitialAngleDeg { get; set; } = 0f;
    public enum RotationDirs
    {
        Clockwise,
        Counterclockwise
    }
    [Export] // 转动方向
    public RotationDirs RotationDirection { get; set; } = RotationDirs.Counterclockwise;
    
    // ===== 内部变量 =====
    private float _initialAngleRad; // 初始角度（弧度制）
    private float _omega;           // 三角函数角速度

    protected override void Initialize()
    {
		if (Period <= 0.01f)
		{
			GD.PrintErr("错误：周期必须大于 0。");
			return;
		}
		if (XRadius <= 0.01f || YRadius <= 0.01f)
		{
			GD.PrintErr("错误：横半轴或纵半轴必须大于 0。");
			return;
		}
		_isValid = true;

        //周期赋值
        _period = Period;
        // 计算角速度 ω = 2π / T
        _omega = Mathf.Tau / _period;
        // 初始角弧度
        _initialAngleRad = Mathf.DegToRad(InitialAngleDeg);
        // 初始坐标，椭圆的三角表示
		(float sinValue, float cosValue) = Mathf.SinCos(_initialAngleRad);
        Vector2 initialPosition = new(XRadius * cosValue, YRadius * sinValue);
        GlobalPosition = OrbitCenter + initialPosition;

        // 旋转方向，逆时针 = 1，顺时针 = -1
		int rotationDirection = RotationDirection == RotationDirs.Clockwise
			? -1
			: 1;
        _omega *= rotationDirection;
    }

	// 方法：更新坐标
	protected override void UpdatePosition()
	{
		float angle = (float)(_omega * _time + _initialAngleRad);
		(float sinValue, float cosValue) = Mathf.SinCos(angle);
		Vector2 distance = new(cosValue * XRadius, sinValue * YRadius);
		GlobalPosition = OrbitCenter + distance;
	}
}
