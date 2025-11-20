using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.ServerManagerScheduler;

public static class BuildVersionReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static BuildVersionReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CilTZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyL0J1aWxkVmVyc2lvbi5wcm90bxIg" + "SHlkcmEuQXBpLlNlcnZlck1hbmFnZXJTY2hlZHVsZXIiMQoMQnVpbGRWZXJz" + "aW9uEhAKCHRpdGxlX2lkGAEgASgJEg8KB3ZlcnNpb24YAiABKAliBnByb3Rv" + "Mw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BuildVersion), BuildVersion.Parser, new string[2] { "TitleId", "Version" }, null, null, null, null)
		}));
	}
}
