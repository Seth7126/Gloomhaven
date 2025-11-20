using System.Collections.Generic;

namespace FrameTimeLogger;

public struct Column<T>
{
	public string Name;

	public T[] Values;

	public Column(string name, T[] values)
	{
		Name = name;
		Values = values;
	}

	public Column(string name, List<T> values)
	{
		Name = name;
		Values = values.ToArray();
	}
}
