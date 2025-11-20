using System.Security.Cryptography;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class HashHelper
{
	private static readonly SHA1Managed SHA1 = new SHA1Managed();

	private static readonly uint[] Lookup32 = CreateLookup32();

	public static string Compute(byte[] data)
	{
		return ByteArrayToHexViaLookup32(SHA1.ComputeHash(data)).Substring(0, 10);
	}

	private static uint[] CreateLookup32()
	{
		uint[] array = new uint[256];
		for (int i = 0; i < 256; i++)
		{
			string text = i.ToString("X2");
			array[i] = text[0] + ((uint)text[1] << 16);
		}
		return array;
	}

	private static string ByteArrayToHexViaLookup32(byte[] bytes)
	{
		uint[] lookup = Lookup32;
		char[] array = new char[bytes.Length * 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			uint num = lookup[bytes[i]];
			array[2 * i] = (char)num;
			array[2 * i + 1] = (char)(num >> 16);
		}
		return new string(array);
	}
}
