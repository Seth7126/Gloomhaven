using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Leaderboards;

public static class LeaderboardsContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static LeaderboardsContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CihMZWFkZXJib2FyZHMvTGVhZGVyYm9hcmRzQ29udHJhY3RzLnByb3RvEhZI" + "eWRyYS5BcGkuTGVhZGVyYm9hcmRzGh9nb29nbGUvcHJvdG9idWYvdGltZXN0" + "YW1wLnByb3RvGhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvGhtDb250ZXh0" + "L1NlcnZlckNvbnRleHQucHJvdG8imwEKEExlYWRlcmJvYXJkRW50cnkSDwoH" + "dXNlcl9pZBgBIAEoCRIOCgZyYXRpbmcYAiABKAESEAoIcG9zaXRpb24YAyAB" + "KAUSMAoMbGFzdF91cGRhdGVkGAQgASgLMhouZ29vZ2xlLnByb3RvYnVmLlRp" + "bWVzdGFtcBITCgtjdXN0b21fZGF0YRgFIAEoCRINCgVjb3VudBgGIAEoBSK2" + "AQoPTGVhZGVyYm9hcmREYXRhEhYKDmxlYWRlcmJvYXJkX2lkGAEgASgJEjkK" + "B2VudHJpZXMYAiADKAsyKC5IeWRyYS5BcGkuTGVhZGVyYm9hcmRzLkxlYWRl" + "cmJvYXJkRW50cnkSOwoJc2VsZl9kYXRhGAMgASgLMiguSHlkcmEuQXBpLkxl" + "YWRlcmJvYXJkcy5MZWFkZXJib2FyZEVudHJ5EhMKC3RvdGFsX2NvdW50GAQg" + "ASgFIlsKEkxlYWRlcmJvYXJkUmVxdWVzdBIWCg5sZWFkZXJib2FyZF9pZBgC" + "IAEoCRIWCg5zdGFydF9wb3NpdGlvbhgDIAEoBRIVCg1yZXN1bHRzX2NvdW50" + "GAQgASgFIpYBChZHZXRMZWFkZXJib2FyZHNSZXF1ZXN0Ej4KB2NvbnRleHQY" + "ASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2Vy" + "Q29udGV4dBI8CghyZXF1ZXN0cxgCIAMoCzIqLkh5ZHJhLkFwaS5MZWFkZXJi" + "b2FyZHMuTGVhZGVyYm9hcmRSZXF1ZXN0IlMKF0dldExlYWRlcmJvYXJkc1Jl" + "c3BvbnNlEjgKB3Jlc3VsdHMYASADKAsyJy5IeWRyYS5BcGkuTGVhZGVyYm9h" + "cmRzLkxlYWRlcmJvYXJkRGF0YSKJAQodR2V0TGVhZGVyYm9hcmRGaWx0ZXJl" + "ZFJlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0" + "cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0EhYKDmxlYWRlcmJvYXJkX2lk" + "GAIgASgJEhAKCHVzZXJfaWRzGAMgAygJIlcKHkdldExlYWRlcmJvYXJkRmls" + "dGVyZWRSZXNwb25zZRI1CgRkYXRhGAEgASgLMicuSHlkcmEuQXBpLkxlYWRl" + "cmJvYXJkcy5MZWFkZXJib2FyZERhdGEiQgoLVXBkYXRlRW50cnkSDwoHdXNl" + "cl9pZBgBIAEoCRINCgV2YWx1ZRgCIAEoARITCgtjdXN0b21fZGF0YRgDIAEo" + "CSKsAQocVXBkYXRlTGVhZGVyYm9hcmRVc2VyUmVxdWVzdBI+Cgdjb250ZXh0" + "GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNl" + "ckNvbnRleHQSFgoObGVhZGVyYm9hcmRfaWQYAiABKAkSNAoHZW50cmllcxgD" + "IAMoCzIjLkh5ZHJhLkFwaS5MZWFkZXJib2FyZHMuVXBkYXRlRW50cnkiHwod" + "VXBkYXRlTGVhZGVyYm9hcmRVc2VyUmVzcG9uc2UisAEKHlVwZGF0ZUxlYWRl" + "cmJvYXJkU2VydmVyUmVxdWVzdBJACgdjb250ZXh0GAEgASgLMi8uSHlkcmEu" + "QXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuU2VydmVyQ29udGV4dBIWCg5s" + "ZWFkZXJib2FyZF9pZBgCIAEoCRI0CgdlbnRyaWVzGAMgAygLMiMuSHlkcmEu" + "QXBpLkxlYWRlcmJvYXJkcy5VcGRhdGVFbnRyeSIhCh9VcGRhdGVMZWFkZXJi" + "b2FyZFNlcnZlclJlc3BvbnNlYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[3]
		{
			TimestampReflection.Descriptor,
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[12]
		{
			new GeneratedClrTypeInfo(typeof(LeaderboardEntry), LeaderboardEntry.Parser, new string[6] { "UserId", "Rating", "Position", "LastUpdated", "CustomData", "Count" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(LeaderboardData), LeaderboardData.Parser, new string[4] { "LeaderboardId", "Entries", "SelfData", "TotalCount" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(LeaderboardRequest), LeaderboardRequest.Parser, new string[3] { "LeaderboardId", "StartPosition", "ResultsCount" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetLeaderboardsRequest), GetLeaderboardsRequest.Parser, new string[2] { "Context", "Requests" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetLeaderboardsResponse), GetLeaderboardsResponse.Parser, new string[1] { "Results" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetLeaderboardFilteredRequest), GetLeaderboardFilteredRequest.Parser, new string[3] { "Context", "LeaderboardId", "UserIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetLeaderboardFilteredResponse), GetLeaderboardFilteredResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateEntry), UpdateEntry.Parser, new string[3] { "UserId", "Value", "CustomData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateLeaderboardUserRequest), UpdateLeaderboardUserRequest.Parser, new string[3] { "Context", "LeaderboardId", "Entries" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateLeaderboardUserResponse), UpdateLeaderboardUserResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateLeaderboardServerRequest), UpdateLeaderboardServerRequest.Parser, new string[3] { "Context", "LeaderboardId", "Entries" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateLeaderboardServerResponse), UpdateLeaderboardServerResponse.Parser, null, null, null, null, null)
		}));
	}
}
