using System;

namespace XUnity.Common.Utilities;

public class HookingHelperPriorityAttribute : Attribute
{
	public int priority;

	public HookingHelperPriorityAttribute(int priority)
	{
		this.priority = priority;
	}
}
