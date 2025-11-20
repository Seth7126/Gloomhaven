using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;

public class PatternBuilder
{
	private sealed class LinearPattern : ILinearPattern, IPattern
	{
		public IList<IPathSegment> Segments { get; }

		public LinearPattern(List<IPathSegment> allSegments)
		{
			Segments = allSegments;
		}

		public IPatternContext CreatePatternContextForInclude()
		{
			return new PatternContextLinearInclude(this);
		}

		public IPatternContext CreatePatternContextForExclude()
		{
			return new PatternContextLinearExclude(this);
		}
	}

	private sealed class RaggedPattern : IRaggedPattern, IPattern
	{
		public IList<IList<IPathSegment>> Contains { get; }

		public IList<IPathSegment> EndsWith { get; }

		public IList<IPathSegment> Segments { get; }

		public IList<IPathSegment> StartsWith { get; }

		public RaggedPattern(List<IPathSegment> allSegments, IList<IPathSegment> segmentsPatternStartsWith, IList<IPathSegment> segmentsPatternEndsWith, IList<IList<IPathSegment>> segmentsPatternContains)
		{
			Segments = allSegments;
			StartsWith = segmentsPatternStartsWith;
			Contains = segmentsPatternContains;
			EndsWith = segmentsPatternEndsWith;
		}

		public IPatternContext CreatePatternContextForInclude()
		{
			return new PatternContextRaggedInclude(this);
		}

		public IPatternContext CreatePatternContextForExclude()
		{
			return new PatternContextRaggedExclude(this);
		}
	}

	private static readonly char[] _slashes = new char[2] { '/', '\\' };

	private static readonly char[] _star = new char[1] { '*' };

	public StringComparison ComparisonType { get; }

	public PatternBuilder()
	{
		ComparisonType = StringComparison.OrdinalIgnoreCase;
	}

	public PatternBuilder(StringComparison comparisonType)
	{
		ComparisonType = comparisonType;
	}

	public IPattern Build(string pattern)
	{
		if (pattern == null)
		{
			throw new ArgumentNullException("pattern");
		}
		pattern = pattern.TrimStart(_slashes);
		if (pattern.TrimEnd(_slashes).Length < pattern.Length)
		{
			pattern = pattern.TrimEnd(_slashes) + "/**";
		}
		List<IPathSegment> list = new List<IPathSegment>();
		bool flag = true;
		IList<IPathSegment> list2 = null;
		IList<IList<IPathSegment>> list3 = null;
		IList<IPathSegment> list4 = null;
		int length = pattern.Length;
		int num = 0;
		while (num < length)
		{
			int num2 = num;
			int num3 = NextIndex(pattern, _slashes, num, length);
			IPathSegment pathSegment = null;
			if (pathSegment == null && num3 - num2 == 3 && pattern[num2] == '*' && pattern[num2 + 1] == '.' && pattern[num2 + 2] == '*')
			{
				num2 += 2;
			}
			if (pathSegment == null && num3 - num2 == 2)
			{
				if (pattern[num2] == '*' && pattern[num2 + 1] == '*')
				{
					pathSegment = new RecursiveWildcardSegment();
				}
				else if (pattern[num2] == '.' && pattern[num2 + 1] == '.')
				{
					if (!flag)
					{
						throw new ArgumentException("\"..\" can be only added at the beginning of the pattern.");
					}
					pathSegment = new ParentPathSegment();
				}
			}
			if (pathSegment == null && num3 - num2 == 1 && pattern[num2] == '.')
			{
				pathSegment = new CurrentPathSegment();
			}
			if (pathSegment == null && num3 - num2 > 2 && pattern[num2] == '*' && pattern[num2 + 1] == '*' && pattern[num2 + 2] == '.')
			{
				pathSegment = new RecursiveWildcardSegment();
				num3 = num2;
			}
			if (pathSegment == null)
			{
				string beginsWith = string.Empty;
				List<string> list5 = new List<string>();
				string endsWith = string.Empty;
				int num4 = num2;
				while (num4 < num3)
				{
					int num5 = num4;
					int num6 = NextIndex(pattern, _star, num4, num3);
					if (num5 == num2)
					{
						if (num6 == num3)
						{
							pathSegment = new LiteralPathSegment(Portion(pattern, num5, num6), ComparisonType);
						}
						else
						{
							beginsWith = Portion(pattern, num5, num6);
						}
					}
					else if (num6 == num3)
					{
						endsWith = Portion(pattern, num5, num6);
					}
					else if (num5 != num6)
					{
						list5.Add(Portion(pattern, num5, num6));
					}
					num4 = num6 + 1;
				}
				if (pathSegment == null)
				{
					pathSegment = new WildcardPathSegment(beginsWith, list5, endsWith, ComparisonType);
				}
			}
			if (!(pathSegment is ParentPathSegment))
			{
				flag = false;
			}
			if (!(pathSegment is CurrentPathSegment))
			{
				if (pathSegment is RecursiveWildcardSegment)
				{
					if (list2 == null)
					{
						list2 = new List<IPathSegment>(list);
						list4 = new List<IPathSegment>();
						list3 = new List<IList<IPathSegment>>();
					}
					else if (list4.Count != 0)
					{
						list3.Add(list4);
						list4 = new List<IPathSegment>();
					}
				}
				else
				{
					list4?.Add(pathSegment);
				}
				list.Add(pathSegment);
			}
			num = num3 + 1;
		}
		if (list2 == null)
		{
			return new LinearPattern(list);
		}
		return new RaggedPattern(list, list2, list4, list3);
	}

	private static int NextIndex(string pattern, char[] anyOf, int beginIndex, int endIndex)
	{
		int num = pattern.IndexOfAny(anyOf, beginIndex, endIndex - beginIndex);
		if (num != -1)
		{
			return num;
		}
		return endIndex;
	}

	private static string Portion(string pattern, int beginIndex, int endIndex)
	{
		return pattern.Substring(beginIndex, endIndex - beginIndex);
	}
}
