using System;
using System.Numerics.Hashing;

namespace Microsoft.Extensions.FileSystemGlobbing;

public struct FilePatternMatch : IEquatable<FilePatternMatch>
{
	public string Path { get; }

	public string Stem { get; }

	public FilePatternMatch(string path, string stem)
	{
		Path = path;
		Stem = stem;
	}

	public bool Equals(FilePatternMatch other)
	{
		if (string.Equals(other.Path, Path, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(other.Stem, Stem, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals((FilePatternMatch)obj);
	}

	public override int GetHashCode()
	{
		return System.Numerics.Hashing.HashHelpers.Combine(GetHashCode(Path), GetHashCode(Stem));
	}

	private static int GetHashCode(string value)
	{
		if (value == null)
		{
			return 0;
		}
		return StringComparer.OrdinalIgnoreCase.GetHashCode(value);
	}
}
