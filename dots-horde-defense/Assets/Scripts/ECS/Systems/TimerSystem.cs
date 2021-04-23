using Unity.Entities;
using UnityEngine;

public class TimerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        
        Entities.ForEach((
            ref TimerData timer) =>
        {
            if (timer.ElapsedTime >= timer.Interval)
            {
                timer.IsDone = true;
            }
            
            if (timer.IsDone)
                return;
            
            timer.ElapsedTime += deltaTime;
        }).ScheduleParallel();
    }
}