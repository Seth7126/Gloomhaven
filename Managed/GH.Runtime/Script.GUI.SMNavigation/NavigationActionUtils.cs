using System.Collections.Generic;
using SM.Gamepad;
using Script.LogicalOperations;
using UnityEngine;

namespace Script.GUI.SMNavigation;

public static class NavigationActionUtils
{
	private static readonly ICondition TrueCondition = new TrueCondition();

	public static ICondition GetConditionOrTrue(this NavigationAction.NavigationActionArgs args)
	{
		return args.GetComponent<ICondition>() ?? TrueCondition;
	}

	public static T GetComponent<T>(this NavigationAction.NavigationActionArgs args)
	{
		return args.Components.FirstOrDefaultOfType<T>();
	}

	public static T FirstOrDefaultOfType<T>(this IEnumerable<MonoBehaviour> components)
	{
		foreach (MonoBehaviour component in components)
		{
			if (component is T)
			{
				return (T)(object)((component is T) ? component : null);
			}
		}
		return default(T);
	}
}
