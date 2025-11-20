using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Push;

public static class PushClientDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushClientDataReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChlQdXNoL1B1c2hDbGllbnREYXRhLnByb3RvEg5IeWRyYS5BcGkuUHVzaBoW" + "UHVzaC9QdXNoVmVyc2lvbi5wcm90bxoUUHVzaC9QdXNoVG9rZW4ucHJvdG8a" + "GUNvbnRleHQvVXNlckNvbnRleHQucHJvdG8aG0NvbnRleHQvU2VydmVyQ29u" + "dGV4dC5wcm90byKhAgoVUHVzaEF1dGhvcml6YXRpb25EYXRhEhIKCmdlbmVy" + "YXRpb24YASABKAUSKAoFdG9rZW4YAiABKAsyGS5IeWRyYS5BcGkuUHVzaC5Q" + "dXNoVG9rZW4SLQoIdmVyc2lvbnMYAyADKAsyGy5IeWRyYS5BcGkuUHVzaC5Q" + "dXNoVmVyc2lvbhJFCgx1c2VyX2NvbnRleHQYBCABKAsyLS5IeWRyYS5BcGku" + "SW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dEgAEkkKDnNlcnZl" + "cl9jb250ZXh0GAUgASgLMi8uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNv" + "bnRleHQuU2VydmVyQ29udGV4dEgAQgkKB2NvbnRleHRiBnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[4]
		{
			PushVersionReflection.Descriptor,
			PushTokenReflection.Descriptor,
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PushAuthorizationData), PushAuthorizationData.Parser, new string[5] { "Generation", "Token", "Versions", "UserContext", "ServerContext" }, new string[1] { "Context" }, null, null, null)
		}));
	}
}
