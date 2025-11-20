using System;
using System.Diagnostics;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Sdk.Extensions;

public static class DescriptorExtensions
{
	public static T Extract<T>(this MessageDescriptor descriptor, int field, FieldCodec<T> codec)
	{
		try
		{
			if (descriptor == null)
			{
				return default(T);
			}
			MessageOptions options = descriptor.GetOptions();
			Extension<MessageOptions, T> extension = new Extension<MessageOptions, T>(field, codec);
			if (options.HasExtension(extension))
			{
				return options.GetExtension(extension);
			}
		}
		catch
		{
		}
		return default(T);
	}

	public static T ExtractDescriptor<T>(this Type type)
	{
		return (T)(type.GetProperty("Descriptor", BindingFlags.Static | BindingFlags.Public)?.GetValue(null, null));
	}

	public static string ExtractHash(this Type type)
	{
		try
		{
			return FileVersionInfo.GetVersionInfo(type.Assembly.Location).ProductVersion;
		}
		catch
		{
			return null;
		}
	}
}
