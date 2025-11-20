using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Push.Signaling;

namespace Hydra.Api.Push;

public static class PushClientMessageReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushClientMessageReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChxQdXNoL1B1c2hDbGllbnRNZXNzYWdlLnByb3RvEg5IeWRyYS5BcGkuUHVz" + "aBoeUHVzaC9TaWduYWxpbmcvU2lnbmFsaW5nLnByb3RvIpYBChFQdXNoQ2xp" + "ZW50TWVzc2FnZRI7CgxtZXNzYWdlX3R5cGUYASABKA4yJS5IeWRyYS5BcGku" + "UHVzaC5QdXNoQ2xpZW50TWVzc2FnZVR5cGUSPAoGc2lnbmFsGAIgASgLMiou" + "SHlkcmEuQXBpLlB1c2guU2lnbmFsaW5nLlNpZ25hbGluZ01lc3NhZ2VIAEIG" + "CgRraW5kKmUKFVB1c2hDbGllbnRNZXNzYWdlVHlwZRIkCiBQVVNIX0NMSUVO" + "VF9NRVNTQUdFX1RZUEVfVU5LTk9XThAAEiYKIlBVU0hfQ0xJRU5UX01FU1NB" + "R0VfVFlQRV9TSUdOQUxJTkcQAWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { SignalingReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[1] { typeof(PushClientMessageType) }, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PushClientMessage), PushClientMessage.Parser, new string[2] { "MessageType", "Signal" }, new string[1] { "Kind" }, null, null, null)
		}));
	}
}
