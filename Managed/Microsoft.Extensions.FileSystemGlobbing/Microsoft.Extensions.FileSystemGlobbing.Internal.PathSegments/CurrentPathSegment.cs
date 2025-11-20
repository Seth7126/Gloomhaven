namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

public class CurrentPathSegment : IPathSegment
{
	public bool CanProduceStem => false;

	public bool Match(string value)
	{
		return false;
	}
}
