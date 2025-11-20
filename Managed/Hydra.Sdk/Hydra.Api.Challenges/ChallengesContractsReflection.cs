using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.Push;

namespace Hydra.Api.Challenges;

public static class ChallengesContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ChallengesContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiRDaGFsbGVuZ2VzL0NoYWxsZW5nZXNDb250cmFjdHMucHJvdG8SFEh5ZHJh" + "LkFwaS5DaGFsbGVuZ2VzGh9DaGFsbGVuZ2VzL0NoYWxsZW5nZXNDb3JlLnBy" + "b3RvGhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvGhtDb250ZXh0L1NlcnZl" + "ckNvbnRleHQucHJvdG8aIkNvbnRleHQvQ29uZmlndXJhdGlvbkNvbnRleHQu" + "cHJvdG8aFFB1c2gvUHVzaFRva2VuLnByb3RvItYBCg5Db25uZWN0UmVxdWVz" + "dBI+Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJl" + "LkNvbnRleHQuVXNlckNvbnRleHQSVQoVY29uZmlndXJhdGlvbl9jb250ZXh0" + "GAIgASgLMjYuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuQ29u" + "ZmlndXJhdGlvbkNvbnRleHQSLQoKcHVzaF90b2tlbhgDIAEoCzIZLkh5ZHJh" + "LkFwaS5QdXNoLlB1c2hUb2tlbiJqCg9Db25uZWN0UmVzcG9uc2USRgoUdXNl" + "cl9jaGFsbGVuZ2VzX2luZm8YASABKAsyKC5IeWRyYS5BcGkuQ2hhbGxlbmdl" + "cy5Vc2VyQ2hhbGxlbmdlc0luZm8SDwoHdmVyc2lvbhgCIAEoAyKtAQoUR2V0" + "Q2hhbGxlbmdlc1JlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFw" + "aS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0ElUKFWNvbmZp" + "Z3VyYXRpb25fY29udGV4dBgCIAEoCzI2Lkh5ZHJhLkFwaS5JbmZyYXN0cnVj" + "dHVyZS5Db250ZXh0LkNvbmZpZ3VyYXRpb25Db250ZXh0InAKFUdldENoYWxs" + "ZW5nZXNSZXNwb25zZRJGChR1c2VyX2NoYWxsZW5nZXNfaW5mbxgBIAEoCzIo" + "Lkh5ZHJhLkFwaS5DaGFsbGVuZ2VzLlVzZXJDaGFsbGVuZ2VzSW5mbxIPCgd2" + "ZXJzaW9uGAIgASgDItQBCiVHZXRDaGFsbGVuZ2VzSW5jcmVtZW50YWxVcGRh" + "dGVSZXF1ZXN0Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFz" + "dHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dBJVChVjb25maWd1cmF0aW9u" + "X2NvbnRleHQYAiABKAsyNi5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29u" + "dGV4dC5Db25maWd1cmF0aW9uQ29udGV4dBIUCgxmcm9tX3ZlcnNpb24YAyAB" + "KAMinAEKJkdldENoYWxsZW5nZXNJbmNyZW1lbnRhbFVwZGF0ZVJlc3BvbnNl" + "EmEKInVzZXJfY2hhbGxlbmdlc19pbmNyZW1lbnRhbF91cGRhdGUYASABKAsy" + "NS5IeWRyYS5BcGkuQ2hhbGxlbmdlcy5Vc2VyQ2hhbGxlbmdlc0luY3JlbWVu" + "dGFsVXBkYXRlEg8KB3ZlcnNpb24YAiABKAMi3QEKJFNlcnZlclN1Ym1pdENo" + "YWxsZW5nZUNvdW50ZXJzUmVxdWVzdBJACgdjb250ZXh0GAEgASgLMi8uSHlk" + "cmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuU2VydmVyQ29udGV4dBIU" + "CgxyZWZlcmVuY2VfaWQYAiABKAkSRQoPdXNlcl9vcGVyYXRpb25zGAMgAygL" + "MiwuSHlkcmEuQXBpLkNoYWxsZW5nZXMuQ2hhbGxlbmdlT3BlcmF0aW9uTGlz" + "dBIWCg5pc19sYXN0X3VwZGF0ZRgEIAEoCCJACiVTZXJ2ZXJTdWJtaXRDaGFs" + "bGVuZ2VDb3VudGVyc1Jlc3BvbnNlEhcKD2ZhaWxlZF91c2VyX2lkcxgBIAMo" + "CSJwChpTZXJ2ZXJHZXRDaGFsbGVuZ2VzUmVxdWVzdBJACgdjb250ZXh0GAEg" + "ASgLMi8uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuU2VydmVy" + "Q29udGV4dBIQCgh1c2VyX2lkcxgCIAMoCSJ4ChdQZXJVc2VyQ2hhbGxlbmdl" + "Q291bnRlchIPCgd1c2VyX2lkGAEgASgJEkwKEmNoYWxsZW5nZV9jb3VudGVy" + "cxgCIAMoCzIwLkh5ZHJhLkFwaS5DaGFsbGVuZ2VzLkNoYWxsZW5nZUNvdW50" + "ZXJXaXRoRXZlbnRzIsUBChtTZXJ2ZXJHZXRDaGFsbGVuZ2VzUmVzcG9uc2US" + "UgobcGVyX3VzZXJfY2hhbGxlbmdlX2NvdW50ZXJzGAEgAygLMi0uSHlkcmEu" + "QXBpLkNoYWxsZW5nZXMuUGVyVXNlckNoYWxsZW5nZUNvdW50ZXISUgoac2Vy" + "dmVyX2NoYWxsZW5nZXNfc2V0dGluZ3MYAiABKAsyLi5IeWRyYS5BcGkuQ2hh" + "bGxlbmdlcy5TZXJ2ZXJDaGFsbGVuZ2VzU2V0dGluZ3MiWQoYU2VydmVyQ2hh" + "bGxlbmdlc1NldHRpbmdzEh8KF2ZsdXNoX3RpbWVfY3JpdGVyaWFfc2VjGAEg" + "ASgDEhwKFGZsdXNoX2NvdW50X2NyaXRlcmlhGAIgASgDYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[5]
		{
			ChallengesCoreReflection.Descriptor,
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor,
			ConfigurationContextReflection.Descriptor,
			PushTokenReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[12]
		{
			new GeneratedClrTypeInfo(typeof(ConnectRequest), ConnectRequest.Parser, new string[3] { "Context", "ConfigurationContext", "PushToken" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConnectResponse), ConnectResponse.Parser, new string[2] { "UserChallengesInfo", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetChallengesRequest), GetChallengesRequest.Parser, new string[2] { "Context", "ConfigurationContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetChallengesResponse), GetChallengesResponse.Parser, new string[2] { "UserChallengesInfo", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetChallengesIncrementalUpdateRequest), GetChallengesIncrementalUpdateRequest.Parser, new string[3] { "Context", "ConfigurationContext", "FromVersion" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetChallengesIncrementalUpdateResponse), GetChallengesIncrementalUpdateResponse.Parser, new string[2] { "UserChallengesIncrementalUpdate", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerSubmitChallengeCountersRequest), ServerSubmitChallengeCountersRequest.Parser, new string[4] { "Context", "ReferenceId", "UserOperations", "IsLastUpdate" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerSubmitChallengeCountersResponse), ServerSubmitChallengeCountersResponse.Parser, new string[1] { "FailedUserIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerGetChallengesRequest), ServerGetChallengesRequest.Parser, new string[2] { "Context", "UserIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PerUserChallengeCounter), PerUserChallengeCounter.Parser, new string[2] { "UserId", "ChallengeCounters" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerGetChallengesResponse), ServerGetChallengesResponse.Parser, new string[2] { "PerUserChallengeCounters", "ServerChallengesSettings" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerChallengesSettings), ServerChallengesSettings.Parser, new string[2] { "FlushTimeCriteriaSec", "FlushCountCriteria" }, null, null, null, null)
		}));
	}
}
