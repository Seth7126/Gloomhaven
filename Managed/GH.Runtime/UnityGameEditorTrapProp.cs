using UnityEngine;

public class UnityGameEditorTrapProp : MonoBehaviour, IHoverable
{
	private void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Hovering");
	}

	public void OnCursorEnter()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<UIPropInfoPanel>.Instance?.ShowTrap(base.gameObject);
		}
	}

	public void OnCursorExit()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<UIPropInfoPanel>.Instance?.Hide();
		}
	}
}
