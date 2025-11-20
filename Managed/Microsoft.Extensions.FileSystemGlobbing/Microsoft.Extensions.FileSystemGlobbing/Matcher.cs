using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;

namespace Microsoft.Extensions.FileSystemGlobbing;

public class Matcher
{
	private readonly IList<IPattern> _includePatterns = new List<IPattern>();

	private readonly IList<IPattern> _excludePatterns = new List<IPattern>();

	private readonly PatternBuilder _builder;

	private readonly StringComparison _comparison;

	public Matcher()
		: this(StringComparison.OrdinalIgnoreCase)
	{
	}

	public Matcher(StringComparison comparisonType)
	{
		_comparison = comparisonType;
		_builder = new PatternBuilder(comparisonType);
	}

	public virtual Matcher AddInclude(string pattern)
	{
		_includePatterns.Add(_builder.Build(pattern));
		return this;
	}

	public virtual Matcher AddExclude(string pattern)
	{
		_excludePatterns.Add(_builder.Build(pattern));
		return this;
	}

	public virtual PatternMatchingResult Execute(DirectoryInfoBase directoryInfo)
	{
		MatcherContext matcherContext = new MatcherContext(_includePatterns, _excludePatterns, directoryInfo, _comparison);
		return matcherContext.Execute();
	}
}
