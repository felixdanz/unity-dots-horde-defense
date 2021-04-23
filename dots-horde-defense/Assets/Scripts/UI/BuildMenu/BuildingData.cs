using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Data / Building Data")]
public class BuildingData : ScriptableObject
{
	[SerializeField] private string buildingName;
	[SerializeField] private Sprite icon;
	[SerializeField] private GameObject buildingPreview;
	[SerializeField] private GameObject buildingPrefab;
	[SerializeField] private bool isDraggable;
	
	public string BuildingName => buildingName;
	public Sprite Icon => icon;
	public GameObject BuildingPreview => buildingPreview;
	public GameObject BuildingPrefab => buildingPrefab;
	public bool IsDraggable => isDraggable;
}