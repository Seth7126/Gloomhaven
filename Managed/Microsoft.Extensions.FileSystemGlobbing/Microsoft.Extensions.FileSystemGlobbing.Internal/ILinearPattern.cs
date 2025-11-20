using System.Collections.Generic;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal;

public interface ILinearPattern : IPattern
{
	IList<IPathSegment> Segments { get; }
}
