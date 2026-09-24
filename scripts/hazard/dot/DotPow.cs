using Godot;

public partial class DotPow : DotBase
{
	// ===== 导出参数 =====
	[Export(PropertyHint.Range, "2,7,1")] // 幂函数次数
	public int Power { get ; set; } = 3;
	[Export] // 运动时间
	public float MoveTime { get; set; } = 0.75f;
    [Export] // 停顿时间
    public float WaitTime { get; set; } = 0.5f;
	public enum PathModes
	{
		ClosedLoop,
		BackAndForth
	}
	[Export] // 路径模式
	public PathModes PathMode { get; set; } = PathModes.ClosedLoop;

	[Export] // 路径点坐标
	public Vector2[] Waypoints { get; set; } =
	{
		new(350f, 200f),
		new(200f, 459.81f),
		new(500f, 459.81f)
	};

	// ===== 内部变量 =====
	private float _stepTime;      // 每步（径段）时间，等于 MoveTime + WaitTime
	private int _totalSeg;        // 总径段数，等于数组 _timeScales 的元素个数
	private float _timeForward;   // 正向时间，仅应用于 BackAndForth，等于半周期
	private float _timeEffective; // 有效时间，在 BackAndForth 的情况下使用三角波折叠映射进度
	private int _currentSegIdx;   // 当前径段索引，动点处于 Waypoints[i] 与 Waypoints[i+1] 的区间
	private float _powScale;      // 预计算幂 Mathf.Pow(2, Power - 1)
	private float _invMoveTime;   // 预计算 MoveTime 的倒数

	protected override void Initialize()
	{
		if (Waypoints.Length < 2)
		{
			GD.PushError("错误：列表 Waypoints 至少需要 2 个元素。");
			return;
		}
		if (MoveTime <= 0.01f || WaitTime < 0f)
		{
			GD.PushError("错误：移动时间或停顿时间必须大于 0。");
			return;
		}
		_isValid = true;

		_totalSeg = PathMode == PathModes.ClosedLoop
    		? Waypoints.Length
    		: 2 * (Waypoints.Length - 1);
		_stepTime = MoveTime + WaitTime;
		_period = _stepTime * _totalSeg;
		_timeForward = _period / 2;
		_currentSegIdx = 0;

		_powScale = Mathf.Pow(2, Power - 1);
		_invMoveTime = 1.0f / MoveTime;

		if (_isValid) GlobalPosition = Waypoints[0];
	}

    protected override void UpdatePosition()
    {
		switch (PathMode)
		{
			case PathModes.ClosedLoop:
				_timeEffective = (float)_time; // 闭环不需要折叠，直接使用
				break;
			case PathModes.BackAndForth:
				_timeEffective = (_time <= _timeForward) ? (float)_time : (_period - (float)_time);
				break;
		}
        _currentSegIdx = (int)(_timeEffective / _stepTime) % _totalSeg;
		
		Vector2 startpoint = PathMode == PathModes.ClosedLoop
			? Waypoints[_currentSegIdx]
			: _currentSegIdx < _totalSeg / 2
				? Waypoints[_currentSegIdx % (_totalSeg / 2)]
				: Waypoints[_currentSegIdx % (_totalSeg / 2) + 1];
		Vector2 endpoint = PathMode == PathModes.ClosedLoop
			? Waypoints[(_currentSegIdx + 1) % _totalSeg]
			: _currentSegIdx < _totalSeg / 2
				? Waypoints[_currentSegIdx % (_totalSeg / 2) + 1]
				: Waypoints[_currentSegIdx % (_totalSeg / 2)];
				float segTime = _timeEffective - _stepTime * _currentSegIdx;
/*
插值比例 segProg 为三段分段函数 s(t)，其中 s 为segProg，t 为 segTime / MoveTime，n 为 Power：
       ┌ 2 ^ (n-1) × t ^ n,    0 ≤ t ≤ 0.5
s(t) = │ 1 - 0.5 (2 - 2t) ^ n, 0.5 < t ＜ 1
       └ 1,                    t ≥ 1
*/
		float segProg;
		if (segTime <= 0.5f * MoveTime)
		{
    		segProg = _powScale * Mathf.Pow(segTime * _invMoveTime, Power);
		}
		else if (segTime >= MoveTime)
		{
		    segProg = 1.0f;
		}
		else
		{
		    segProg = 1.0f - Mathf.Pow(2.0f - 2.0f * segTime * _invMoveTime, Power) * 0.5f;
		}

		GlobalPosition = startpoint.Lerp(endpoint, segProg);
    }
}
