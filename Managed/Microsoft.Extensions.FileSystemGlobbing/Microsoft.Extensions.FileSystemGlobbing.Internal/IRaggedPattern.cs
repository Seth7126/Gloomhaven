using System.Collections.Generic;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal;

public interface IRaggedPattern : IPattern
{
	IList<IPathSegment> Segments { get; }

	IList<IPathSegment> StartsWith { get; }

	IList<IList<IPathSegment>> Contains { get; }

	IList<IPathSegment> EndsWith { get; }
}
