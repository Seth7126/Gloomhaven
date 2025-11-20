using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation;

[CreateAssetMenu(menuName = "Data/DisplayPanelHotkeyNavAction")]
public class DisplayPanelHotkeyAction : SessionNavAction
{
	[SerializeField]
	private string _hotkey;

	protected override bool TryInitSession(NavigationActionArgs args, out ISession session)
	{
		if (!args.NavigationElement.TryGetComponentInParents<HotkeyContainerWrapper>(out var component))
		{
			session = null;
			return false;
		}
		session = new DisplayPanelHotkeySession(_hotkey, component.HotkeyContainer, args.GetConditionOrTrue());
		return true;
	}
}
