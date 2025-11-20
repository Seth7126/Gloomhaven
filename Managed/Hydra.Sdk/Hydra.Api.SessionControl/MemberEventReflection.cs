using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.SessionControl;

public static class MemberEventReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static MemberEventReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiBTZXNzaW9uQ29udHJvbC9NZW1iZXJFdmVudC5wcm90bxIYSHlkcmEuQXBp" + "LlNlc3Npb25Db250cm9sGiFTZXNzaW9uQ29udHJvbC9LZXlDb250YWluZXIu" + "cHJvdG8aGUNvbnRleHQvVXNlckNvbnRleHQucHJvdG8iRgoKQWNjZXB0RGF0" + "YRI4CghrZXlfaW5mbxgBIAEoCzImLkh5ZHJhLkFwaS5TZXNzaW9uQ29udHJv" + "bC5LZXlDb250YWluZXIiVAoYU2Vzc2lvbk1lbWJlckJhY2tlbmREYXRhEjgK" + "CGtleV9pbmZvGAEgASgLMiYuSHlkcmEuQXBpLlNlc3Npb25Db250cm9sLktl" + "eUNvbnRhaW5lciI+CgpTZXJ2ZXJJbmZvEhcKD2Nvbm5lY3Rpb25faW5mbxgB" + "IAEoCRIXCg9zZXJ2ZXJfcHJvcGVydHkYAiABKAkiuwEKEkFjY2VwdENsaWVu" + "dFJlc3VsdBI2CgZzdGF0dXMYASABKA4yJi5IeWRyYS5BcGkuU2Vzc2lvbkNv" + "bnRyb2wuQWNjZXB0U3RhdHVzEjIKBGRhdGEYAiABKAsyJC5IeWRyYS5BcGku" + "U2Vzc2lvbkNvbnRyb2wuQWNjZXB0RGF0YRI5CgtzZXJ2ZXJfaW5mbxgDIAEo" + "CzIkLkh5ZHJhLkFwaS5TZXNzaW9uQ29udHJvbC5TZXJ2ZXJJbmZvIlgKEVNl" + "cnZlclVzZXJDb250ZXh0EkMKDHVzZXJfY29udGV4dBgBIAEoCzItLkh5ZHJh" + "LkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0IvIBChZT" + "ZXNzaW9uTWVtYmVyRXZlbnREYXRhEkgKE3NlcnZlcl91c2VyX2NvbnRleHQY" + "ASABKAsyKy5IeWRyYS5BcGkuU2Vzc2lvbkNvbnRyb2wuU2VydmVyVXNlckNv" + "bnRleHQSSAoMYmFja2VuZF9kYXRhGAIgASgLMjIuSHlkcmEuQXBpLlNlc3Np" + "b25Db250cm9sLlNlc3Npb25NZW1iZXJCYWNrZW5kRGF0YRJECgpldmVudF90" + "eXBlGAMgASgOMjAuSHlkcmEuQXBpLlNlc3Npb25Db250cm9sLlNlc3Npb25N" + "ZW1iZXJFdmVudFR5cGUiYQoSU2Vzc2lvbk1lbWJlckV2ZW50EgsKA2tleRgB" + "IAEoCRI+CgRkYXRhGAIgASgLMjAuSHlkcmEuQXBpLlNlc3Npb25Db250cm9s" + "LlNlc3Npb25NZW1iZXJFdmVudERhdGEigAEKEk1lbWJlckFjY2VwdFJlc3Vs" + "dBI2CgZzdGF0dXMYASABKA4yJi5IeWRyYS5BcGkuU2Vzc2lvbkNvbnRyb2wu" + "QWNjZXB0U3RhdHVzEjIKBGRhdGEYAiABKAsyJC5IeWRyYS5BcGkuU2Vzc2lv" + "bkNvbnRyb2wuQWNjZXB0RGF0YSJWChxTZXNzaW9uTWVtYmVyRXZlbnRSZXN1" + "bHREYXRhEjYKBnN0YXR1cxgBIAEoDjImLkh5ZHJhLkFwaS5TZXNzaW9uQ29u" + "dHJvbC5BY2NlcHRTdGF0dXMibQoYU2Vzc2lvbk1lbWJlckV2ZW50UmVzdWx0" + "EgsKA2tleRgBIAEoCRJECgRkYXRhGAIgASgLMjYuSHlkcmEuQXBpLlNlc3Np" + "b25Db250cm9sLlNlc3Npb25NZW1iZXJFdmVudFJlc3VsdERhdGEqYQoMQWNj" + "ZXB0U3RhdHVzEhkKFUFDQ0VQVF9TVEFUVVNfUEVORElORxAAEhoKFkFDQ0VQ" + "VF9TVEFUVVNfQUNDRVBURUQQARIaChZBQ0NFUFRfU1RBVFVTX1JFSkVDVEVE" + "EAIq0wEKFlNlc3Npb25NZW1iZXJFdmVudFR5cGUSIgoeU0VTU0lPTl9NRU1C" + "RVJfRVZFTlRfVFlQRV9OT05FEAASIQodU0VTU0lPTl9NRU1CRVJfRVZFTlRf" + "VFlQRV9BREQQARIkCiBTRVNTSU9OX01FTUJFUl9FVkVOVF9UWVBFX1JFTU9W" + "RRACEiYKIlNFU1NJT05fTUVNQkVSX0VWRU5UX1RZUEVfQVdBSVRJTkcQAxIk" + "CiBTRVNTSU9OX01FTUJFUl9FVkVOVF9UWVBFX1JFVFVSThAEKukFChRVc2Vy" + "U2Vzc2lvbkV2ZW50VHlwZRIjCh9VU0VSX1NFU1NJT05fRVZFTlRfVFlQRV9V" + "TktOT1dOEAASKgomVVNFUl9TRVNTSU9OX0VWRU5UX1RZUEVfS1JfUEFSVFlf" + "T1dORVIQARInCiNVU0VSX1NFU1NJT05fRVZFTlRfVFlQRV9LUl9SRUpFQ1RF" + "RBACEiYKIlVTRVJfU0VTU0lPTl9FVkVOVF9UWVBFX0tSX1RJTUVPVVQQAxIk" + "CiBVU0VSX1NFU1NJT05fRVZFTlRfVFlQRV9LUl9DSEVBVBAEEiIKHlVTRVJf" + "U0VTU0lPTl9FVkVOVF9UWVBFX0tSX0FGSxAFEiUKIVVTRVJfU0VTU0lPTl9F" + "VkVOVF9UWVBFX0tSX1JFVEFSRBAGEiwKKFVTRVJfU0VTU0lPTl9FVkVOVF9U" + "WVBFX0tSX1JFQURZX1RJTUVPVVQQBxIjCh9VU0VSX1NFU1NJT05fRVZFTlRf" + "VFlQRV9ESVNCQU5EEAgSLgoqVVNFUl9TRVNTSU9OX0VWRU5UX1RZUEVfU0Nf" + "Tk9fTUFUQ0hJTkdfRFNNEAkSLgoqVVNFUl9TRVNTSU9OX0VWRU5UX1RZUEVf" + "U0NfUEVORElOR19USU1FT1VUEAoSJgoiVVNFUl9TRVNTSU9OX0VWRU5UX1RZ" + "UEVfU0NfVU5LTk9XThALEisKJ1VTRVJfU0VTU0lPTl9FVkVOVF9UWVBFX1ND" + "X05PX1NMT1RTX0RTTRAMEioKJlVTRVJfU0VTU0lPTl9FVkVOVF9UWVBFX1ND" + "X05PX1BST1ZJREVSEA0SLAooVVNFUl9TRVNTSU9OX0VWRU5UX1RZUEVfU0Nf" + "VElNRU9VVF9RVUVVRRAOEisKJ1VTRVJfU0VTU0lPTl9FVkVOVF9UWVBFX1JF" + "TUFUQ0hfRElTQkFORBAPEi8KK1VTRVJfU0VTU0lPTl9FVkVOVF9UWVBFX1NF" + "U1NJT05fSk9JTl9GQUlMRUQQECpSCgZUZWFtSWQSEwoPVEVBTV9JRF9VTktO" + "T1dOEAASDQoJVEVBTV9JRF8xEAESDQoJVEVBTV9JRF8yEAISFQoRVEVBTV9J" + "RF9TUEVDVEFUT1IQA2IGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			KeyContainerReflection.Descriptor,
			UserContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(new Type[4]
		{
			typeof(AcceptStatus),
			typeof(SessionMemberEventType),
			typeof(UserSessionEventType),
			typeof(TeamId)
		}, null, new GeneratedClrTypeInfo[10]
		{
			new GeneratedClrTypeInfo(typeof(AcceptData), AcceptData.Parser, new string[1] { "KeyInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionMemberBackendData), SessionMemberBackendData.Parser, new string[1] { "KeyInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerInfo), ServerInfo.Parser, new string[2] { "ConnectionInfo", "ServerProperty" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(AcceptClientResult), AcceptClientResult.Parser, new string[3] { "Status", "Data", "ServerInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerUserContext), ServerUserContext.Parser, new string[1] { "UserContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionMemberEventData), SessionMemberEventData.Parser, new string[3] { "ServerUserContext", "BackendData", "EventType" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionMemberEvent), SessionMemberEvent.Parser, new string[2] { "Key", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MemberAcceptResult), MemberAcceptResult.Parser, new string[2] { "Status", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionMemberEventResultData), SessionMemberEventResultData.Parser, new string[1] { "Status" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionMemberEventResult), SessionMemberEventResult.Parser, new string[2] { "Key", "Data" }, null, null, null, null)
		}));
	}
}
