using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIMapFTUEAddTagButtonListener : UIMapFTUEListener
{
	[SerializeField]
	private string ftueTag;

	private Button button;

	protected override void OnStartedFTUE()
	{
		base.OnStartedFTUE();
		button = GetComponent<Button>();
		button.onClick.AddListener(AddTag);
	}

	protected override void Clear()
	{
		base.Clear();
		button?.onClick.RemoveListener(AddTag);
	}

	private void AddTag()
	{
		button.onClick.RemoveListener(AddTag);
		Singleton<MapFTUEManager>.Instance.AddTag(ftueTag);
	}
}
