using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Builds.Common;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.BuildServers;

public static class BuildServersContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static BuildServersContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Ci5CdWlsZHMvUGFja1NlcnZlcnMvQnVpbGRTZXJ2ZXJzQ29udHJhY3RzLnBy" + "b3RvEhZIeWRyYS5BcGkuQnVpbGRTZXJ2ZXJzGh9nb29nbGUvcHJvdG9idWYv" + "dGltZXN0YW1wLnByb3RvGhlDb250ZXh0L1Rvb2xDb250ZXh0LnByb3RvGiJC" + "dWlsZHMvQnVpbGRzQ29tbW9uQ29udHJhY3RzLnByb3RvImYKH0dldFVwbG9h" + "ZENyZWRlbnRpYWxzVG9vbFJlcXVlc3QSQwoMdG9vbF9jb250ZXh0GAEgASgL" + "Mi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVG9vbENvbnRl" + "eHQitAEKIEdldFVwbG9hZENyZWRlbnRpYWxzVG9vbFJlc3BvbnNlEg8KB3Bh" + "Y2tfaWQYASABKAkSDQoFdG9rZW4YAiABKAkSQAoKdG9rZW5fdHlwZRgDIAEo" + "DjIsLkh5ZHJhLkFwaS5CdWlsZFNlcnZlcnMuQ3JlZGVudGlhbHNUb2tlblR5" + "cGUSLgoKZXhwaXJlc19vbhgEIAEoCzIaLmdvb2dsZS5wcm90b2J1Zi5UaW1l" + "c3RhbXAifQohQW5hbHl6ZUJ1aWxkUGFja0NvbmZpZ1Rvb2xSZXF1ZXN0EkMK" + "DHRvb2xfY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVy" + "ZS5Db250ZXh0LlRvb2xDb250ZXh0EhMKC2NvbmZpZ19kYXRhGAIgASgJIiQK" + "IkFuYWx5emVCdWlsZFBhY2tDb25maWdUb29sUmVzcG9uc2UikQIKIFVwbG9h" + "ZEJ1aWxkUGFja0NvbmZpZ1Rvb2xSZXF1ZXN0EkMKDHRvb2xfY29udGV4dBgB" + "IAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlRvb2xD" + "b250ZXh0EhMKC2NvbmZpZ19kYXRhGAIgASgJEg8KB3BhY2tfaWQYAyABKAkS" + "TgoWYnVpbGRfdmVyc2lvbnNfd2l0aF9pZBgEIAMoCzIuLkh5ZHJhLkFwaS5C" + "dWlsZHMuQ29tbW9uLkJ1aWxkVmVyc2lvbldpdGhJZER0bxIXCg9jb21wcmVz" + "c2VkX3NpemUYBSABKAMSGQoRZGVjb21wcmVzc2VkX3NpemUYBiABKAMiIwoh" + "VXBsb2FkQnVpbGRQYWNrQ29uZmlnVG9vbFJlc3BvbnNlKl4KFENyZWRlbnRp" + "YWxzVG9rZW5UeXBlEiIKHkNSRURFTlRJQUxTX1RPS0VOX1RZUEVfVU5LTk9X" + "ThAAEiIKHkNSRURFTlRJQUxTX1RPS0VOX1RZUEVfU0FTX1VSSRABYgZwcm90" + "bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[3]
		{
			TimestampReflection.Descriptor,
			ToolContextReflection.Descriptor,
			BuildsCommonContractsReflection.Descriptor
		}, new GeneratedClrTypeInfo(new System.Type[1] { typeof(CredentialsTokenType) }, null, new GeneratedClrTypeInfo[6]
		{
			new GeneratedClrTypeInfo(typeof(GetUploadCredentialsToolRequest), GetUploadCredentialsToolRequest.Parser, new string[1] { "ToolContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetUploadCredentialsToolResponse), GetUploadCredentialsToolResponse.Parser, new string[4] { "PackId", "Token", "TokenType", "ExpiresOn" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(AnalyzeBuildPackConfigToolRequest), AnalyzeBuildPackConfigToolRequest.Parser, new string[2] { "ToolContext", "ConfigData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(AnalyzeBuildPackConfigToolResponse), AnalyzeBuildPackConfigToolResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UploadBuildPackConfigToolRequest), UploadBuildPackConfigToolRequest.Parser, new string[6] { "ToolContext", "ConfigData", "PackId", "BuildVersionsWithId", "CompressedSize", "DecompressedSize" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UploadBuildPackConfigToolResponse), UploadBuildPackConfigToolResponse.Parser, null, null, null, null, null)
		}));
	}
}
