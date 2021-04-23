using Unity.Entities;
using UnityEngine;

public class GridController : MonoBehaviour
{
	public static GridController Instance { get; private set; }

	public Grid Grid { get; private set; }
	
	[Header("Grid Configuration")] 
	[SerializeField] private int width;
	[SerializeField] private int height;
	[SerializeField] private float cellSize;
	[SerializeField] private Transform origin;
	
	private EntityManager _entityManager;
	private Entity _supportEntity;


	private void Awake()
	{
		#region Singleton

		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		#endregion
	}
	
	private void Start()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_supportEntity = _entityManager.CreateEntity();

		RequestNewGrid();
	}

	public void RequestNewGrid()
	{
		_entityManager.AddComponentObject(_supportEntity, this);
		_entityManager.AddComponentData<Tag_NeedsInitialization>(
			_supportEntity, 
			new Tag_NeedsInitialization());
	}

	public void InitializeGrid()
	{
		Grid = new Grid(width, height, cellSize, origin.position);
	}

	private void OnDrawGizmosSelected()
	{
		if (Grid == null)
			return;

		foreach (var gridNode in Grid.GetNodes())
		{
			Gizmos.color = gridNode.IsBlocked
				? Color.red
				: Color.green;
			
			Gizmos.DrawCube(gridNode.WorldPosition, Vector3.one * 0.5f);
		}
	}
}
