using System;

namespace Apparance.Net;

public class Parameter
{
	public int ID { get; internal set; }

	public string Name { get; internal set; }

	public char Type { get; internal set; }

	public object Value { get; internal set; }

	public void Set(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("Can't set parameter to null");
		}
		char c = Helpers.cTypeIDFromObjectType(value);
		if (c == '\0')
		{
			throw new ArgumentException("Can't set parameter of unknown type " + value.GetType().Name);
		}
		if (Type == '\0')
		{
			Type = c;
		}
		else if (Type != c)
		{
			throw new ArithmeticException("Can't change parameter type to " + value.GetType().Name + " (expecting '" + Type + "')");
		}
		Value = value;
	}
}
