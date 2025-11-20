namespace Microsoft.Extensions.FileSystemGlobbing.Internal;

public interface IPathSegment
{
	bool CanProduceStem { get; }

	bool Match(string value);
}
