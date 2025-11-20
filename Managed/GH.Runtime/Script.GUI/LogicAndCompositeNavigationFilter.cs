using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class LogicAndCompositeNavigationFilter : BaseNavigationFilter
{
	[SerializeField]
	private BaseNavigationFilter[] _navigationFilters;

	public override bool IsTrue(IUiNavigationElement navigationElement)
	{
		BaseNavigationFilter[] navigationFilters = _navigationFilters;
		for (int i = 0; i < navigationFilters.Length; i++)
		{
			if (!navigationFilters[i].IsTrue(navigationElement))
			{
				return false;
			}
		}
		return true;
	}
}
