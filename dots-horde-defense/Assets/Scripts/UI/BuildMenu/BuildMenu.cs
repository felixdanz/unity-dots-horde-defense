using System.Collections.Generic;
using GenericPool;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
	[Header("Data")]
	[SerializeField] private List<BuildingData> buildingData;
	
	[Header("Components")] 
	[SerializeField] private Transform entryParent;
	[SerializeField] private BuildMenuEntry entryPrefab;

	private Entity[] dataToEntity;
	
	private int _selectedBuildingIndex;
	private GridNode _firstHit;
	private GridNode _lastHit;
	private List<GameObject> _pooledObjectsInUse;

	private EntityManager _entityManager;
	private BlobAssetStore _blobAssetStore;
	private PathfindingRefreshSystem _pathfindingRefreshSystem;
	
	
	private void Start()
	{
		foreach (var data in buildingData)
		{
			PoolManager.Instance.CreatePool(
				data.BuildingName,
				() => Instantiate(data.BuildingPreview),
				(toDestroy) => Destroy(toDestroy),
				(toSetActive, value) => toSetActive.SetActive(value),
				true,
				50,
				true,
				50);
		}
		
		_selectedBuildingIndex = -1;
		_pooledObjectsInUse = new List<GameObject>();

		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_blobAssetStore = new BlobAssetStore();

		_pathfindingRefreshSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PathfindingRefreshSystem>();
		
		CreateBuildingEntities();
	}
	
	private void Update()
	{
		BuildInput();
	}

	private void BuildInput()
	{
		if (_selectedBuildingIndex == -1)
			return;
		
		// disable
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ReturnBuildingPreviews();
			
			if (_firstHit != null)
				_selectedBuildingIndex = -1;

			_firstHit = null;
			return;
		}
		
		var raycastSuccess = CameraController.Instance.Raycast(
			Input.mousePosition,
			PhysicsUtilities.GetCollisionFilter(PhysicCategories.Ground), 
			out var raycastHit);
		
		if (!raycastSuccess)
			return;
		
		var closestNode = GridController.Instance.Grid.GetClosestNode(raycastHit.Position);
		var secondHit = Vector3.zero;
		
		// TODO(FD): restriction options, for now always line restricted
		if (_firstHit != null)
		{
			var firstPos = _firstHit.WorldPosition;
			var secondPos = closestNode.WorldPosition;

			var xDiff = Mathf.Abs(secondPos.x - firstPos.x);
			var zDiff = Mathf.Abs(secondPos.z - firstPos.z);
			
			secondHit = xDiff >= zDiff
				? new Vector3(secondPos.x, firstPos.y, firstPos.z)
				: new Vector3(firstPos.x, firstPos.y, secondPos.z);
		}
		
		// if no first node, set first node
		// if first node is set, place buildings
		if (Input.GetMouseButtonDown(0) && !closestNode.IsBlocked)
		{
			if (!buildingData[_selectedBuildingIndex].IsDraggable)
			{
				PlaceSelectedBuildingOn(closestNode);
			}
			
			if (_firstHit == null)
			{
				_firstHit = closestNode;
				return;
			}
			
			var nodesToBuildOn = GridController.Instance.Grid.GetNodesOnLineBetween(
				_firstHit,
				secondHit);
				
			PlaceSelectedBuildingOn(nodesToBuildOn);
			_firstHit = null;
			return;
		}

		if (_firstHit == null)
		{
			ReturnBuildingPreviews();
			
			PoolManager.Instance.GetObject<GameObject>(
				$"{buildingData[_selectedBuildingIndex].BuildingName}", 
				out var obj);
			
			_pooledObjectsInUse.Add(obj);
			obj.transform.position = closestNode.WorldPosition;
			return;
		}
		
		if (_lastHit == closestNode)
			return;

		_lastHit = closestNode;
		
		ReturnBuildingPreviews();
		
		var selectedNodes = GridController.Instance.Grid.GetNodesOnLineBetween(
			_firstHit,
			secondHit);
		
		// display preview
		foreach (var selectedNode in selectedNodes)
		{
			PoolManager.Instance.GetObject<GameObject>(
				$"{buildingData[_selectedBuildingIndex].BuildingName}", 
				out var obj);
			
			_pooledObjectsInUse.Add(obj);
			obj.transform.position = selectedNode.WorldPosition;
		}
	}

	private void CreateBuildingEntities()
	{
		dataToEntity = new Entity[buildingData.Count];
			
		var conversionSettings = GameObjectConversionSettings.FromWorld(
			World.DefaultGameObjectInjectionWorld, 
			_blobAssetStore);

		var index = 0;
		foreach (var buildingData in buildingData)
		{
			var convertedPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
				buildingData.BuildingPrefab, 
				conversionSettings);
			
			dataToEntity[index++] = convertedPrefab;
		}
	}

	private void PlaceSelectedBuildingOn(List<GridNode> targetNodes)
	{
		foreach (var node in targetNodes)
			PlaceSelectedBuildingOn(node);
	}

	private void PlaceSelectedBuildingOn(GridNode targetNode)
	{
		var instance = _entityManager.Instantiate(dataToEntity[_selectedBuildingIndex]);
		
		_entityManager.SetComponentData<Translation>(instance, new Translation()
		{
			Value = targetNode.WorldPosition
		});
		
		targetNode.SetIsBlocked(true);
		_pathfindingRefreshSystem.RequestRefresh(targetNode.WorldPosition);
	}

	private void SelectBuilding(int buildingIdx)
	{
		ReturnBuildingPreviews();
		_selectedBuildingIndex = buildingIdx;
	}

	private void ReturnBuildingPreviews()
	{
		foreach (var obj in _pooledObjectsInUse)
		{
			PoolManager.Instance.ReturnObject(
				$"{buildingData[_selectedBuildingIndex].BuildingName}", 
				obj);
		}
		
		_pooledObjectsInUse.Clear();
	}

	private void UpdateView()
	{
		foreach (Transform entry in entryParent)
			Destroy(entry.gameObject);

		var count = 0;
		
		foreach (var buildingData in buildingData)
		{
			var buildingIdx = count;
			var buildMenuEntry = Instantiate(entryPrefab, entryParent);
			buildMenuEntry.UpdateView(buildingData.Icon, buildingData.BuildingName);
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
