using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public static class ConfigurationContextReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ConfigurationContextReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiJDb250ZXh0L0NvbmZpZ3VyYXRpb25Db250ZXh0LnByb3RvEiBIeWRyYS5B" + "cGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dCJzChRDb25maWd1cmF0aW9uQ29u" + "dGV4dBJICgRkYXRhGAEgASgLMjouSHlkcmEuQXBpLkluZnJhc3RydWN0dXJl" + "LkNvbnRleHQuQ29uZmlndXJhdGlvbkNvbnRleHREYXRhEhEKCXNpZ25hdHVy" + "ZRgCIAEoDCJIChhDb25maWd1cmF0aW9uQ29udGV4dERhdGESEAoIdGl0bGVf" + "aWQYASABKAkSGgoSY29uZmlndXJhdGlvbl9oYXNoGAIgASgJYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(ConfigurationContext), ConfigurationContext.Parser, new string[2] { "Data", "Signature" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConfigurationContextData), ConfigurationContextData.Parser, new string[2] { "TitleId", "ConfigurationHash" }, null, null, null, null)
		}));
	}
}
