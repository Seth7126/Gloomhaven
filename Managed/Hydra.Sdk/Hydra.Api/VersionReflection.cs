using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api;

public static class VersionReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static VersionReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Cg1WZXJzaW9uLnByb3RvEglIeWRyYS5BcGkaIGdvb2dsZS9wcm90b2J1Zi9k" + "ZXNjcmlwdG9yLnByb3RvIiAKEEJ1aWxkSW5mb3JtYXRpb246DIq1GAIKAIq1" + "GAISACImCgVCdWlsZBIPCgd2ZXJzaW9uGAEgASgJEgwKBGhhc2gYAiABKAk6" + "QgoFYnVpbGQSHy5nb29nbGUucHJvdG9idWYuTWVzc2FnZU9wdGlvbnMY0YYD" + "IAEoCzIQLkh5ZHJhLkFwaS5CdWlsZGIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { DescriptorReflection.Descriptor }, new GeneratedClrTypeInfo(null, new Extension[1] { VersionExtensions.Build }, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(BuildInformation), BuildInformation.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(Build), Build.Parser, new string[2] { "Version", "Hash" }, null, null, null, null)
		}));
	}
}
