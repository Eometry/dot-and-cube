using Godot;

public partial class DotSinewave : DotBase
{
	// ===== 导出参数 =====
	[Export] // 原点坐标
	public Vector2 Origin { get; set; } = new(128f, 128f);
	[Export] // 半波长数量
	public int HalfWaveCount { get; set; } = 5;
	[Export] // 振幅
	public float Amplitude { get; set; } = 128f;
	[Export] // 半波长长度
	public float HalfWaveLength { get; set; } = 64f;
	[Export] // 半波周期
	public float HalfWavePeriod { get; set; } = 0.75f;
	[Export(PropertyHint.Range, "0.00, 0.99, 0.01")]   // 初线性进程，0 → 原点，0.5 → 折返点
	public float InitialLinearProgress { get; set; } = 0f;
	[Export(PropertyHint.Range, "-180.0, 180.0, 0.1")] // 初相位（角度制，作用于 _dirVibration）
	public float PhaseOffsetDeg { get; set; } = -90f;
	[Export(PropertyHint.Range, "-180.0, 180.0, 0.1")] // 旋转角度（角度制，作用于 _dirLinear）
	public float RotationDeg { get; set; } = 0f;

	// ===== 内部变量 =====
	private Vector2 _dirLinear;    // 线性运动方向向量（乘以线性距离总长（半程））
	private Vector2 _dirVibration; // 简谐振动方向向量（乘以振幅）
	private float _omega;          // 三角函数角频率
	private float _phaseOffsetRad; // 初相位（弧度制）
	private float _linearScale;    // 初线性位移动态比例

	protected override void Initialize()
	{
		if (HalfWaveCount < 1)
		{
			GD.PrintErr("半波长数量必须大于等于 1。");
			return;
		}
		if (HalfWaveLength <= 0.01f)
		{
			GD.PrintErr("半波长长度必须大于 0。");
			return;
		}
		if (HalfWavePeriod <= 0.01f)
		{
			GD.PrintErr("半周期必须大于 0。");
			return;
		}
		_period = HalfWavePeriod * HalfWaveCount * 2;
		_isValid = true;

		// 关于 _dirVibration
		// 计算角频率
		_omega = Mathf.Pi / HalfWavePeriod;
		// 初相位弧度
		_phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
		// 方向向量 _dirVibration 乘以振幅
		_dirVibration = Vector2.Down.Rotated(Mathf.DegToRad(RotationDeg)) * Amplitude;
		// 初相位 → 振幅系数
		float initialVibrationFactor = Mathf.Sin(_phaseOffsetRad);

		// 关于 _dirLinear
		// 线性距离总长（半程）
		float distLinear = HalfWaveCount * HalfWaveLength;
		// 方向向量 _dirLinear，与 _dirVibration 垂直，乘以线性距离总长
		_dirLinear = Vector2.Right.Rotated(Mathf.DegToRad(RotationDeg)) * distLinear;
		// 初线性位移比例，根据三角波折叠推出分段函数
		_linearScale = 1f - Mathf.Abs(2f * InitialLinearProgress - 1f);

		GlobalPosition = Origin + _dirVibration * initialVibrationFactor + _dirLinear * _linearScale;
	}

    protected override void UpdatePosition()
    {
		_linearScale = InitialLinearProgress + (float)_time / _period;
		if (_linearScale > 1f) _linearScale %= 1f;
		_linearScale = 1f - Mathf.Abs(2f * _linearScale - 1f);
		GlobalPosition = Origin + _dirVibration * Mathf.Sin((float)(_omega * _time + _phaseOffsetRad)) + _dirLinear * _linearScale;
    }
}
