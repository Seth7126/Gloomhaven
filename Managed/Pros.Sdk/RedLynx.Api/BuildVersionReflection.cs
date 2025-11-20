using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api;

public static class BuildVersionReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static BuildVersionReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch5yZWRseW54LWFwaS9CdWlsZFZlcnNpb24ucHJvdG8SC1JlZEx5bnguQXBp" + "GiBnb29nbGUvcHJvdG9idWYvZGVzY3JpcHRvci5wcm90byIgChBCdWlsZElu" + "Zm9ybWF0aW9uOgyKtRgCCgCKtRgCEgAiJgoFQnVpbGQSDwoHdmVyc2lvbhgB" + "IAEoCRIMCgRoYXNoGAIgASgJOkQKBWJ1aWxkEh8uZ29vZ2xlLnByb3RvYnVm" + "Lk1lc3NhZ2VPcHRpb25zGNGGAyABKAsyEi5SZWRMeW54LkFwaS5CdWlsZGIG" + "cHJvdG8z"), new FileDescriptor[1] { DescriptorReflection.Descriptor }, new GeneratedClrTypeInfo(null, new Extension[1] { BuildVersionExtensions.Build }, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(BuildInformation), BuildInformation.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(Build), Build.Parser, new string[2] { "Version", "Hash" }, null, null, null, null)
		}));
	}
}
