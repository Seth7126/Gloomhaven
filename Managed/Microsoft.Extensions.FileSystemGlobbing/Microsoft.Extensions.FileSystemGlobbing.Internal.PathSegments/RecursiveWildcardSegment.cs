namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

public class RecursiveWildcardSegment : IPathSegment
{
	public bool CanProduceStem => true;

	public bool Match(string value)
	{
		return false;
	}
}
