using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Builds.Common;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Builds;

public static class BuildsGroupContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static BuildsGroupContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CihCdWlsZHMvR3JvdXBzL0J1aWxkc0dyb3VwQ29udHJhY3RzLnByb3RvEhBI" + "eWRyYS5BcGkuQnVpbGRzGhlDb250ZXh0L1Rvb2xDb250ZXh0LnByb3RvGh9n" + "b29nbGUvcHJvdG9idWYvdGltZXN0YW1wLnByb3RvGiJCdWlsZHMvQnVpbGRz" + "Q29tbW9uQ29udHJhY3RzLnByb3RvIr8BCiBSZWdpc3RlckJ1aWxkVmVyc2lv" + "bnNUb29sUmVxdWVzdBJDCgx0b29sX2NvbnRleHQYASABKAsyLS5IeWRyYS5B" + "cGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Ub29sQ29udGV4dBISCgpncm91" + "cF9uYW1lGAIgASgJEkIKE2J1aWxkX3ZlcnNpb25faW5mb3MYAyADKAsyJS5I" + "eWRyYS5BcGkuQnVpbGRzLkJ1aWxkVmVyc2lvbkluZm9EdG8ijwEKIVJlZ2lz" + "dGVyQnVpbGRWZXJzaW9uc1Rvb2xSZXNwb25zZRIXCg9mYWlsZWRfdmVyc2lv" + "bnMYASADKAkSUQoZcmVnaXN0ZXJlZF9idWlsZF92ZXJzaW9ucxgCIAMoCzIu" + "Lkh5ZHJhLkFwaS5CdWlsZHMuQ29tbW9uLkJ1aWxkVmVyc2lvbldpdGhJZER0" + "byKJAgoTQnVpbGRWZXJzaW9uSW5mb0R0bxIVCg1idWlsZF92ZXJzaW9uGAEg" + "ASgJEkkKCmF0dHJpYnV0ZXMYAiADKAsyNS5IeWRyYS5BcGkuQnVpbGRzLkJ1" + "aWxkVmVyc2lvbkluZm9EdG8uQXR0cmlidXRlc0VudHJ5Eg4KBnBpbm5lZBgD" + "IAEoCBIwCgxjcmVhdGVkX3RpbWUYBCABKAsyGi5nb29nbGUucHJvdG9idWYu" + "VGltZXN0YW1wEgoKAmlkGAUgASgJEg8KB3BhY2tfaWQYBiABKAkaMQoPQXR0" + "cmlidXRlc0VudHJ5EgsKA2tleRgBIAEoCRINCgV2YWx1ZRgCIAEoCToCOAFi" + "BnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[3]
		{
			ToolContextReflection.Descriptor,
			TimestampReflection.Descriptor,
			BuildsCommonContractsReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[3]
		{
			new GeneratedClrTypeInfo(typeof(RegisterBuildVersionsToolRequest), RegisterBuildVersionsToolRequest.Parser, new string[3] { "ToolContext", "GroupName", "BuildVersionInfos" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RegisterBuildVersionsToolResponse), RegisterBuildVersionsToolResponse.Parser, new string[2] { "FailedVersions", "RegisteredBuildVersions" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(BuildVersionInfoDto), BuildVersionInfoDto.Parser, new string[6] { "BuildVersion", "Attributes", "Pinned", "CreatedTime", "Id", "PackId" }, null, null, null, new GeneratedClrTypeInfo[1])
		}));
	}
}
