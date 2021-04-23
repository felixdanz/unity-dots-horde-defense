using Unity.Burst;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TimerSystem))]
public class DefenseTowerAttackSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	
	
	protected override void OnCreate()
	{
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnUpdate()
	{
		var deltaTime = Time.DeltaTime;

		Entities.ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref TimerData timerData,
			ref DefenseTowerData towerData) =>
		{ 
			//var towerHeadRot = GetComponent<Rotation>(towerData.TowerHead);
			
			// check if target is set
			if (towerData.AttackTarget == Entity.Null)
			{
				
				//if (towerHeadRot.Value != quaternion.Euler(math.forward(), math.RotationOrder.Default))
					
				//var towerPos = GetComponent<Translation>(rotatingEntity);
				//var targetPos = GetComponent<Translation>(towerData.AttackTarget);
				//var targetDir = targetPos.Value - towerPos.Value;
				//var targetRotation = quaternion.LookRotation(targetDir, math.up());
				//var stepRotation = math.slerp(towerHeadRot.Value, targetRotation, deltaTime * 1.0f);
				return;
			}

			// check if target still exists
			if (!HasComponent<Translation>(towerData.AttackTarget))
			{
				towerData.AttackTarget = Entity.Null;
				return;
			}
			
			// rotate if not on target
			var towerPos = GetComponent<Translation>(entity);
			var towerHeadRot = GetComponent<Rotation>(towerData.TowerHead);
			var targetPos = GetComponent<Translation>(towerData.AttackTarget);
			var targetDir = targetPos.Value - towerPos.Value;
		
			var targetRotation = quaternion.LookRotation(targetDir, math.up());
			var stepRotation = math.slerp(towerHeadRot.Value, targetRotation, deltaTime * 1.0f);
			SetComponent<Rotation>(towerData.TowerHead, new Rotation() { Value = stepRotation });
			
			// check if target is in sight (rotation is close enough)
			// TODO(FD): quaternion.angle ?
			
			if (!timerData.IsDone)
				return;
			
			timerData.IsDone = false;
			timerData.ElapsedTime = 0;
			
			var targetHealthData = GetComponent<HealthData>(towerData.AttackTarget);
			targetHealthData.HealthCurrent -= towerData.AttackDamage;
			SetComponent<HealthData>(towerData.AttackTarget, targetHealthData);
		}).Schedule();
	}
}
