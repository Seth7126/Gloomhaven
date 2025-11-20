using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.FileSystemGlobbing;

public class PatternMatchingResult
{
	public IEnumerable<FilePatternMatch> Files { get; set; }

	public bool HasMatches { get; }

	public PatternMatchingResult(IEnumerable<FilePatternMatch> files)
		: this(files, files.Any())
	{
		Files = files;
	}

	public PatternMatchingResult(IEnumerable<FilePatternMatch> files, bool hasMatches)
	{
		Files = files;
		HasMatches = hasMatches;
	}
}
