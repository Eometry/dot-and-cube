using Godot;

public partial class PlyCamera : Camera2D
{
    public override void _Ready()
    {
        CallDeferred(MethodName.ResetSmoothing);
    }
}
