using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.SessionControl;

namespace Hydra.Api.ServerManagerConfiguration;

public static class DevDSMConfigurationContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static DevDSMConfigurationContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Cj1TZXJ2ZXJNYW5hZ2VyQ29uZmlndXJhdGlvbi9EZXZEU01Db25maWd1cmF0" + "aW9uQ29udHJhY3RzLnByb3RvEiRIeWRyYS5BcGkuU2VydmVyTWFuYWdlckNv" + "bmZpZ3VyYXRpb24aK1NlcnZlck1hbmFnZXJTY2hlZHVsZXIvUGVuZGluZ1Nl" + "c3Npb24ucHJvdG8aGUNvbnRleHQvVG9vbENvbnRleHQucHJvdG8iYgoPUmVn" + "aXN0ZXJSZXF1ZXN0Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5m" + "cmFzdHJ1Y3R1cmUuQ29udGV4dC5Ub29sQ29udGV4dBIPCgd2ZXJzaW9uGAIg" + "ASgJIh4KEFJlZ2lzdGVyUmVzcG9uc2USCgoCaWQYASABKAkiXwoRVW5yZWdp" + "c3RlclJlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZy" + "YXN0cnVjdHVyZS5Db250ZXh0LlRvb2xDb250ZXh0EgoKAmlkGAIgASgJIhQK" + "ElVucmVnaXN0ZXJSZXNwb25zZSJ2ChlHZXRQZW5kaW5nU2Vzc2lvbnNSZXF1" + "ZXN0Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1" + "cmUuQ29udGV4dC5Ub29sQ29udGV4dBIZChFzZXJ2ZXJfbWFuYWdlcl9pZBgC" + "IAEoCSJ3ChpHZXRQZW5kaW5nU2Vzc2lvbnNSZXNwb25zZRI6CghzZXNzaW9u" + "cxgBIAMoCzIoLkh5ZHJhLkFwaS5TZXNzaW9uQ29udHJvbC5QZW5kaW5nU2Vz" + "c2lvbhIdChVyZWZyZXNoX2FmdGVyX3NlY29uZHMYAiABKAUiswEKG1JlamVj" + "dFBlbmRpbmdTZXNzaW9uUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgLMi0uSHlk" + "cmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVG9vbENvbnRleHQSGQoR" + "c2VydmVyX21hbmFnZXJfaWQYAyABKAkSOQoHc2Vzc2lvbhgEIAEoCzIoLkh5" + "ZHJhLkFwaS5TZXNzaW9uQ29udHJvbC5QZW5kaW5nU2Vzc2lvbiIeChxSZWpl" + "Y3RQZW5kaW5nU2Vzc2lvblJlc3BvbnNlYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			PendingSessionReflection.Descriptor,
			ToolContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[8]
		{
			new GeneratedClrTypeInfo(typeof(RegisterRequest), RegisterRequest.Parser, new string[2] { "Context", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RegisterResponse), RegisterResponse.Parser, new string[1] { "Id" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UnregisterRequest), UnregisterRequest.Parser, new string[2] { "Context", "Id" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UnregisterResponse), UnregisterResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetPendingSessionsRequest), GetPendingSessionsRequest.Parser, new string[2] { "Context", "ServerManagerId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetPendingSessionsResponse), GetPendingSessionsResponse.Parser, new string[2] { "Sessions", "RefreshAfterSeconds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RejectPendingSessionRequest), RejectPendingSessionRequest.Parser, new string[3] { "Context", "ServerManagerId", "Session" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RejectPendingSessionResponse), RejectPendingSessionResponse.Parser, null, null, null, null, null)
		}));
	}
}
