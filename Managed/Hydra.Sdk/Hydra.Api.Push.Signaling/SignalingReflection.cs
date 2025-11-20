using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Signaling;

public static class SignalingReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static SignalingReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Ch5QdXNoL1NpZ25hbGluZy9TaWduYWxpbmcucHJvdG8SGEh5ZHJhLkFwaS5Q" + "dXNoLlNpZ25hbGluZyKVAQoQU2lnbmFsaW5nTWVzc2FnZRISCgptZXNzYWdl" + "X2lkGAEgASgJEjUKBHR5cGUYAiABKA4yJy5IeWRyYS5BcGkuUHVzaC5TaWdu" + "YWxpbmcuU2lnbmFsaW5nVHlwZRIUCgx1c2VyX2lkX2Zyb20YAyABKAkSEgoK" + "dXNlcl9pZF90bxgEIAEoCRIMCgRkYXRhGAUgASgMImYKF1NpZ25hbGluZ01l" + "c3NhZ2VWZXJzaW9uEg8KB3ZlcnNpb24YASABKAUSOgoGdXBkYXRlGAIgASgL" + "MiouSHlkcmEuQXBpLlB1c2guU2lnbmFsaW5nLlNpZ25hbGluZ01lc3NhZ2Uq" + "UQoNU2lnbmFsaW5nVHlwZRIhCh1TSUdOQUxJTkdfVFlQRV9TSUdOQUxfVU5L" + "Tk9XThAAEh0KGVNJR05BTElOR19UWVBFX1NJR05BTF9QMlAQAWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(SignalingType) }, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(SignalingMessage), SignalingMessage.Parser, new string[5] { "MessageId", "Type", "UserIdFrom", "UserIdTo", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SignalingMessageVersion), SignalingMessageVersion.Parser, new string[2] { "Version", "Update" }, null, null, null, null)
		}));
	}
}
