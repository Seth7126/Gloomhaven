using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public static class MatchmakeStatusReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static MatchmakeStatusReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Ch5QcmVzZW5jZS9NYXRjaG1ha2VTdGF0dXMucHJvdG8SEkh5ZHJhLkFwaS5Q" + "cmVzZW5jZSIkCgZEQ1BpbmcSDAoEbmFtZRgBIAEoCRIMCgRwaW5nGAIgASgF" + "IisKDVF1ZXVlVmFyaWFudHMSCgoCaWQYASABKAkSDgoGdmFsdWVzGAIgAygJ" + "IsMBChhNYXRjaG1ha2VTZXNzaW9uU2V0dGluZ3MSMgoDamlwGAEgASgOMiUu" + "SHlkcmEuQXBpLlByZXNlbmNlLk1hdGNobWFrZUpJUFN0YXRlEhMKC21heF9w" + "bGF5ZXJzGAIgASgFEkQKD2pvaW5fZGVsZWdhdGlvbhgDIAEoDjIrLkh5ZHJh" + "LkFwaS5QcmVzZW5jZS5NYXRjaG1ha2VKb2luRGVsZWdhdGlvbhIYChBhbGxv" + "d2VkX3VzZXJfaWRzGAQgAygJIlkKF01hdGNobWFrZVNlc3Npb25PcHRpb25z" + "EhMKC3BsYXlsaXN0X2lkGAEgASgJEikKBXBpbmdzGAIgAygLMhouSHlkcmEu" + "QXBpLlByZXNlbmNlLkRDUGluZyLJAQoWTWF0Y2htYWtlQ3JlYXRlT3B0aW9u" + "cxI8CgdvcHRpb25zGAEgASgLMisuSHlkcmEuQXBpLlByZXNlbmNlLk1hdGNo" + "bWFrZVNlc3Npb25PcHRpb25zEj4KCHNldHRpbmdzGAIgASgLMiwuSHlkcmEu" + "QXBpLlByZXNlbmNlLk1hdGNobWFrZVNlc3Npb25TZXR0aW5ncxIxCgh2YXJp" + "YW50cxgDIAMoCzIfLkh5ZHJhLkFwaS5QcmVzZW5jZS5HYW1lVmFyaWFudCKL" + "AQoWTWF0Y2htYWtlU2VhcmNoT3B0aW9ucxI8CgdvcHRpb25zGAEgASgLMisu" + "SHlkcmEuQXBpLlByZXNlbmNlLk1hdGNobWFrZVNlc3Npb25PcHRpb25zEjMK" + "CHZhcmlhbnRzGAIgAygLMiEuSHlkcmEuQXBpLlByZXNlbmNlLlF1ZXVlVmFy" + "aWFudHMiKAoLR2FtZVZhcmlhbnQSCgoCaWQYASABKAkSDQoFdmFsdWUYAiAB" + "KAkinwEKDVNlc3Npb25NZW1iZXISDwoHdXNlcl9pZBgBIAEoCRIQCghpc19v" + "d25lchgCIAEoCBIMCgRkYXRhGAMgASgJEhMKC3N0YXRpY19kYXRhGAQgASgJ" + "Eg4KBnJhdGluZxgFIAEoAhIVCg1zb3J0aW5nX2luZGV4GAYgASgFEhAKCGdy" + "b3VwX2lkGAcgASgJEg8KB3RlYW1faWQYCCABKAUi2gEKD0dhbWVTZXNzaW9u" + "RGF0YRIKCgJpZBgBIAEoCRI+CghzZXR0aW5ncxgCIAEoCzIsLkh5ZHJhLkFw" + "aS5QcmVzZW5jZS5NYXRjaG1ha2VTZXNzaW9uU2V0dGluZ3MSMQoIdmFyaWFu" + "dHMYAyADKAsyHy5IeWRyYS5BcGkuUHJlc2VuY2UuR2FtZVZhcmlhbnQSDAoE" + "ZGF0YRgEIAEoCRI6Cg9zZXNzaW9uX21lbWJlcnMYBSADKAsyIS5IeWRyYS5B" + "cGkuUHJlc2VuY2UuU2Vzc2lvbk1lbWJlciJaCg1HYW1lU2Vzc2lvbklkEgoK" + "AmlkGAEgASgJEj0KBnJlYXNvbhgCIAEoDjItLkh5ZHJhLkFwaS5QcmVzZW5j" + "ZS5HYW1lU2Vzc2lvbklkQ2hhbmdlUmVhc29uIqkBCg9NYXRjaG1ha2VTdGF0" + "dXMSLQoCaWQYASABKAsyIS5IeWRyYS5BcGkuUHJlc2VuY2UuR2FtZVNlc3Np" + "b25JZBIxCgVzdGF0ZRgCIAEoDjIiLkh5ZHJhLkFwaS5QcmVzZW5jZS5NYXRj" + "aG1ha2VTdGF0ZRI0CgdzZXNzaW9uGAMgASgLMiMuSHlkcmEuQXBpLlByZXNl" + "bmNlLkdhbWVTZXNzaW9uRGF0YSKXAQoQVXNlclByZXNlbmNlRGF0YRIPCgd1" + "c2VyX2lkGAEgASgJEjQKB3Nlc3Npb24YAiABKAsyIy5IeWRyYS5BcGkuUHJl" + "c2VuY2UuR2FtZVNlc3Npb25EYXRhEhEKCWlzX29ubGluZRgDIAEoCBIUCgxp" + "c19pbnZpdGFibGUYBCABKAgSEwoLc3RhdGljX2RhdGEYBSABKAkqXwoOTWF0" + "Y2htYWtlU3RhdGUSGAoUTUFUQ0hNQUtFX1NUQVRFX05PTkUQABIZChVNQVRD" + "SE1BS0VfU1RBVEVfUVVFVUUQARIYChRNQVRDSE1BS0VfU1RBVEVfR0FNRRAC" + "KlYKEU1hdGNobWFrZUpJUFN0YXRlEiAKHE1BVENITUFLRV9KSVBfU1RBVEVf" + "RElTQUJMRUQQABIfChtNQVRDSE1BS0VfSklQX1NUQVRFX0VOQUJMRUQQASqW" + "AQoXTWF0Y2htYWtlSm9pbkRlbGVnYXRpb24SJgoiTUFUQ0hNQUtFX0pPSU5f" + "REVMRUdBVElPTl9ESVNBQkxFRBAAEiYKIk1BVENITUFLRV9KT0lOX0RFTEVH" + "QVRJT05fRVZFUllPTkUQARIrCidNQVRDSE1BS0VfSk9JTl9ERUxFR0FUSU9O" + "X0FMTE9XRURfVVNFUlMQAirtBAoZR2FtZVNlc3Npb25JZENoYW5nZVJlYXNv" + "bhImCiJHQU1FX1NFU1NJT05fSURfQ0hBTkdFX1JFQVNPTl9OT05FEAASJgoi" + "R0FNRV9TRVNTSU9OX0lEX0NIQU5HRV9SRUFTT05fSk9JThABEicKI0dBTUVf" + "U0VTU0lPTl9JRF9DSEFOR0VfUkVBU09OX0xFQVZFEAISJgoiR0FNRV9TRVNT" + "SU9OX0lEX0NIQU5HRV9SRUFTT05fS0lDSxADEikKJUdBTUVfU0VTU0lPTl9J" + "RF9DSEFOR0VfUkVBU09OX0RJU0JBTkQQBBI5CjVHQU1FX1NFU1NJT05fSURf" + "Q0hBTkdFX1JFQVNPTl9TRVNTSU9OX0ZJTklTSEVEX05PUk1BTBAFEkIKPkdB" + "TUVfU0VTU0lPTl9JRF9DSEFOR0VfUkVBU09OX1NFU1NJT05fRklOSVNIRURf" + "Tk9fTUFUQ0hJTkdfRFNNEAYSQgo+R0FNRV9TRVNTSU9OX0lEX0NIQU5HRV9S" + "RUFTT05fU0VTU0lPTl9GSU5JU0hFRF9QRU5ESU5HX1RJTUVPVVQQBxI/CjtH" + "QU1FX1NFU1NJT05fSURfQ0hBTkdFX1JFQVNPTl9TRVNTSU9OX0ZJTklTSEVE" + "X05PX1NMT1RTX0RTTRAIEj4KOkdBTUVfU0VTU0lPTl9JRF9DSEFOR0VfUkVB" + "U09OX1NFU1NJT05fRklOSVNIRURfTk9fUFJPVklERVIQCRJACjxHQU1FX1NF" + "U1NJT05fSURfQ0hBTkdFX1JFQVNPTl9TRVNTSU9OX0ZJTklTSEVEX1RJTUVP" + "VVRfUVVFVUUQCmIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[4]
		{
			typeof(MatchmakeState),
			typeof(MatchmakeJIPState),
			typeof(MatchmakeJoinDelegation),
			typeof(GameSessionIdChangeReason)
		}, null, new GeneratedClrTypeInfo[12]
		{
			new GeneratedClrTypeInfo(typeof(DCPing), DCPing.Parser, new string[2] { "Name", "Ping" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(QueueVariants), QueueVariants.Parser, new string[2] { "Id", "Values" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionSettings), MatchmakeSessionSettings.Parser, new string[4] { "Jip", "MaxPlayers", "JoinDelegation", "AllowedUserIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSessionOptions), MatchmakeSessionOptions.Parser, new string[2] { "PlaylistId", "Pings" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeCreateOptions), MatchmakeCreateOptions.Parser, new string[3] { "Options", "Settings", "Variants" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeSearchOptions), MatchmakeSearchOptions.Parser, new string[2] { "Options", "Variants" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GameVariant), GameVariant.Parser, new string[2] { "Id", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionMember), SessionMember.Parser, new string[8] { "UserId", "IsOwner", "Data", "StaticData", "Rating", "SortingIndex", "GroupId", "TeamId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GameSessionData), GameSessionData.Parser, new string[5] { "Id", "Settings", "Variants", "Data", "SessionMembers" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GameSessionId), GameSessionId.Parser, new string[2] { "Id", "Reason" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MatchmakeStatus), MatchmakeStatus.Parser, new string[3] { "Id", "State", "Session" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserPresenceData), UserPresenceData.Parser, new string[5] { "UserId", "Session", "IsOnline", "IsInvitable", "StaticData" }, null, null, null, null)
		}));
	}
}
