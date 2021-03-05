using Unity.Entities;

public struct TimerData : IComponentData
{
    public float Interval;
    public float ElapsedTime;
    public bool IsDone;
}