using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Manatee.Trello;

public class WebColor
{
	private static readonly Regex Pattern = new Regex("^#(?<Red>[0-9a-fA-F]{2})(?<Green>[0-9a-fA-F]{2})(?<Blue>[0-9a-fA-F]{2})$");

	public ushort Red { get; }

	public ushort Green { get; }

	public ushort Blue { get; }

	public WebColor(ushort red, ushort green, ushort blue)
	{
		Red = red;
		Green = green;
		Blue = blue;
	}

	public WebColor(string serialized)
	{
		MatchCollection matchCollection = Pattern.Matches(serialized);
		if (matchCollection.Count == 0)
		{
			throw new ArgumentException("'" + serialized + "' is not a valid web color", "serialized");
		}
		Red = ushort.Parse(matchCollection[0].Groups["Red"].Value, NumberStyles.HexNumber);
		Green = ushort.Parse(matchCollection[0].Groups["Green"].Value, NumberStyles.HexNumber);
		Blue = ushort.Parse(matchCollection[0].Groups["Blue"].Value, NumberStyles.HexNumber);
	}

	public override string ToString()
	{
		return $"#{Red:X2}{Green:X2}{Blue:X2}";
	}
}
