using System.Runtime.CompilerServices;

namespace System.IO;

internal static class PathInternal
{
	private static readonly bool s_isCaseSensitive = GetIsCaseSensitive();

	internal const string ExtendedDevicePathPrefix = "\\\\?\\";

	internal const string UncPathPrefix = "\\\\";

	internal const string UncDevicePrefixToInsert = "?\\UNC\\";

	internal const string UncExtendedPathPrefix = "\\\\?\\UNC\\";

	internal const string DevicePathPrefix = "\\\\.\\";

	internal const int MaxShortPath = 260;

	internal const int DevicePrefixLength = 4;

	internal static StringComparison StringComparison
	{
		get
		{
			if (!s_isCaseSensitive)
			{
				return StringComparison.OrdinalIgnoreCase;
			}
			return StringComparison.Ordinal;
		}
	}

	internal static bool IsCaseSensitive => s_isCaseSensitive;

	private static bool GetIsCaseSensitive()
	{
		try
		{
			string text = Path.Combine(Path.GetTempPath(), "CASESENSITIVETEST" + Guid.NewGuid().ToString("N"));
			using (new FileStream(text, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose))
			{
				return !File.Exists(text.ToLowerInvariant());
			}
		}
		catch (Exception)
		{
			return false;
		}
	}

	internal static bool IsValidDriveChar(char value)
	{
		if (value < 'A' || value > 'Z')
		{
			if (value >= 'a')
			{
				return value <= 'z';
			}
			return false;
		}
		return true;
	}

	private static bool EndsWithPeriodOrSpace(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		char c = path[path.Length - 1];
		if (c != ' ')
		{
			return c == '.';
		}
		return true;
	}

	internal static string EnsureExtendedPrefixIfNeeded(string path)
	{
		if (path != null && (path.Length >= 260 || EndsWithPeriodOrSpace(path)))
		{
			return EnsureExtendedPrefix(path);
		}
		return path;
	}

	internal static string EnsureExtendedPrefix(string path)
	{
		if (IsPartiallyQualified(path) || IsDevice(path))
		{
			return path;
		}
		if (path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
		{
			return path.Insert(2, "?\\UNC\\");
		}
		return "\\\\?\\" + path;
	}

	internal static bool IsDevice(string path)
	{
		if (!IsExtended(path))
		{
			if (path.Length >= 4 && IsDirectorySeparator(path[0]) && IsDirectorySeparator(path[1]) && (path[2] == '.' || path[2] == '?'))
			{
				return IsDirectorySeparator(path[3]);
			}
			return false;
		}
		return true;
	}

	internal static bool IsExtended(string path)
	{
		if (path.Length >= 4 && path[0] == '\\' && (path[1] == '\\' || path[1] == '?') && path[2] == '?')
		{
			return path[3] == '\\';
		}
		return false;
	}

	internal static int GetRootLength(ReadOnlySpan<char> path)
	{
		int i = 0;
		int num = 2;
		int num2 = 2;
		bool num3 = path.StartsWith("\\\\?\\");
		bool flag = path.StartsWith("\\\\?\\UNC\\");
		if (num3)
		{
			if (flag)
			{
				num2 = "\\\\?\\UNC\\".Length;
			}
			else
			{
				num += "\\\\?\\".Length;
			}
		}
		if ((!num3 || flag) && path.Length > 0 && IsDirectorySeparator(path[0]))
		{
			i = 1;
			if (flag || (path.Length > 1 && IsDirectorySeparator(path[1])))
			{
				i = num2;
				int num4 = 2;
				for (; i < path.Length; i++)
				{
					if (IsDirectorySeparator(path[i]) && --num4 <= 0)
					{
						break;
					}
				}
			}
		}
		else if (path.Length >= num && path[num - 1] == Path.VolumeSeparatorChar)
		{
			i = num;
			if (path.Length >= num + 1 && IsDirectorySeparator(path[num]))
			{
				i++;
			}
		}
		return i;
	}

	internal static bool IsPartiallyQualified(string path)
	{
		if (path.Length < 2)
		{
			return true;
		}
		if (IsDirectorySeparator(path[0]))
		{
			if (path[1] != '?')
			{
				return !IsDirectorySeparator(path[1]);
			}
			return false;
		}
		if (path.Length >= 3 && path[1] == Path.VolumeSeparatorChar && IsDirectorySeparator(path[2]))
		{
			return !IsValidDriveChar(path[0]);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsDirectorySeparator(char c)
	{
		if (c != Path.DirectorySeparatorChar)
		{
			return c == Path.AltDirectorySeparatorChar;
		}
		return true;
	}
}
