using System;
using System.IO;

namespace XUnity.Common.Extensions;

public static class StreamExtensions
{
	public static byte[] ReadFully(this Stream stream, int initialLength)
	{
		if (initialLength < 1)
		{
			initialLength = 32768;
		}
		byte[] array = new byte[initialLength];
		int num = 0;
		int num2;
		while ((num2 = stream.Read(array, num, array.Length - num)) > 0)
		{
			num += num2;
			if (num == array.Length)
			{
				int num3 = stream.ReadByte();
				if (num3 == -1)
				{
					return array;
				}
				byte[] array2 = new byte[array.Length * 2];
				Array.Copy(array, array2, array.Length);
				array2[num] = (byte)num3;
				array = array2;
				num++;
			}
		}
		byte[] array3 = new byte[num];
		Array.Copy(array, array3, num);
		return array3;
	}
}
