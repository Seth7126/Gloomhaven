using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Auth;
using Hydra.Api.Infrastructure.Context;

namespace RedLynx.Api.CrossSave;

public static class CrossSaveContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static CrossSaveContractsReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci5yZWRseW54LWFwaS9Dcm9zc1NhdmUvQ3Jvc3NTYXZlQ29udHJhY3RzLnBy" + "b3RvEhVSZWRMeW54LkFwaS5Dcm9zc1NhdmUaGUNvbnRleHQvVXNlckNvbnRl" + "eHQucHJvdG8aIUF1dGgvQXV0aG9yaXphdGlvbkNvbnRyYWN0cy5wcm90bxoe" + "Z29vZ2xlL3Byb3RvYnVmL2R1cmF0aW9uLnByb3RvGh9nb29nbGUvcHJvdG9i" + "dWYvdGltZXN0YW1wLnByb3RvIpMBChVTdWJtaXRTbmFwc2hvdFJlcXVlc3QS" + "QwoMdXNlcl9jb250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0" + "dXJlLkNvbnRleHQuVXNlckNvbnRleHQSNQoIc25hcHNob3QYAiABKAsyIy5S" + "ZWRMeW54LkFwaS5Dcm9zc1NhdmUuU2F2ZVNuYXBzaG90IkUKFlN1Ym1pdFNu" + "YXBzaG90UmVzcG9uc2USKwoIY29vbGRvd24YASABKAsyGS5nb29nbGUucHJv" + "dG9idWYuRHVyYXRpb24iXQoWR2V0VHJhbnNmZXJJbmZvUmVxdWVzdBJDCgx1" + "c2VyX2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUu" + "Q29udGV4dC5Vc2VyQ29udGV4dCKpAgoXR2V0VHJhbnNmZXJJbmZvUmVzcG9u" + "c2USEwoLdHJhbnNmZXJfaWQYASABKAkSFAoMY29udGVudF90eXBlGAIgASgJ" + "Eh0KFWRhdGFfZGVzY3JpcHRpb25fanNvbhgDIAEoCRIqCghwbGF0Zm9ybRgE" + "IAEoDjIYLkh5ZHJhLkFwaS5BdXRoLlBsYXRmb3JtEioKCHByb3ZpZGVyGAUg" + "ASgOMhguSHlkcmEuQXBpLkF1dGguUHJvdmlkZXISMwoPc2F2ZV9jcmVhdGVk" + "X2F0GAYgASgLMhouZ29vZ2xlLnByb3RvYnVmLlRpbWVzdGFtcBI3ChN0cmFu" + "c2Zlcl9jcmVhdGVkX2F0GAcgASgLMhouZ29vZ2xlLnByb3RvYnVmLlRpbWVz" + "dGFtcCKxAQoXQ29tcGxldGVUcmFuc2ZlclJlcXVlc3QSQwoMdXNlcl9jb250" + "ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQu" + "VXNlckNvbnRleHQSEwoLdHJhbnNmZXJfaWQYAiABKAkSPAoPYmFja3VwX3Nu" + "YXBzaG90GAMgASgLMiMuUmVkTHlueC5BcGkuQ3Jvc3NTYXZlLlNhdmVTbmFw" + "c2hvdCJaChhDb21wbGV0ZVRyYW5zZmVyUmVzcG9uc2USPgoRdHJhbnNmZXJf" + "c25hcHNob3QYASABKAsyIy5SZWRMeW54LkFwaS5Dcm9zc1NhdmUuU2F2ZVNu" + "YXBzaG90Im8KE1NraXBUcmFuc2ZlclJlcXVlc3QSQwoMdXNlcl9jb250ZXh0" + "GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNl" + "ckNvbnRleHQSEwoLdHJhbnNmZXJfaWQYAiABKAkiFgoUU2tpcFRyYW5zZmVy" + "UmVzcG9uc2UicwoMU2F2ZVNuYXBzaG90Ei4KBXNhdmVzGAEgAygLMh8uUmVk" + "THlueC5BcGkuQ3Jvc3NTYXZlLlNhdmVEYXRhEhQKDGNvbnRlbnRfdHlwZRgC" + "IAEoCRIdChVkYXRhX2Rlc2NyaXB0aW9uX2pzb24YAyABKAkiJQoIU2F2ZURh" + "dGESCwoDa2V5GAEgASgJEgwKBGRhdGEYAiABKAxiBnByb3RvMw=="), new FileDescriptor[4]
		{
			UserContextReflection.Descriptor,
			AuthorizationContractsReflection.Descriptor,
			DurationReflection.Descriptor,
			TimestampReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[10]
		{
			new GeneratedClrTypeInfo(typeof(SubmitSnapshotRequest), SubmitSnapshotRequest.Parser, new string[2] { "UserContext", "Snapshot" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SubmitSnapshotResponse), SubmitSnapshotResponse.Parser, new string[1] { "Cooldown" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetTransferInfoRequest), GetTransferInfoRequest.Parser, new string[1] { "UserContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetTransferInfoResponse), GetTransferInfoResponse.Parser, new string[7] { "TransferId", "ContentType", "DataDescriptionJson", "Platform", "Provider", "SaveCreatedAt", "TransferCreatedAt" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(CompleteTransferRequest), CompleteTransferRequest.Parser, new string[3] { "UserContext", "TransferId", "BackupSnapshot" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(CompleteTransferResponse), CompleteTransferResponse.Parser, new string[1] { "TransferSnapshot" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SkipTransferRequest), SkipTransferRequest.Parser, new string[2] { "UserContext", "TransferId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SkipTransferResponse), SkipTransferResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SaveSnapshot), SaveSnapshot.Parser, new string[3] { "Saves", "ContentType", "DataDescriptionJson" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SaveData), SaveData.Parser, new string[2] { "Key", "Data" }, null, null, null, null)
		}));
	}
}
