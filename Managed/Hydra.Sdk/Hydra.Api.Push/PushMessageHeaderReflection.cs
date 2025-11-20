using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public static class PushMessageHeaderReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushMessageHeaderReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChxQdXNoL1B1c2hNZXNzYWdlSGVhZGVyLnByb3RvEg5IeWRyYS5BcGkuUHVz" + "aBoUUHVzaC9QdXNoVG9rZW4ucHJvdG8aGlB1c2gvUHVzaE1lc3NhZ2VUeXBl" + "LnByb3RvIoUBChFQdXNoTWVzc2FnZUhlYWRlchIoCgV0b2tlbhgBIAEoCzIZ" + "Lkh5ZHJhLkFwaS5QdXNoLlB1c2hUb2tlbhI1CgxtZXNzYWdlX3R5cGUYAiAB" + "KA4yHy5IeWRyYS5BcGkuUHVzaC5QdXNoTWVzc2FnZVR5cGUSDwoHdmVyc2lv" + "bhgDIAEoBWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			PushTokenReflection.Descriptor,
			PushMessageTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PushMessageHeader), PushMessageHeader.Parser, new string[3] { "Token", "MessageType", "Version" }, null, null, null, null)
		}));
	}
}
