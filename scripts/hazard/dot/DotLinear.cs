using Godot;

public partial class DotLinear : DotBase
{
	// ===== 导出参数 =====
	[Export] // 每秒移动速度
	public float Speed { get; set; } = 150f;
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
		new(200, 200),
		new(400, 200),
		new(400, 400),
		new(200, 400)
	};

	// ===== 内部变量 =====
	private float[] _accTimes;    // 累计时间表
	private float _accDist;       // 累计距离
	private float _timeForward;   // 正向时间，仅应用于 BackAndForth，等于半周期
	private float _timeEffective; // 有效时间，在 BackAndForth 的情况下使用三角波折叠映射进度
	private int _currentSegIdx;   // 当前线段指数，动点处于 Waypoints[i] 与 Waypoints[i+1] 的区间

	protected override void Initialize()
	{
		if (Waypoints.Length < 2)
		{
			GD.PrintErr("错误：列表 Waypoints 至少需要 2 个元素。");
			return;
		}
		if (Speed <= 0f)
		{
			GD.PrintErr("错误：速度必须大于 0。");
			return;
		}
		_isValid = true;
		BuildTimetable();
		if (_isValid) GlobalPosition = Waypoints[0];
	}

	// 方法：建立时间表
	private void BuildTimetable()
	{
		_accDist = 0f;
		int tableSize = PathMode == PathModes.ClosedLoop
		? Waypoints.Length + 1
		: Waypoints.Length; //时间表大小
		_accTimes = new float[tableSize];
		_accTimes[0] = 0f; // 起始时间 0
		for (int i = 0; i < Waypoints.Length - 1; i++)
		{
			float segmentLength = Waypoints[i].DistanceTo(Waypoints[i + 1]);
			if (segmentLength <= 0.01f)
			{
				GD.PrintErr($"错误：Waypoints[{i}] 与 Waypoints[{i + 1}] 坐标重合或距离过近。");
				_isValid = false;
				return;
			}
			_accDist += segmentLength;
			_accTimes[i + 1] = _accDist / Speed;
		}
		switch (PathMode)
		{
			case PathModes.ClosedLoop:
				// 闭环多一段：最后一个节点回到起点
				if (Waypoints[Waypoints.Length - 1].DistanceTo(Waypoints[0]) <= 0.01f)
				{
					GD.PrintErr("错误：Closed Loop 路径模式下，最后一个节点与起点坐标重合或距离过近。");
					_isValid = false;
					return;
				}
				_accDist += Waypoints[Waypoints.Length - 1].DistanceTo(Waypoints[0]);
				_accTimes[tableSize - 1] = _accDist / Speed;
				_period = _accTimes[tableSize - 1];
				break;

			case PathModes.BackAndForth:
				_timeForward = _accDist / Speed;
				_period = _timeForward * 2f;
				break;

			default:
				GD.PrintErr("错误：未输入有效的路径模式。");
				_isValid = false;
				break;
		}
	}

	// 方法：更新当前线段指数
	private void UpdateSegmentIndex()
	{
		while (_currentSegIdx < _accTimes.Length - 2 && _timeEffective > _accTimes[_currentSegIdx + 1])
		{
			_currentSegIdx += 1;
		}
		if (PathMode == PathModes.BackAndForth)
		{
			while (_currentSegIdx > 0 && _timeEffective < _accTimes[_currentSegIdx])
			{
				_currentSegIdx -= 1;
			}
		}
	}

	protected override void UpdatePosition()
	{
		// 确定路径模式
		// TODO：优化架构，消除每帧的 switch 判断
		switch (PathMode)
		{
			case PathModes.ClosedLoop:
				_timeEffective = (float)_time; // 闭环不需要折叠，直接使用
				break;
			case PathModes.BackAndForth:
				_timeEffective = (_time <= _timeForward) ? (float)_time : (_period - (float)_time);
				break;
		}
		// 先更新当前线段指数
		UpdateSegmentIndex();
		// 始/末路径点坐标
		Vector2 startPoint = Waypoints[_currentSegIdx];
		Vector2 endPoint = Waypoints[(_currentSegIdx + 1) % Waypoints.Length]; // 取模防止浮点误差导致时间表上界溢出
		// 插值比例
		float segmentProgress = (_timeEffective - _accTimes[_currentSegIdx]) / 
			(_accTimes[_currentSegIdx + 1] - _accTimes[_currentSegIdx]);

		GlobalPosition = startPoint.Lerp(endPoint, segmentProgress);
	}

    protected override void OnPeriodReset()
    {
        _currentSegIdx = 0;
    }
}