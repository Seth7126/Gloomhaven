using System;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.Push;

namespace Hydra.Api.Presence;

public static class PresenceServiceContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PresenceServiceContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CidQcmVzZW5jZS9QcmVzZW5jZVNlcnZpY2VDb250cmFjdHMucHJvdG8SEkh5" + "ZHJhLkFwaS5QcmVzZW5jZRoaUHJlc2VuY2UvUGFydHlTdGF0dXMucHJvdG8a" + "HlByZXNlbmNlL01hdGNobWFrZVN0YXR1cy5wcm90bxoZUHJlc2VuY2UvSW52" + "aXRlRGF0YS5wcm90bxoZQ29udGV4dC9Vc2VyQ29udGV4dC5wcm90bxoUUHVz" + "aC9QdXNoVG9rZW4ucHJvdG8aNEVuZHBvaW50RGlzcGF0Y2hlci9FbmRwb2lu" + "dERpc3BhdGNoZXJDb250cmFjdHMucHJvdG8isAEKDkNvbm5lY3RSZXF1ZXN0" + "Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUu" + "Q29udGV4dC5Vc2VyQ29udGV4dBItCgpwdXNoX3Rva2VuGAIgASgLMhkuSHlk" + "cmEuQXBpLlB1c2guUHVzaFRva2VuEhYKDmNsaWVudF92ZXJzaW9uGAMgASgJ" + "EhcKD3N0YXRpY19wcm9wZXJ0eRgEIAEoCSIRCg9Db25uZWN0UmVzcG9uc2Ui" + "UwoRRGlzY29ubmVjdFJlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJh" + "LkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0IhQKEkRp" + "c2Nvbm5lY3RSZXNwb25zZSJoChdHZXRVc2Vyc1ByZXNlbmNlUmVxdWVzdBI+" + "Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNv" + "bnRleHQuVXNlckNvbnRleHQSDQoFdXNlcnMYAiADKAkiTgoYR2V0VXNlcnNQ" + "cmVzZW5jZVJlc3BvbnNlEjIKBGRhdGEYASADKAsyJC5IeWRyYS5BcGkuUHJl" + "c2VuY2UuVXNlclByZXNlbmNlRGF0YSKLAQoSUGFydHlDcmVhdGVSZXF1ZXN0" + "Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUu" + "Q29udGV4dC5Vc2VyQ29udGV4dBI1CgZwYXJhbXMYAiABKAsyJS5IeWRyYS5B" + "cGkuUHJlc2VuY2UuUGFydHlDcmVhdGVQYXJhbXMiFQoTUGFydHlDcmVhdGVS" + "ZXNwb25zZSKOAQoXUGFydHlTZXRTZXR0aW5nc1JlcXVlc3QSPgoHY29udGV4" + "dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVz" + "ZXJDb250ZXh0EjMKCHNldHRpbmdzGAIgASgLMiEuSHlkcmEuQXBpLlByZXNl" + "bmNlLlBhcnR5U2V0dGluZ3MiGgoYUGFydHlTZXRTZXR0aW5nc1Jlc3BvbnNl" + "ImMKE1BhcnR5U2V0RGF0YVJlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5" + "ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0EgwK" + "BGRhdGEYAiABKAkiFgoUUGFydHlTZXREYXRhUmVzcG9uc2UicAoZUGFydHlT" + "ZXRNZW1iZXJEYXRhUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEu" + "QXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQSEwoLbWVt" + "YmVyX2RhdGEYAiABKAkiHAoaUGFydHlTZXRNZW1iZXJEYXRhUmVzcG9uc2Ui" + "bAoWUGFydHlJbnZpdGVTZW5kUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgLMi0u" + "SHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQS" + "EgoKdXNlcl9pZF90bxgCIAEoCSIZChdQYXJ0eUludml0ZVNlbmRSZXNwb25z" + "ZSJuChhQYXJ0eUludml0ZVJldm9rZVJlcXVlc3QSPgoHY29udGV4dBgBIAEo" + "CzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250" + "ZXh0EhIKCnVzZXJfaWRfdG8YAiABKAkiGwoZUGFydHlJbnZpdGVSZXZva2VS" + "ZXNwb25zZSKPAQoYUGFydHlJbnZpdGVBY2NlcHRSZXF1ZXN0Ej4KB2NvbnRl" + "eHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5V" + "c2VyQ29udGV4dBIzCgtpbnZpdGVfZGF0YRgCIAEoCzIeLkh5ZHJhLkFwaS5Q" + "cmVzZW5jZS5JbnZpdGVEYXRhIhsKGVBhcnR5SW52aXRlQWNjZXB0UmVzcG9u" + "c2UijwEKGFBhcnR5SW52aXRlUmVqZWN0UmVxdWVzdBI+Cgdjb250ZXh0GAEg" + "ASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNv" + "bnRleHQSMwoLaW52aXRlX2RhdGEYAiABKAsyHi5IeWRyYS5BcGkuUHJlc2Vu" + "Y2UuSW52aXRlRGF0YSIbChlQYXJ0eUludml0ZVJlamVjdFJlc3BvbnNlImYK" + "EFBhcnR5Sm9pblJlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFw" + "aS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0EhIKCnVzZXJf" + "aWRfdG8YAiABKAkiEwoRUGFydHlKb2luUmVzcG9uc2UiUwoRUGFydHlMZWF2" + "ZVJlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0" + "cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0IhQKElBhcnR5TGVhdmVSZXNw" + "b25zZSJsChlQYXJ0eVJlbW92ZU1lbWJlcnNSZXF1ZXN0Ej4KB2NvbnRleHQY" + "ASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2Vy" + "Q29udGV4dBIPCgd1c2VyX2lkGAIgASgJIhwKGlBhcnR5UmVtb3ZlTWVtYmVy" + "c1Jlc3BvbnNlImcKFFBhcnR5U2V0T3duZXJSZXF1ZXN0Ej4KB2NvbnRleHQY" + "ASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2Vy" + "Q29udGV4dBIPCgd1c2VyX2lkGAIgASgJIhcKFVBhcnR5U2V0T3duZXJSZXNw" + "b25zZSJVChNQYXJ0eURpc2JhbmRSZXF1ZXN0Ej4KB2NvbnRleHQYASABKAsy" + "LS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4" + "dCIWChRQYXJ0eURpc2JhbmRSZXNwb25zZSJeChxQYXJ0eUdlbmVyYXRlSm9p" + "bkNvZGVSZXF1ZXN0Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5m" + "cmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dCIfCh1QYXJ0eUdlbmVy" + "YXRlSm9pbkNvZGVSZXNwb25zZSJbChlQYXJ0eUNsZWFySm9pbkNvZGVSZXF1" + "ZXN0Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1" + "cmUuQ29udGV4dC5Vc2VyQ29udGV4dCIcChpQYXJ0eUNsZWFySm9pbkNvZGVS" + "ZXNwb25zZSJsChdQYXJ0eVVzZUpvaW5Db2RlUmVxdWVzdBI+Cgdjb250ZXh0" + "GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNl" + "ckNvbnRleHQSEQoJam9pbl9jb2RlGAIgASgJIhoKGFBhcnR5VXNlSm9pbkNv" + "ZGVSZXNwb25zZSKUAQoVTWF0Y2htYWtlU3RhcnRSZXF1ZXN0Ej4KB2NvbnRl" + "eHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5V" + "c2VyQ29udGV4dBI7CgdvcHRpb25zGAIgASgLMiouSHlkcmEuQXBpLlByZXNl" + "bmNlLk1hdGNobWFrZVNlYXJjaE9wdGlvbnMiGAoWTWF0Y2htYWtlU3RhcnRS" + "ZXNwb25zZSJWChRNYXRjaG1ha2VTdG9wUmVxdWVzdBI+Cgdjb250ZXh0GAEg" + "ASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNv" + "bnRleHQiLwoVTWF0Y2htYWtlU3RvcFJlc3BvbnNlEhYKDmNvcnJlbGF0aW9u" + "X2lkGAEgASgJIpwBCh1NYXRjaG1ha2VTZXNzaW9uQ3JlYXRlUmVxdWVzdBI+" + "Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNv" + "bnRleHQuVXNlckNvbnRleHQSOwoHb3B0aW9ucxgCIAEoCzIqLkh5ZHJhLkFw" + "aS5QcmVzZW5jZS5NYXRjaG1ha2VDcmVhdGVPcHRpb25zIjgKHk1hdGNobWFr" + "ZVNlc3Npb25DcmVhdGVSZXNwb25zZRIWCg5jb3JyZWxhdGlvbl9pZBgBIAEo" + "CSKkAQoiTWF0Y2htYWtlU2Vzc2lvblNldFNldHRpbmdzUmVxdWVzdBI+Cgdj" + "b250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRl" + "eHQuVXNlckNvbnRleHQSPgoIc2V0dGluZ3MYAiABKAsyLC5IeWRyYS5BcGku" + "UHJlc2VuY2UuTWF0Y2htYWtlU2Vzc2lvblNldHRpbmdzIiUKI01hdGNobWFr" + "ZVNlc3Npb25TZXRTZXR0aW5nc1Jlc3BvbnNlIpcBCiJNYXRjaG1ha2VTZXNz" + "aW9uU2V0VmFyaWFudHNSZXF1ZXN0Ej4KB2NvbnRleHQYASABKAsyLS5IeWRy" + "YS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dBIxCgh2" + "YXJpYW50cxgCIAMoCzIfLkh5ZHJhLkFwaS5QcmVzZW5jZS5HYW1lVmFyaWFu" + "dCIlCiNNYXRjaG1ha2VTZXNzaW9uU2V0VmFyaWFudHNSZXNwb25zZSJuCh5N" + "YXRjaG1ha2VTZXNzaW9uU2V0RGF0YVJlcXVlc3QSPgoHY29udGV4dBgBIAEo" + "CzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250" + "ZXh0EgwKBGRhdGEYAiABKAkiIQofTWF0Y2htYWtlU2Vzc2lvblNldERhdGFS" + "ZXNwb25zZSJ7CiRNYXRjaG1ha2VTZXNzaW9uU2V0TWVtYmVyRGF0YVJlcXVl" + "c3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVy" + "ZS5Db250ZXh0LlVzZXJDb250ZXh0EhMKC21lbWJlcl9kYXRhGAIgASgJIicK" + "JU1hdGNobWFrZVNlc3Npb25TZXRNZW1iZXJEYXRhUmVzcG9uc2UicQobTWF0" + "Y2htYWtlU2Vzc2lvbkpvaW5SZXF1ZXN0Ej4KB2NvbnRleHQYASABKAsyLS5I" + "eWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dBIS" + "CgpzZXNzaW9uX2lkGAIgASgJIjYKHE1hdGNobWFrZVNlc3Npb25Kb2luUmVz" + "cG9uc2USFgoOY29ycmVsYXRpb25faWQYASABKAkiXgocTWF0Y2htYWtlU2Vz" + "c2lvbkxlYXZlUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEuQXBp" + "LkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQiNwodTWF0Y2ht" + "YWtlU2Vzc2lvbkxlYXZlUmVzcG9uc2USFgoOY29ycmVsYXRpb25faWQYASAB" + "KAkidwokTWF0Y2htYWtlU2Vzc2lvblJlbW92ZU1lbWJlcnNSZXF1ZXN0Ej4K" + "B2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29u" + "dGV4dC5Vc2VyQ29udGV4dBIPCgd1c2VyX2lkGAIgAygJIicKJU1hdGNobWFr" + "ZVNlc3Npb25SZW1vdmVNZW1iZXJzUmVzcG9uc2UicgofTWF0Y2htYWtlU2Vz" + "c2lvblNldE93bmVyUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEu" + "QXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQSDwoHdXNl" + "cl9pZBgCIAEoCSIiCiBNYXRjaG1ha2VTZXNzaW9uU2V0T3duZXJSZXNwb25z" + "ZSJwChdEYXRhQ2VudGVyUGluZ0VuZHBvaW50cxIWCg5kYXRhX2NlbnRlcl9p" + "ZBgBIAEoCRI9CgllbmRwb2ludHMYAiADKAsyKi5IeWRyYS5BcGkuRW5kcG9p" + "bnREaXNwYXRjaGVyLkVuZHBvaW50SW5mbyIjCiFHZXREYXRhQ2VudGVyUGlu" + "Z0VuZHBvaW50c1JlcXVlc3QidQoiR2V0RGF0YUNlbnRlclBpbmdFbmRwb2lu" + "dHNSZXNwb25zZRJPChpkYXRhX2NlbnRlcl9waW5nX2VuZHBvaW50cxgBIAMo" + "CzIrLkh5ZHJhLkFwaS5QcmVzZW5jZS5EYXRhQ2VudGVyUGluZ0VuZHBvaW50" + "c2IGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[6]
		{
			PartyStatusReflection.Descriptor,
			MatchmakeStatusReflection.Descriptor,
			InviteDataReflection.Descriptor,
			UserContextReflection.Descriptor,
			PushTokenReflection.Descriptor,
			EndpointDispatcherContractsReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[63]
		{
			new GeneratedClrTypeInfo(typeof(ConnectRequest), ConnectRequest.Parser, new string[4] { "Context", "PushToken", "ClientVersion", "StaticProperty" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConnectResponse), ConnectResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DisconnectRequest), DisconnectRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DisconnectResponse), DisconnectResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetUsersPresenceRequest), GetUsersPresenceRequest.Parser, new string[2] { "Context", "Users" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetUsersPresenceResponse), GetUsersPresenceResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyCreateRequest), PartyCreateRequest.Parser, new string[2] { "Context", "Params" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyCreateResponse), PartyCreateResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetSettingsRequest), PartySetSettingsRequest.Parser, new string[2] { "Context", "Settings" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetSettingsResponse), PartySetSettingsResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetDataRequest), PartySetDataRequest.Parser, new string[2] { "Context", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetDataResponse), PartySetDataResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetMemberDataRequest), PartySetMemberDataRequest.Parser, new string[2] { "Context", "MemberData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetMemberDataResponse), PartySetMemberDataResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteSendRequest), PartyInviteSendRequest.Parser, new string[2] { "Context", "UserIdTo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteSendResponse), PartyInviteSendResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteRevokeRequest), PartyInviteRevokeRequest.Parser, new string[2] { "Context", "UserIdTo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteRevokeResponse), PartyInviteRevokeResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteAcceptRequest), PartyInviteAcceptRequest.Parser, new string[2] { "Context", "InviteData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteAcceptResponse), PartyInviteAcceptResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteRejectRequest), PartyInviteRejectRequest.Parser, new string[2] { "Context", "InviteData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyInviteRejectResponse), PartyInviteRejectResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyJoinRequest), PartyJoinRequest.Parser, new string[2] { "Context", "UserIdTo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyJoinResponse), PartyJoinResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyLeaveRequest), PartyLeaveRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyLeaveResponse), PartyLeaveResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyRemoveMembersRequest), PartyRemoveMembersRequest.Parser, new string[2] { "Context", "UserId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyRemoveMembersResponse), PartyRemoveMembersResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetOwnerRequest), PartySetOwnerRequest.Parser, new string[2] { "Context", "UserId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySetOwnerResponse), PartySetOwnerResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyDisbandRequest), PartyDisbandRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyDisbandResponse), PartyDisbandResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyGenerateJoinCodeRequest), PartyGenerateJoinCodeRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyGenerateJoinCodeResponse), PartyGenerateJoinCodeResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyClearJoinCodeRequest), PartyClearJoinCodeRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyClearJoinCodeResponse), PartyClearJoinCodeResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyUseJoinCodeRequest), PartyUseJoinCodeRequest.Parser, new string[2] { "Context", "JoinCode" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyUseJoinCodeResponse), PartyUseJoinCodeResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeStartRequest), MatchmakeStartRequest.Parser, new string[2] { "Context", "Options" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeStartResponse), MatchmakeStartResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeStopRequest), MatchmakeStopRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeStopResponse), MatchmakeStopResponse.Parser, new string[1] { "CorrelationId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionCreateRequest), MatchmakeSessionCreateRequest.Parser, new string[2] { "Context", "Options" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionCreateResponse), MatchmakeSessionCreateResponse.Parser, new string[1] { "CorrelationId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetSettingsRequest), MatchmakeSessionSetSettingsRequest.Parser, new string[2] { "Context", "Settings" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetSettingsResponse), MatchmakeSessionSetSettingsResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetVariantsRequest), MatchmakeSessionSetVariantsRequest.Parser, new string[2] { "Context", "Variants" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetVariantsResponse), MatchmakeSessionSetVariantsResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetDataRequest), MatchmakeSessionSetDataRequest.Parser, new string[2] { "Context", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetDataResponse), MatchmakeSessionSetDataResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetMemberDataRequest), MatchmakeSessionSetMemberDataRequest.Parser, new string[2] { "Context", "MemberData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetMemberDataResponse), MatchmakeSessionSetMemberDataResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionJoinRequest), MatchmakeSessionJoinRequest.Parser, new string[2] { "Context", "SessionId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionJoinResponse), MatchmakeSessionJoinResponse.Parser, new string[1] { "CorrelationId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionLeaveRequest), MatchmakeSessionLeaveRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionLeaveResponse), MatchmakeSessionLeaveResponse.Parser, new string[1] { "CorrelationId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionRemoveMembersRequest), MatchmakeSessionRemoveMembersRequest.Parser, new string[2] { "Context", "UserId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionRemoveMembersResponse), MatchmakeSessionRemoveMembersResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetOwnerRequest), MatchmakeSessionSetOwnerRequest.Parser, new string[2] { "Context", "UserId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSetOwnerResponse), MatchmakeSessionSetOwnerResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DataCenterPingEndpoints), DataCenterPingEndpoints.Parser, new string[2] { "DataCenterId", "Endpoints" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataCenterPingEndpointsRequest), GetDataCenterPingEndpointsRequest.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataCenterPingEndpointsResponse), GetDataCenterPingEndpointsResponse.Parser, new string[1] { "DataCenterPingEndpoints" }, null, null, null, null)
		}));
	}
}
