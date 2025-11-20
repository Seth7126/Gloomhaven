using System.Text;

namespace Manatee.Json.Internal;

internal static class StringBuilderCache
{
	private static readonly ObjectCache<StringBuilder> _cache = new ObjectCache<StringBuilder>(() => new StringBuilder());

	public static StringBuilder Acquire()
	{
		return _cache.Acquire();
	}

	public static void Release(StringBuilder sb)
	{
		if (sb.Capacity < 360)
		{
			sb.Clear();
			_cache.Release(sb);
		}
	}

	public static string GetStringAndRelease(StringBuilder sb)
	{
		string result = sb.ToString();
		Release(sb);
		return result;
	}
}
