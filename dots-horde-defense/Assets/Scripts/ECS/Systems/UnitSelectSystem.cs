using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class UnitSelectSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
	
	private float3 _startPos;
	private float3 _endPos;
	
	
	protected override void OnCreate()
	{
		base.OnCreate();
		_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
	
	protected override void OnUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			UnselectUnits();

		if (Input.GetMouseButtonDown(0))
		{
			var raycastSuccess = CameraController.Instance.Raycast(
				Input.mousePosition,
				PhysicsUtilities.GetCollisionFilter(PhysicCategories.Ground),
				out var raycastHit);

			if (!raycastSuccess)
				return;
			
			_startPos = raycastHit.Position;
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			var raycastSuccess = CameraController.Instance.Raycast(
				Input.mousePosition,
				PhysicsUtilities.GetCollisionFilter(PhysicCategories.Ground),
				out var raycastHit);
			
			if (!raycastSuccess)
				return;

			_endPos = raycastHit.Position;
			
			UnselectUnits();
			SelectUnitsInsideRec();
		}
	}
	
	private void SelectUnitsInsideRec()
	{
		var (lowerX, upperX) = (
			math.min(_startPos.x, _endPos.x), 
			math.max(_startPos.x, _endPos.x));
		
		var (lowerZ, upperZ) = (
			math.min(_startPos.z, _endPos.z), 
			math.max(_startPos.z, _endPos.z));
		
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		
		Entities.WithAll<UnitData>().ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref Translation translation,
			ref UnitData data) =>
		{
			var entityPos = translation.Value;
			
			if (entityPos.x > lowerX && entityPos.x < upperX &&
			    entityPos.z > lowerZ && entityPos.z < upperZ)
			{
				ecb.AddComponent(entityInQueryIndex, entity, new Tag_UnitSelected());
				ecb.RemoveComponent<DisableRendering>(data.MarkerSelected.Index, data.MarkerSelected);
			}
		}).ScheduleParallel();
		
		_endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
	}

	private void UnselectUnits()
	{
		var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
		
		Entities.WithAll<Tag_UnitSelected>().ForEach((
			Entity entity,
			int entityInQueryIndex,
			ref UnitData data) =>
		{
			ecb.RemoveComponent<Tag_UnitSelected>(entityInQueryIndex, entity);
			ecb.AddComponent(data.MarkerSelected.Index, data.MarkerSelected, new DisableRendering());
		}).ScheduleParallel();
		
		_endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
	}
}