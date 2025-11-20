namespace AsmodeeNet.Utils;

public static class Format
{
	public static string HexStringFromString(string str)
	{
		return HexStringFromUInt64(ulong.Parse(str));
	}

	public static string HexStringFromUInt64(ulong i)
	{
		return $"{i:X}".ToLower();
	}
}
