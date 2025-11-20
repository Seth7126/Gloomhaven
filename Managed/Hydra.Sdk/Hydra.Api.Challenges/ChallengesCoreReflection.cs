using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public static class ChallengesCoreReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ChallengesCoreReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Ch9DaGFsbGVuZ2VzL0NoYWxsZW5nZXNDb3JlLnByb3RvEhRIeWRyYS5BcGku" + "Q2hhbGxlbmdlcyK0AgofVXNlckNoYWxsZW5nZXNJbmNyZW1lbnRhbFVwZGF0" + "ZRI+Cgt1cGRhdGVfdHlwZRgBIAEoDjIpLkh5ZHJhLkFwaS5DaGFsbGVuZ2Vz" + "LkNoYWxsZW5nZVVwZGF0ZVR5cGUSMwoKY2hhbGxlbmdlcxgCIAMoCzIfLkh5" + "ZHJhLkFwaS5DaGFsbGVuZ2VzLkNoYWxsZW5nZRIaChJyZW1vdmVkX2NoYWxs" + "ZW5nZXMYAyADKAkSFAoMY3VycmVudF90aWNrGAQgASgEEiMKG2N1cnJlbnRf" + "dGlja19leHBpcmF0aW9uX3NlYxgFIAEoBBJFCg9kYWlseV9zbG90X2luZm8Y" + "BiADKAsyLC5IeWRyYS5BcGkuQ2hhbGxlbmdlcy5DaGFsbGVuZ2VEYWlseVNs" + "b3RJbmZvIrUBChtVc2VyQ2hhbGxlbmdlc1VwZGF0ZVZlcnNpb24SDwoHdmVy" + "c2lvbhgBIAEoBRIfChdmcm9tX2NoYWxsZW5nZXNfdmVyc2lvbhgCIAEoAxId" + "ChV0b19jaGFsbGVuZ2VzX3ZlcnNpb24YAyABKAMSRQoGdXBkYXRlGAQgASgL" + "MjUuSHlkcmEuQXBpLkNoYWxsZW5nZXMuVXNlckNoYWxsZW5nZXNJbmNyZW1l" + "bnRhbFVwZGF0ZSLLAQoSVXNlckNoYWxsZW5nZXNJbmZvEjMKCmNoYWxsZW5n" + "ZXMYASADKAsyHy5IeWRyYS5BcGkuQ2hhbGxlbmdlcy5DaGFsbGVuZ2USFAoM" + "Y3VycmVudF90aWNrGAIgASgEEiMKG2N1cnJlbnRfdGlja19leHBpcmF0aW9u" + "X3NlYxgDIAEoBBJFCg9kYWlseV9zbG90X2luZm8YBCADKAsyLC5IeWRyYS5B" + "cGkuQ2hhbGxlbmdlcy5DaGFsbGVuZ2VEYWlseVNsb3RJbmZvIvMBCglDaGFs" + "bGVuZ2USFAoMY2hhbGxlbmdlX2lkGAEgASgJEhIKCmRlZmluaXRpb24YAiAB" + "KAkSMQoEdHlwZRgDIAEoDjIjLkh5ZHJhLkFwaS5DaGFsbGVuZ2VzLkNoYWxs" + "ZW5nZVR5cGUSMwoFc3RhdGUYBCABKA4yJC5IeWRyYS5BcGkuQ2hhbGxlbmdl" + "cy5DaGFsbGVuZ2VTdGF0ZRIMCgRzbG90GAUgASgFEgwKBHRpY2sYBiABKA0S" + "OAoIY291bnRlcnMYByADKAsyJi5IeWRyYS5BcGkuQ2hhbGxlbmdlcy5DaGFs" + "bGVuZ2VDb3VudGVyIocBChpDaGFsbGVuZ2VDb3VudGVyRmlsdGVySXRlbRIM" + "CgRuYW1lGAEgASgJEg0KBXZhbHVlGAIgASgJEkwKCW9wZXJhdGlvbhgDIAEo" + "DjI5Lkh5ZHJhLkFwaS5DaGFsbGVuZ2VzLkNoYWxsZW5nZUNvdW50ZXJGaWx0" + "ZXJPcGVyYXRpb25UeXBlImEKEENoYWxsZW5nZUNvdW50ZXISHAoUY2hhbGxl" + "bmdlX2NvdW50ZXJfaWQYASABKAkSDQoFdmFsdWUYAiABKAQSDAoEZ29hbBgD" + "IAEoBBISCgptaWxlc3RvbmVzGAQgAygEIrwBCg5DaGFsbGVuZ2VFdmVudBIS" + "CgpldmVudF9uYW1lGAEgASgJEj8KCW9wZXJhdGlvbhgCIAEoDjIsLkh5ZHJh" + "LkFwaS5DaGFsbGVuZ2VzLkNoYWxsZW5nZU9wZXJhdGlvblR5cGUSRgoMZXZl" + "bnRfZmlsdGVyGAMgAygLMjAuSHlkcmEuQXBpLkNoYWxsZW5nZXMuQ2hhbGxl" + "bmdlQ291bnRlckZpbHRlckl0ZW0SDQoFdmFsdWUYBCABKAQioQEKGkNoYWxs" + "ZW5nZUNvdW50ZXJXaXRoRXZlbnRzEhwKFGNoYWxsZW5nZV9jb3VudGVyX2lk" + "GAEgASgJEjQKBmV2ZW50cxgCIAMoCzIkLkh5ZHJhLkFwaS5DaGFsbGVuZ2Vz" + "LkNoYWxsZW5nZUV2ZW50Eg0KBXZhbHVlGAMgASgEEgwKBGdvYWwYBCABKAQS" + "EgoKbWlsZXN0b25lcxgFIAMoBCJuChZDaGFsbGVuZ2VPcGVyYXRpb25MaXN0" + "Eg8KB3VzZXJfaWQYASABKAkSQwoKb3BlcmF0aW9ucxgCIAMoCzIvLkh5ZHJh" + "LkFwaS5DaGFsbGVuZ2VzLkNoYWxsZW5nZUNvdW50ZXJPcGVyYXRpb24ijgEK" + "GUNoYWxsZW5nZUNvdW50ZXJPcGVyYXRpb24SHAoUY2hhbGxlbmdlX2NvdW50" + "ZXJfaWQYASABKAkSRAoOb3BlcmF0aW9uX3R5cGUYAiABKA4yLC5IeWRyYS5B" + "cGkuQ2hhbGxlbmdlcy5DaGFsbGVuZ2VPcGVyYXRpb25UeXBlEg0KBXZhbHVl" + "GAMgASgEIloKFkNoYWxsZW5nZURhaWx5U2xvdEluZm8SIAoYYXZhaWxhYmxl" + "X3JlY2hhcmdlX2NvdW50GAEgASgFEh4KFnJlbWFpbmluZ19jb29sZG93bl9z" + "ZWMYAiABKAUqoAEKE0NoYWxsZW5nZVVwZGF0ZVR5cGUSHgoaQ0hBTExFTkdF" + "X1VQREFURV9UWVBFX05PTkUQABIeChpDSEFMTEVOR0VfVVBEQVRFX1RZUEVf" + "RlVMTBABEiUKIUNIQUxMRU5HRV9VUERBVEVfVFlQRV9JTkNSRU1FTlRBTBAC" + "EiIKHkNIQUxMRU5HRV9VUERBVEVfVFlQRV9GSUxURVJFRBADKl0KDUNoYWxs" + "ZW5nZVR5cGUSFwoTQ0hBTExFTkdFX1RZUEVfTk9ORRAAEhgKFENIQUxMRU5H" + "RV9UWVBFX0RBSUxZEAESGQoVQ0hBTExFTkdFX1RZUEVfU1RBVElDEAIqowEK" + "DkNoYWxsZW5nZVN0YXRlEhgKFENIQUxMRU5HRV9TVEFURV9OT05FEAASHQoZ" + "Q0hBTExFTkdFX1NUQVRFX0FDVElWQVRFRBABEh0KGUNIQUxMRU5HRV9TVEFU" + "RV9DT01QTEVURUQQAhIbChdDSEFMTEVOR0VfU1RBVEVfRVhQSVJFRBADEhwK" + "GENIQUxMRU5HRV9TVEFURV9SRVdBUkRFRBAEKsEBCiNDaGFsbGVuZ2VDb3Vu" + "dGVyRmlsdGVyT3BlcmF0aW9uVHlwZRIwCixDSEFMTEVOR0VfQ09VTlRFUl9G" + "SUxURVJfT1BFUkFUSU9OX1RZUEVfTk9ORRAAEjEKLUNIQUxMRU5HRV9DT1VO" + "VEVSX0ZJTFRFUl9PUEVSQVRJT05fVFlQRV9FUVVBTBABEjUKMUNIQUxMRU5H" + "RV9DT1VOVEVSX0ZJTFRFUl9PUEVSQVRJT05fVFlQRV9OT1RfRVFVQUwQAiqG" + "AQoWQ2hhbGxlbmdlT3BlcmF0aW9uVHlwZRIhCh1DSEFMTEVOR0VfT1BFUlRB" + "SU9OX1RZUEVfTk9ORRAAEiUKIUNIQUxMRU5HRV9PUEVSVEFJT05fVFlQRV9J" + "TkNSRUFTRRABEiIKHkNIQUxMRU5HRV9PUEVSVEFJT05fVFlQRV9SRVNFVBAC" + "YgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[5]
		{
			typeof(ChallengeUpdateType),
			typeof(ChallengeType),
			typeof(ChallengeState),
			typeof(ChallengeCounterFilterOperationType),
			typeof(ChallengeOperationType)
		}, null, new GeneratedClrTypeInfo[11]
		{
			new GeneratedClrTypeInfo(typeof(UserChallengesIncrementalUpdate), UserChallengesIncrementalUpdate.Parser, new string[6] { "UpdateType", "Challenges", "RemovedChallenges", "CurrentTick", "CurrentTickExpirationSec", "DailySlotInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserChallengesUpdateVersion), UserChallengesUpdateVersion.Parser, new string[4] { "Version", "FromChallengesVersion", "ToChallengesVersion", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserChallengesInfo), UserChallengesInfo.Parser, new string[4] { "Challenges", "CurrentTick", "CurrentTickExpirationSec", "DailySlotInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(Challenge), Challenge.Parser, new string[7] { "ChallengeId", "Definition", "Type", "State", "Slot", "Tick", "Counters" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ChallengeCounterFilterItem), ChallengeCounterFilterItem.Parser, new string[3] { "Name", "Value", "Operation" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ChallengeCounter), ChallengeCounter.Parser, new string[4] { "ChallengeCounterId", "Value", "Goal", "Milestones" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ChallengeEvent), ChallengeEvent.Parser, new string[4] { "EventName", "Operation", "EventFilter", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ChallengeCounterWithEvents), ChallengeCounterWithEvents.Parser, new string[5] { "ChallengeCounterId", "Events", "Value", "Goal", "Milestones" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ChallengeOperationList), ChallengeOperationList.Parser, new string[2] { "UserId", "Operations" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ChallengeCounterOperation), ChallengeCounterOperation.Parser, new string[3] { "ChallengeCounterId", "OperationType", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ChallengeDailySlotInfo), ChallengeDailySlotInfo.Parser, new string[2] { "AvailableRechargeCount", "RemainingCooldownSec" }, null, null, null, null)
		}));
	}
}
