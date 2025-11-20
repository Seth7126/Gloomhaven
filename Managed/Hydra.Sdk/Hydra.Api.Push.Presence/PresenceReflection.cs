using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Errors;
using Hydra.Api.Nullable;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public static class PresenceReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PresenceReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChxQdXNoL1ByZXNlbmNlL1ByZXNlbmNlLnByb3RvEhdIeWRyYS5BcGkuUHVz" + "aC5QcmVzZW5jZRoaUHJlc2VuY2UvUGFydHlTdGF0dXMucHJvdG8aHlByZXNl" + "bmNlL01hdGNobWFrZVN0YXR1cy5wcm90bxoZUHJlc2VuY2UvSW52aXRlRGF0" + "YS5wcm90bxoWRXJyb3JzL0Vycm9yQ29kZS5wcm90bxoXTnVsbGFibGUvTnVs" + "bGFibGUucHJvdG8ijgEKC0ludml0ZUV2ZW50EjwKCmV2ZW50X3R5cGUYASAB" + "KA4yKC5IeWRyYS5BcGkuUHVzaC5QcmVzZW5jZS5JbnZpdGVFdmVudFR5cGUS" + "LAoEZGF0YRgCIAEoCzIeLkh5ZHJhLkFwaS5QcmVzZW5jZS5JbnZpdGVEYXRh" + "EhMKC3NlcXVlbmNlX2lkGAMgASgFImUKGUxvbmdPcGVyYXRpb25SZXN1bHRV" + "cGRhdGUSFgoOY29ycmVsYXRpb25faWQYASABKAkSMAoLcmVzdWx0X2NvZGUY" + "AiABKA4yGy5IeWRyYS5BcGkuRXJyb3JzLkVycm9yQ29kZSKkAQoSUHJlc2Vu" + "Y2VVc2VyVXBkYXRlEjsKDWludml0ZV9ldmVudHMYASADKAsyJC5IeWRyYS5B" + "cGkuUHVzaC5QcmVzZW5jZS5JbnZpdGVFdmVudBJRChVsb25nX29wZXJhdGlv" + "bl9yZXN1bHQYAiABKAsyMi5IeWRyYS5BcGkuUHVzaC5QcmVzZW5jZS5Mb25n" + "T3BlcmF0aW9uUmVzdWx0VXBkYXRlIqQBChtQcmVzZW5jZVNlc3Npb25NZW1i" + "ZXJVcGRhdGUSTQoLdXBkYXRlX3R5cGUYAiABKA4yOC5IeWRyYS5BcGkuUHVz" + "aC5QcmVzZW5jZS5QcmVzZW5jZVNlc3Npb25NZW1iZXJVcGRhdGVUeXBlEjYK" + "C21lbWJlcl9kYXRhGAMgASgLMiEuSHlkcmEuQXBpLlByZXNlbmNlLlNlc3Np" + "b25NZW1iZXIiawoYUHJlc2VuY2VTZXNzaW9uUXVldWVEYXRhEhQKDHBsYXlf" + "bGlzdF9pZBgBIAEoCRI5Cg5xdWV1ZV92YXJpYW50cxgCIAMoCzIhLkh5ZHJh" + "LkFwaS5QcmVzZW5jZS5RdWV1ZVZhcmlhbnRzIuMCChdQcmVzZW5jZVNlc3Np" + "b25HYW1lRGF0YRI4CgxwbGF5X2xpc3RfaWQYASABKAsyIi5IeWRyYS5BcGku" + "TnVsbGFibGUuTnVsbGFibGVTdHJpbmcSMQoIdmFyaWFudHMYAiADKAsyHy5I" + "eWRyYS5BcGkuUHJlc2VuY2UuR2FtZVZhcmlhbnQSPgoIc2V0dGluZ3MYAyAB" + "KAsyLC5IeWRyYS5BcGkuUHJlc2VuY2UuTWF0Y2htYWtlU2Vzc2lvblNldHRp" + "bmdzEjAKBGRhdGEYBCABKAsyIi5IeWRyYS5BcGkuTnVsbGFibGUuTnVsbGFi" + "bGVTdHJpbmcSTAoObWVtYmVyc191cGRhdGUYBSADKAsyNC5IeWRyYS5BcGku" + "UHVzaC5QcmVzZW5jZS5QcmVzZW5jZVNlc3Npb25NZW1iZXJVcGRhdGUSGwoT" + "c19jX21hbmFnZWRfY29udGV4dBgGIAEoCSKFAgoVUHJlc2VuY2VTZXNzaW9u" + "VXBkYXRlEi0KAmlkGAEgASgLMiEuSHlkcmEuQXBpLlByZXNlbmNlLkdhbWVT" + "ZXNzaW9uSWQSMQoFc3RhdGUYAiABKA4yIi5IeWRyYS5BcGkuUHJlc2VuY2Uu" + "TWF0Y2htYWtlU3RhdGUSRQoKcXVldWVfZGF0YRgDIAEoCzIxLkh5ZHJhLkFw" + "aS5QdXNoLlByZXNlbmNlLlByZXNlbmNlU2Vzc2lvblF1ZXVlRGF0YRJDCgln" + "YW1lX2RhdGEYBCABKAsyMC5IeWRyYS5BcGkuUHVzaC5QcmVzZW5jZS5QcmVz" + "ZW5jZVNlc3Npb25HYW1lRGF0YSKhAgoTUHJlc2VuY2VQYXJ0eVVwZGF0ZRIn" + "CgJpZBgCIAEoCzIbLkh5ZHJhLkFwaS5QcmVzZW5jZS5QYXJ0eUlkEjAKBGRh" + "dGEYAyABKAsyIi5IeWRyYS5BcGkuTnVsbGFibGUuTnVsbGFibGVTdHJpbmcS" + "MwoIc2V0dGluZ3MYBCABKAsyIS5IeWRyYS5BcGkuUHJlc2VuY2UuUGFydHlT" + "ZXR0aW5ncxJDCgdtZW1iZXJzGAUgAygLMjIuSHlkcmEuQXBpLlB1c2guUHJl" + "c2VuY2UuUHJlc2VuY2VQYXJ0eU1lbWJlclVwZGF0ZRI1Cglqb2luX2NvZGUY" + "BiABKAsyIi5IeWRyYS5BcGkuTnVsbGFibGUuTnVsbGFibGVTdHJpbmcimQEK" + "GVByZXNlbmNlUGFydHlNZW1iZXJVcGRhdGUSSwoLdXBkYXRlX3R5cGUYASAB" + "KA4yNi5IeWRyYS5BcGkuUHVzaC5QcmVzZW5jZS5QcmVzZW5jZVBhcnR5TWVt" + "YmVyVXBkYXRlVHlwZRIvCgZtZW1iZXIYAiABKAsyHy5IeWRyYS5BcGkuUHJl" + "c2VuY2UuUGFydHlNZW1iZXIiaQoZUHJlc2VuY2VVc2VyVXBkYXRlVmVyc2lv" + "bhIPCgd2ZXJzaW9uGAEgASgFEjsKBnVwZGF0ZRgCIAEoCzIrLkh5ZHJhLkFw" + "aS5QdXNoLlByZXNlbmNlLlByZXNlbmNlVXNlclVwZGF0ZSJvChxQcmVzZW5j" + "ZVNlc3Npb25VcGRhdGVWZXJzaW9uEg8KB3ZlcnNpb24YASABKAUSPgoGdXBk" + "YXRlGAIgASgLMi4uSHlkcmEuQXBpLlB1c2guUHJlc2VuY2UuUHJlc2VuY2VT" + "ZXNzaW9uVXBkYXRlImsKGlByZXNlbmNlUGFydHlVcGRhdGVWZXJzaW9uEg8K" + "B3ZlcnNpb24YASABKAUSPAoGdXBkYXRlGAIgASgLMiwuSHlkcmEuQXBpLlB1" + "c2guUHJlc2VuY2UuUHJlc2VuY2VQYXJ0eVVwZGF0ZSqUAwoPSW52aXRlRXZl" + "bnRUeXBlEh0KGUlOVklURV9FVkVOVF9UWVBFX1VOS05PV04QABIhCh1JTlZJ" + "VEVfRVZFTlRfVFlQRV9JTlZJVEVfU0VOVBABEiEKHUlOVklURV9FVkVOVF9U" + "WVBFX1JFVk9LRV9TRU5UEAISJQohSU5WSVRFX0VWRU5UX1RZUEVfSU5WSVRF" + "X1JFQ0VJVkVEEAMSJQohSU5WSVRFX0VWRU5UX1RZUEVfUkVWT0tFX1JFQ0VJ" + "VkVEEAQSJQohSU5WSVRFX0VWRU5UX1RZUEVfSU5WSVRFX0FDQ0VQVEVEEAUS" + "JQohSU5WSVRFX0VWRU5UX1RZUEVfSU5WSVRFX1JFSkVDVEVEEAYSLQopSU5W" + "SVRFX0VWRU5UX1RZUEVfQUNDRVBUX1NVQ0NFU1NfUkVDRUlWRUQQBxIqCiZJ" + "TlZJVEVfRVZFTlRfVFlQRV9BQ0NFUFRfRkFJTF9SRUNFSVZFRBAIEiUKIUlO" + "VklURV9FVkVOVF9UWVBFX1JFSkVDVF9SRUNFSVZFRBAJKtwBCh9QcmVzZW5j" + "ZVNlc3Npb25NZW1iZXJVcGRhdGVUeXBlEiwKKFBSRVNFTkNFX1NFU1NJT05f" + "TUVNQkVSX1VQREFURV9UWVBFX05PTkUQABIrCidQUkVTRU5DRV9TRVNTSU9O" + "X01FTUJFUl9VUERBVEVfVFlQRV9BREQQARIuCipQUkVTRU5DRV9TRVNTSU9O" + "X01FTUJFUl9VUERBVEVfVFlQRV9SRU1PVkUQAhIuCipQUkVTRU5DRV9TRVNT" + "SU9OX01FTUJFUl9VUERBVEVfVFlQRV9VUERBVEUQAyqBAgoXUHJlc2VuY2VQ" + "YXJ0eVVwZGF0ZVR5cGUSIwofUFJFU0VOQ0VfUEFSVFlfVVBEQVRFX1RZUEVf" + "Tk9ORRAAEiEKHVBSRVNFTkNFX1BBUlRZX1VQREFURV9UWVBFX0lEEAESIwof" + "UFJFU0VOQ0VfUEFSVFlfVVBEQVRFX1RZUEVfREFUQRACEicKI1BSRVNFTkNF" + "X1BBUlRZX1VQREFURV9UWVBFX1NFVFRJTkdTEAQSJgoiUFJFU0VOQ0VfUEFS" + "VFlfVVBEQVRFX1RZUEVfTUVNQkVSUxAIEigKJFBSRVNFTkNFX1BBUlRZX1VQ" + "REFURV9UWVBFX0pPSU5fQ09ERRAQKtIBCh1QcmVzZW5jZVBhcnR5TWVtYmVy" + "VXBkYXRlVHlwZRIqCiZQUkVTRU5DRV9QQVJUWV9NRU1CRVJfVVBEQVRFX1RZ" + "UEVfTk9ORRAAEikKJVBSRVNFTkNFX1BBUlRZX01FTUJFUl9VUERBVEVfVFlQ" + "RV9BREQQARIsCihQUkVTRU5DRV9QQVJUWV9NRU1CRVJfVVBEQVRFX1RZUEVf" + "UkVNT1ZFEAISLAooUFJFU0VOQ0VfUEFSVFlfTUVNQkVSX1VQREFURV9UWVBF" + "X1VQREFURRADYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[5]
		{
			PartyStatusReflection.Descriptor,
			MatchmakeStatusReflection.Descriptor,
			InviteDataReflection.Descriptor,
			ErrorCodeReflection.Descriptor,
			NullableReflection.Descriptor
		}, new GeneratedClrTypeInfo(new Type[4]
		{
			typeof(InviteEventType),
			typeof(PresenceSessionMemberUpdateType),
			typeof(PresencePartyUpdateType),
			typeof(PresencePartyMemberUpdateType)
		}, null, new GeneratedClrTypeInfo[12]
		{
			new GeneratedClrTypeInfo(typeof(InviteEvent), InviteEvent.Parser, new string[3] { "EventType", "Data", "SequenceId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(LongOperationResultUpdate), LongOperationResultUpdate.Parser, new string[2] { "CorrelationId", "ResultCode" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresenceUserUpdate), PresenceUserUpdate.Parser, new string[2] { "InviteEvents", "LongOperationResult" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresenceSessionMemberUpdate), PresenceSessionMemberUpdate.Parser, new string[2] { "UpdateType", "MemberData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresenceSessionQueueData), PresenceSessionQueueData.Parser, new string[2] { "PlayListId", "QueueVariants" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresenceSessionGameData), PresenceSessionGameData.Parser, new string[6] { "PlayListId", "Variants", "Settings", "Data", "MembersUpdate", "SCManagedContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresenceSessionUpdate), PresenceSessionUpdate.Parser, new string[4] { "Id", "State", "QueueData", "GameData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresencePartyUpdate), PresencePartyUpdate.Parser, new string[5] { "Id", "Data", "Settings", "Members", "JoinCode" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresencePartyMemberUpdate), PresencePartyMemberUpdate.Parser, new string[2] { "UpdateType", "Member" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresenceUserUpdateVersion), PresenceUserUpdateVersion.Parser, new string[2] { "Version", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresenceSessionUpdateVersion), PresenceSessionUpdateVersion.Parser, new string[2] { "Version", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PresencePartyUpdateVersion), PresencePartyUpdateVersion.Parser, new string[2] { "Version", "Update" }, null, null, null, null)
		}));
	}
}
