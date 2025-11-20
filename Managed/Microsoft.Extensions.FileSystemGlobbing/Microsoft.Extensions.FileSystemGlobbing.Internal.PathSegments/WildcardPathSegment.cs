using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

public class WildcardPathSegment : IPathSegment
{
	public static readonly WildcardPathSegment MatchAll = new WildcardPathSegment(string.Empty, new List<string>(), string.Empty, StringComparison.OrdinalIgnoreCase);

	private readonly StringComparison _comparisonType;

	public bool CanProduceStem => true;

	public string BeginsWith { get; }

	public List<string> Contains { get; }

	public string EndsWith { get; }

	public WildcardPathSegment(string beginsWith, List<string> contains, string endsWith, StringComparison comparisonType)
	{
		BeginsWith = beginsWith;
		Contains = contains;
		EndsWith = endsWith;
		_comparisonType = comparisonType;
	}

	public bool Match(string value)
	{
		if (value.Length < BeginsWith.Length + EndsWith.Length)
		{
			return false;
		}
		if (!value.StartsWith(BeginsWith, _comparisonType))
		{
			return false;
		}
		if (!value.EndsWith(EndsWith, _comparisonType))
		{
			return false;
		}
		int num = BeginsWith.Length;
		int num2 = value.Length - EndsWith.Length;
		for (int i = 0; i != Contains.Count; i++)
		{
			string text = Contains[i];
			int num3 = value.IndexOf(text, num, num2 - num, _comparisonType);
			if (num3 == -1)
			{
				return false;
			}
			num = num3 + text.Length;
		}
		return true;
	}
}
