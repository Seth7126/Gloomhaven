using System;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

public class ParentPathSegment : IPathSegment
{
	private const string LiteralParent = "..";

	public bool CanProduceStem => false;

	public bool Match(string value)
	{
		return string.Equals("..", value, StringComparison.Ordinal);
	}
}
