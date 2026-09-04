using Godot;

public partial class PlyVelocity : PlyBase
{
	protected override void Initialize()
	{
		GlobalPosition = SpawnPoint;
		ZIndex = 511;
	}
}
