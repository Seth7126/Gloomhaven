using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Manatee.Json.Path.Parsing;

internal class IndexedArrayParser : IJsonPathParser
{
	public bool Handles(string input, int index)
	{
		if (index + 1 >= input.Length)
		{
			return false;
		}
		if (input[index] == '[')
		{
			if (!char.IsDigit(input[index + 1]) && input[index + 1] != '-')
			{
				return input[index + 1] == ':';
			}
			return true;
		}
		return false;
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] ref JsonPath? path, [NotNullWhen(false)] out string? errorMessage)
	{
		if (path == null)
		{
			errorMessage = "Start token not found.";
			return false;
		}
		if (!source.TryGetSlices(ref index, out IList<Slice> slices, out errorMessage))
		{
			return false;
		}
		path = path.Array(slices.ToArray());
		return true;
	}
}
