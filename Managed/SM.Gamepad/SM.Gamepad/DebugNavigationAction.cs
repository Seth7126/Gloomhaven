#define ENABLE_LOGS
using SM.Utils;
using UnityEngine;

namespace SM.Gamepad;

[CreateAssetMenu(menuName = "Data/Navigation Actions")]
public class DebugNavigationAction : NavigationAction
{
	public override void HandleAction(NavigationActionArgs args)
	{
		if (!(args.NavigationElement == null))
		{
			LogUtils.Log(args.NavigationElement.name);
		}
	}
}
