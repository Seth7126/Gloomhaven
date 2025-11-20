using System;

namespace DynamicLinq;

internal sealed class ParseException : Exception
{
	private int position;

	public int Position => position;

	public ParseException(string message, int position)
		: base(message)
	{
		this.position = position;
	}

	public override string ToString()
	{
		return $"{Message} (at index {position})";
	}
}
