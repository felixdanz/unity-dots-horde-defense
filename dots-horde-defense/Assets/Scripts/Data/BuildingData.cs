using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Data / Building Data")]
public class BuildingData : ScriptableObject
{
	[SerializeField] private string name;
	[SerializeField] private Sprite icon;


	public string Name => name;
	public Sprite Icon => icon;
}