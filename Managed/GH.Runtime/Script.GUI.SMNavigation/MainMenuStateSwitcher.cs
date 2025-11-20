using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation;

public class MainMenuStateSwitcher : UIStateSwitcher<MainStateTag, UIWindow>
{
	protected override void OnHide()
	{
		if (Singleton<ESCMenu>.Instance == null)
		{
			base.OnHide();
		}
	}
}
