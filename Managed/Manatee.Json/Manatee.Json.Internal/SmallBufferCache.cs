using System;

namespace Manatee.Json.Internal;

internal static class SmallBufferCache
{
	private const int _bufferSize = 8;

	private static readonly ObjectCache<char[]> _cache = new ObjectCache<char[]>(() => new char[8]);

	public static char[] Acquire(int size)
	{
		if (size > 8)
		{
			return new char[8];
		}
		return _cache.Acquire();
	}

	public static void Release(char[] buffer)
	{
		if (buffer != null && buffer.Length <= 8)
		{
			Array.Clear(buffer, 0, buffer.Length);
			_cache.Release(buffer);
		}
	}
}
