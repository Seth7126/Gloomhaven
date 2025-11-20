using System;
using System.Text;

namespace XUnity.Common.Utilities;

public static class CabHelper
{
	private static string CreateRandomCab()
	{
		return "CAB-" + Guid.NewGuid().ToString("N");
	}

	public static void RandomizeCab(byte[] assetBundleData)
	{
		string text = Encoding.ASCII.GetString(assetBundleData, 0, Math.Min(1024, assetBundleData.Length - 4));
		int num = text.IndexOf("CAB-", StringComparison.Ordinal);
		if (num >= 0)
		{
			int num2 = text.Substring(num).IndexOf('\0');
			if (num2 >= 0 && num2 <= 36)
			{
				string s = CreateRandomCab();
				Buffer.BlockCopy(Encoding.ASCII.GetBytes(s), 36 - num2, assetBundleData, num, num2);
			}
		}
	}

	public static void RandomizeCabWithAnyLength(byte[] assetBundleData)
	{
		FindAndReplaceCab("CAB-", 0, assetBundleData, 2048);
	}

	private static void FindAndReplaceCab(string ansiStringToStartWith, byte byteToEndWith, byte[] data, int maxIterations = -1)
	{
		int num = Math.Min(data.Length, maxIterations);
		if (num == -1)
		{
			num = data.Length;
		}
		int num2 = 0;
		int length = ansiStringToStartWith.Length;
		string text = Guid.NewGuid().ToString("N");
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			char c = (char)data[i];
			if (num2 == length)
			{
				while (data[i] != byteToEndWith && i < num)
				{
					if (num3 >= text.Length)
					{
						num3 = 0;
						text = Guid.NewGuid().ToString("N");
					}
					data[i++] = (byte)text[num3++];
				}
				break;
			}
			num2 = ((c == ansiStringToStartWith[num2]) ? (num2 + 1) : 0);
		}
	}
}
