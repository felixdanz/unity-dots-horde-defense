using UnityEngine;
using UnityEngine.UI;

public class BuildMenuEntry : MonoBehaviour
{
	[SerializeField] private Button buttonRef;
	[SerializeField] private Image iconRef;
	[SerializeField] private Text nameRef;

	
	public Button Button => buttonRef;
	
	
	public void UpdateView(Sprite icon, string entryName)
	{
		iconRef.sprite = icon;
		nameRef.text = entryName;
	}
}