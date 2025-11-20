using System;
using System.Linq;

namespace Manatee.Json;

internal static class UriExtensions
{
	public static Uri GetParentUri(this Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (uri.IsAbsoluteUri && uri.Segments.Length == 1)
		{
			throw new InvalidOperationException("Cannot get parent of root");
		}
		return new Uri(uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length));
	}
}
