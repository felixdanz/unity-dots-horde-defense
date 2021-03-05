using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
	[Header("Data")]
	[SerializeField] private List<BuildEntry> buildEntries;

	[Header("Components")] 
	[SerializeField] private Transform entryParent;
	[SerializeField] private BuildMenuEntry entryPrefab;
	
	private int _selectedBuildingIndex;
	private GameObject _buildingPreview;

	private EntityManager _entityManager;
	private BlobAssetStore _blobAssetStore;
	private PathfindingRefreshSystem _pathfindingRefreshSystem;

	
	private void Start()
	{
		_selectedBuildingIndex = -1;
		
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_blobAssetStore = new BlobAssetStore();

		_pathfindingRefreshSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PathfindingRefreshSystem>();

		var conversionSettings = GameObjectConversionSettings.FromWorld(
			World.DefaultGameObjectInjectionWorld, 
			_blobAssetStore);
		
		for (var i = 0; i < buildEntries.Count; i++)
		{
			var buildEntry = buildEntries[i];
			var convertedPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
				buildEntries[i].buildingPrefab, 
				conversionSettings);

			buildEntry.BuildingEntity = convertedPrefab;
			buildEntries[i] = buildEntry;
		}
	}

	private void Update()
	{
		if (_selectedBuildingIndex == -1)
			return;

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			_selectedBuildingIndex = -1;
			Destroy(_buildingPreview.gameObject);
			return;
		}
		
		var raycastSuccess = CameraController.Instance.Raycast(
			Input.mousePosition,
			PhysicsUtilities.GetCollisionFilter(PhysicCategories.Ground), 
			out var raycastHit);
		
		if (!raycastSuccess)
			return;
		
		var closestNode = GridController.Instance.Grid.GetClosestNode(raycastHit.Position);
		_buildingPreview.transform.position = closestNode.WorldPosition;

		if (Input.GetMouseButtonDown(0) && !closestNode.IsBlocked)
		{
			PlaceSelectedBuildingAt(closestNode);
		}
	}

	private void PlaceSelectedBuildingAt(GridNode targetNode)
	{
		var instance = _entityManager.Instantiate(buildEntries[_selectedBuildingIndex].BuildingEntity);
		
		_entityManager.SetComponentData<Translation>(instance, new Translation()
		{
			Value = targetNode.WorldPosition
		});
		
		targetNode.SetIsBlocked(true);
		_pathfindingRefreshSystem.RequestRefresh();
	}

	private void SelectBuilding(int buildingIdx)
	{
		if (_buildingPreview != null)
			Destroy(_buildingPreview);
		
		_selectedBuildingIndex = buildingIdx;
		_buildingPreview = Instantiate(buildEntries[_selectedBuildingIndex].buildingPreview);
	}

	private void UpdateView()
	{
		foreach (Transform entry in entryParent)
			Destroy(entry.gameObject);

		var count = 0;
		
		foreach (var buildEntry in buildEntries)
		{
			var buildingIdx = count;
			var buildMenuEntry = Instantiate(entryPrefab, entryParent);
			buildMenuEntry.UpdateView(buildEntry.icon, buildEntry.buildingName);
			buildMenuEntry.Button.onClick.AddListener(() => SelectBuilding(buildingIdx));
			count++;
		}
	}

	private void OnEnable()
	{
		UpdateView();
	}

	private void OnDestroy()
	{
		_blobAssetStore.Dispose();
	}
}

[Serializable]
public struct BuildEntry
{
	public string buildingName;
	public Sprite icon;
	public GameObject buildingPreview;
	public GameObject buildingPrefab;
	public Entity BuildingEntity;
}