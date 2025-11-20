using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Nullable;

public static class NullableReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static NullableReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChdOdWxsYWJsZS9OdWxsYWJsZS5wcm90bxISSHlkcmEuQXBpLk51bGxhYmxl" + "Ghxnb29nbGUvcHJvdG9idWYvc3RydWN0LnByb3RvIlQKDk51bGxhYmxlU3Ry" + "aW5nEioKBG51bGwYASABKA4yGi5nb29nbGUucHJvdG9idWYuTnVsbFZhbHVl" + "SAASDgoEZGF0YRgCIAEoCUgAQgYKBGtpbmQiUQoLTnVsbGFibGVJbnQSKgoE" + "bnVsbBgBIAEoDjIaLmdvb2dsZS5wcm90b2J1Zi5OdWxsVmFsdWVIABIOCgRk" + "YXRhGAIgASgFSABCBgoEa2luZCJSCgxOdWxsYWJsZUJvb2wSKgoEbnVsbBgB" + "IAEoDjIaLmdvb2dsZS5wcm90b2J1Zi5OdWxsVmFsdWVIABIOCgRkYXRhGAIg" + "ASgISABCBgoEa2luZGIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { StructReflection.Descriptor }, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[3]
		{
			new GeneratedClrTypeInfo(typeof(NullableString), NullableString.Parser, new string[2] { "Null", "Data" }, new string[1] { "Kind" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableInt), NullableInt.Parser, new string[2] { "Null", "Data" }, new string[1] { "Kind" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableBool), NullableBool.Parser, new string[2] { "Null", "Data" }, new string[1] { "Kind" }, null, null, null)
		}));
	}
}
