using System;
using System.IO;
using System.Text;

namespace Apparance.Net;

public static class Helpers
{
	public static string ReadString(BinaryReader data)
	{
		int count = data.ReadInt32();
		byte[] bytes = data.ReadBytes(count);
		return Encoding.UTF8.GetString(bytes);
	}

	public static char cTypeIDFromObjectType(object value)
	{
		if (value != null)
		{
			Type type = value.GetType();
			if (type == typeof(int))
			{
				return 'i';
			}
			if (type == typeof(float))
			{
				return 'f';
			}
			if (type == typeof(bool))
			{
				return 'b';
			}
			if (type == typeof(Colour))
			{
				return 'C';
			}
			if (type == typeof(string))
			{
				return '$';
			}
			if (type == typeof(Vector3))
			{
				return '3';
			}
			if (type == typeof(Frame))
			{
				return 'F';
			}
			if (type == typeof(ParameterCollection))
			{
				return '[';
			}
		}
		return '\0';
	}
}
