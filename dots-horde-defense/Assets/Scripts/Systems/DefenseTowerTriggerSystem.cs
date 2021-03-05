using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class DefenseTowerTriggerSystem : SystemBase
{
	private StepPhysicsWorld _stepPhysicsWorld;
	private BuildPhysicsWorld _buildPhysicsWorld;
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


	protected override void OnCreate()
	{
		_stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
		_buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnUpdate()
	{
		var translationGroup = GetComponentDataFromEntity<Translation>(true);
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		
		// check if targets left triggers
		Entities.ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref PhysicsCollider collider,
			ref DefenseTowerData towerData) =>
		{
			// check if target is set
			if (towerData.AttackTarget == Entity.Null)
				return;

			// check if target still exists
			if (!translationGroup.HasComponent(towerData.AttackTarget))
			{
				towerData.AttackTarget = Entity.Null;
				return;
			}

			var towerPos = translationGroup[entity];
			var targetPos = translationGroup[towerData.AttackTarget];
			var distanceToTarget = math.distance(targetPos.Value, towerPos.Value);

			if (distanceToTarget > towerData.AttackRange)
			{
				towerData.AttackTarget = Entity.Null;
			}
		}).WithReadOnly(translationGroup).ScheduleParallel();

		// check for current TriggerEvents
		Dependency = new DefenseTowerTriggerJob()
		{
			DefenseTowerDataGroup = GetComponentDataFromEntity<DefenseTowerData>(),
			UnitDataGroup = GetComponentDataFromEntity<UnitData>(),
			CommandBuffer = ecb,
		}.Schedule(
			_stepPhysicsWorld.Simulation,
			ref _buildPhysicsWorld.PhysicsWorld,
			Dependency);
		
		_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
	}

	[BurstCompile]
	private struct DefenseTowerTriggerJob : ITriggerEventsJob
	{
		[ReadOnly] public ComponentDataFromEntity<DefenseTowerData> DefenseTowerDataGroup;
		[ReadOnly] public ComponentDataFromEntity<UnitData> UnitDataGroup;
		public EntityCommandBuffer.ParallelWriter CommandBuffer;


		public void Execute(TriggerEvent triggerEvent)
		{
			var entityA = triggerEvent.EntityA;
			var entityB = triggerEvent.EntityB;

			var isEntityATower = DefenseTowerDataGroup.HasComponent(entityA);
			var isEntityBTower = DefenseTowerDataGroup.HasComponent(entityB);

			var isEntityAUnit = UnitDataGroup.HasComponent(entityA);
			var isEntityBUnit = UnitDataGroup.HasComponent(entityB);

			var towerEntity = isEntityATower ? entityA : entityB;
			var unitEntity = isEntityAUnit ? entityA : entityB;

			if (isEntityATower && isEntityBUnit ||
			    isEntityBTower && isEntityAUnit)
			{
				var towerData = DefenseTowerDataGroup[towerEntity];

				if (towerData.AttackTarget != Entity.Null)
					return;

				towerData.AttackTarget = unitEntity;
				CommandBuffer.SetComponent<DefenseTowerData>(towerEntity.Index, towerEntity, towerData);
			}
		}
	}
}