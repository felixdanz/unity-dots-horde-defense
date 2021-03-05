using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UnitCounter : MonoBehaviour
{
	[SerializeField] private Text textDisplay;

	private EntityManager _entityManager;
	private EntityQueryDesc _unitQueryDesc;


	private void Start()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_unitQueryDesc = new EntityQueryDesc
		{
			All = new ComponentType[] {typeof(UnitData)}
		};
	}

	private void Update()
	{
		UpdateUnitCounter();
	}

	private void UpdateUnitCounter()
	{
		var unitQuery = _entityManager.CreateEntityQuery(_unitQueryDesc);
		var unitArray = unitQuery.ToEntityArray(Allocator.Temp);
		
		textDisplay.text = $"Unit Count: {unitArray.Length}";

		unitArray.Dispose();
	}
}
