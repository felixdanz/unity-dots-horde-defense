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
	private List<GameObject> _buildingPreviews;
	private GridNode _firstHit;
	private List<GridNode> _selectedNodes;

	private EntityManager _entityManager;
	private BlobAssetStore _blobAssetStore;
	private PathfindingRefreshSystem _pathfindingRefreshSystem;


	private void Start()
	{
		_selectedBuildingIndex = -1;
		_buildingPreviews = new List<GameObject>();
		_selectedNodes = new List<GridNode>();
		
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
			DestroyBuildingPreviews();
			return;
		}
		
		var raycastSuccess = CameraController.Instance.Raycast(
			Input.mousePosition,
			PhysicsUtilities.GetCollisionFilter(PhysicCategories.Ground), 
			out var raycastHit);
		
		if (!raycastSuccess)
			return;
		
		var closestNode = GridController.Instance.Grid.GetClosestNode(raycastHit.Position);

		if (Input.GetMouseButtonDown(0) && !closestNode.IsBlocked)
		{
			_firstHit = closestNode;
		}

		if (Input.GetMouseButton(0))
		{
			_selectedNodes = GridController.Instance.Grid.GetNodesBetween(
				_firstHit, 
				closestNode);

			if (_buildingPreviews.Count < _selectedNodes.Count)
			{
				for (var i = _buildingPreviews.Count; i < _selectedNodes.Count; i++)
				{
					_buildingPreviews.Add(Instantiate(buildEntries[_selectedBuildingIndex].buildingPreview));
				}
			}
			else if (_buildingPreviews.Count > _selectedNodes.Count)
			{
				for (var i = _selectedNodes.Count; i < _buildingPreviews.Count; i++)
				{
					var toRemove = _buildingPreviews[i];
					_buildingPreviews.Remove(toRemove);
					Destroy(toRemove);
				}
			}
			
			for (var i = 0; i < _selectedNodes.Count - 1; i++)
			{
				_buildingPreviews[i].transform.position = _selectedNodes[i].WorldPosition;
			}
		}
		else
		{
			if (_buildingPreviews.Count == 0)
			{
				_buildingPreviews.Add(Instantiate(buildEntries[_selectedBuildingIndex].buildingPreview));
			}
			_buildingPreviews[0].transform.position = closestNode.WorldPosition;
		}
		
		if (Input.GetMouseButtonUp(0) && !closestNode.IsBlocked)
		{
			PlaceSelectedBuildingsAt(_selectedNodes);
			DestroyBuildingPreviews();
			_selectedNodes.Clear();
		}
	}

	private void PlaceSelectedBuildingsAt(List<GridNode> targetNodes)
	{
		foreach (var node in targetNodes)
		{
			var instance = _entityManager.Instantiate(buildEntries[_selectedBuildingIndex].BuildingEntity);
		
			_entityManager.SetComponentData<Translation>(instance, new Translation()
			{
				Value = node.WorldPosition
			});
		
			node.SetIsBlocked(true);
			_pathfindingRefreshSystem.RequestRefresh(node.WorldPosition);
		}
	}

	private void SelectBuilding(int buildingIdx)
	{
		DestroyBuildingPreviews();
		
		_selectedBuildingIndex = buildingIdx;
		var preview = Instantiate(buildEntries[_selectedBuildingIndex].buildingPreview);
		_buildingPreviews.Add(preview); 
	}

	private void DestroyBuildingPreviews()
	{
		for (int i = _buildingPreviews.Count - 1; i > 0; i--)
		{
			var toRemove = _buildingPreviews[i];
			_buildingPreviews.Remove(toRemove);
			Destroy(toRemove);
		}
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