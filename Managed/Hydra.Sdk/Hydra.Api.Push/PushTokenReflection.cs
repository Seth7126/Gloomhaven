using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public static class PushTokenReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushTokenReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChRQdXNoL1B1c2hUb2tlbi5wcm90bxIOSHlkcmEuQXBpLlB1c2giMwoJUHVz" + "aFRva2VuEhQKDGluc3RhbmNlX2tleRgBIAEoBRIQCgh1c2VyX2tleRgDIAEo" + "CWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PushToken), PushToken.Parser, new string[2] { "InstanceKey", "UserKey" }, null, null, null, null)
		}));
	}
}
