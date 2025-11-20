using System;
using SRF.Helpers;

namespace SRDebugger;

public sealed class OptionDefinition
{
	public string Name { get; private set; }

	public string Category { get; private set; }

	public int SortPriority { get; private set; }

	public bool IsMethod => Method != null;

	public bool IsProperty => Property != null;

	public MethodReference Method { get; private set; }

	public PropertyReference Property { get; private set; }

	private OptionDefinition(string name, string category, int sortPriority)
	{
		Name = name;
		Category = category;
		SortPriority = sortPriority;
	}

	public OptionDefinition(string name, string category, int sortPriority, MethodReference method)
		: this(name, category, sortPriority)
	{
		Method = method;
	}

	public OptionDefinition(string name, string category, int sortPriority, PropertyReference property)
		: this(name, category, sortPriority)
	{
		Property = property;
	}

	public static OptionDefinition FromMethod(string name, Action callback, string category = "Default", int sortPriority = 0)
	{
		return new OptionDefinition(name, category, sortPriority, callback);
	}

	public static OptionDefinition Create<T>(string name, Func<T> getter, Action<T> setter = null, string category = "Default", int sortPriority = 0)
	{
		return new OptionDefinition(name, category, sortPriority, PropertyReference.FromLambda(getter, setter));
	}
}
