using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public static class ServerContextReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ServerContextReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChtDb250ZXh0L1NlcnZlckNvbnRleHQucHJvdG8SIEh5ZHJhLkFwaS5JbmZy" + "YXN0cnVjdHVyZS5Db250ZXh0ImUKDVNlcnZlckNvbnRleHQSQQoEZGF0YRgB" + "IAEoCzIzLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlNlcnZl" + "ckNvbnRleHREYXRhEhEKCXNpZ25hdHVyZRgCIAEoDCJAChFTZXJ2ZXJDb250" + "ZXh0RGF0YRIQCgh0aXRsZV9pZBgBIAEoCRIZChFrZXJuZWxfc2Vzc2lvbl9p" + "ZBgCIAEoCWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(ServerContext), ServerContext.Parser, new string[2] { "Data", "Signature" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerContextData), ServerContextData.Parser, new string[2] { "TitleId", "KernelSessionId" }, null, null, null, null)
		}));
	}
}
