using System;

namespace SRDebugger;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class SortAttribute : Attribute
{
	public readonly int SortPriority;

	public SortAttribute(int priority)
	{
		SortPriority = priority;
	}
}
