using Godot;

public abstract partial class Hazard : Node2D
{
    // 通用方法：玩家接触到 Hazard
    public void OnPlyContact()
    {
        // 简单模式下 5 血，中等 3 血，困难 1 血；
        // 血 >= 2 时死亡在上一个存档点立即复活，血 = 1 时死亡在起点立即复活；
        // 死亡瞬间所有关卡对象速度变慢，并用 shader 效果进行场景重置，参考案例：Celeste。
    }
}
