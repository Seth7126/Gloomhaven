using System;

namespace DynamicLinq;

internal class DynamicProperty
{
	private string name;

	private Type type;

	public string Name => name;

	public Type Type => type;

	public DynamicProperty(string name, Type type)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		this.name = name;
		this.type = type;
	}
}
