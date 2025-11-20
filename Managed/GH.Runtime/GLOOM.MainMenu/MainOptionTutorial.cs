using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionTutorial : MainOption
{
	[SerializeField]
	private UITutorialSelectorWindow tutorialWindow;

	protected override void Awake()
	{
		base.Awake();
		tutorialWindow.OnHidden.AddListener(base.Button.Deselect);
	}

	public override void Select()
	{
		if (!PlatformLayer.Instance.IsConsole)
		{
			Singleton<PromotionDLCManager>.Instance?.Hide();
		}
		base.Select();
		tutorialWindow.Show();
	}

	public override void Deselect()
	{
		if (!PlatformLayer.Instance.IsConsole)
		{
			Singleton<PromotionDLCManager>.Instance?.Show();
		}
		base.Deselect();
		tutorialWindow.Hide();
	}
}
