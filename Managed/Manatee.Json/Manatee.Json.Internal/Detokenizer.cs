using System.Text.RegularExpressions;

namespace Manatee.Json.Internal;

internal static class Detokenizer
{
	private static readonly Regex _tokenPattern = new Regex("\\{\\{(?<key>[a-z,0-9]*)(?<format>:.*?)?\\}\\}", RegexOptions.IgnoreCase);

	public static string ResolveTokens(this string template, JsonObject settings)
	{
		foreach (Match item in _tokenPattern.Matches(template))
		{
			string value = item.Groups["key"].Value;
			string text = item.Groups["format"]?.Value;
			if (settings.TryGetValue(value, out JsonValue value2))
			{
				string newValue = string.Format((text == null) ? "{0}" : ("{0:" + text + "}"), value2);
				template = template.Replace(item.Value, newValue);
			}
		}
		return template;
	}
}
