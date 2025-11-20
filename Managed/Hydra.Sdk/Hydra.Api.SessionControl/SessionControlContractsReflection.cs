using System;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.Nullable;

namespace Hydra.Api.SessionControl;

public static class SessionControlContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static SessionControlContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CixTZXNzaW9uQ29udHJvbC9TZXNzaW9uQ29udHJvbENvbnRyYWN0cy5wcm90" + "bxIYSHlkcmEuQXBpLlNlc3Npb25Db250cm9sGjRFbmRwb2ludERpc3BhdGNo" + "ZXIvRW5kcG9pbnREaXNwYXRjaGVyQ29udHJhY3RzLnByb3RvGiBTZXNzaW9u" + "Q29udHJvbC9NZW1iZXJFdmVudC5wcm90bxoZQ29udGV4dC9Vc2VyQ29udGV4" + "dC5wcm90bxobQ29udGV4dC9TZXJ2ZXJDb250ZXh0LnByb3RvGhdOdWxsYWJs" + "ZS9OdWxsYWJsZS5wcm90byKgAQoUQ3JlYXRlU2Vzc2lvblJlcXVlc3QSQwoM" + "dXNlcl9jb250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJl" + "LkNvbnRleHQuVXNlckNvbnRleHQSFgoOZGF0YV9jZW50ZXJfaWQYAiABKAkS" + "FgoOY2xpZW50X3ZlcnNpb24YAyABKAkSEwoLc2VydmVyX2RhdGEYBCABKAki" + "MAoVQ3JlYXRlU2Vzc2lvblJlc3BvbnNlEhcKD2dhbWVfc2Vzc2lvbl9pZBgB" + "IAEoCSJ0ChRHZXRTZXJ2ZXJJbmZvUmVxdWVzdBJDCgx1c2VyX2NvbnRleHQY" + "ASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2Vy" + "Q29udGV4dBIXCg9nYW1lX3Nlc3Npb25faWQYAiABKAkiggEKFUdldFNlcnZl" + "ckluZm9SZXNwb25zZRJKChRhY2NlcHRfY2xpZW50X3Jlc3VsdBgBIAEoCzIs" + "Lkh5ZHJhLkFwaS5TZXNzaW9uQ29udHJvbC5BY2NlcHRDbGllbnRSZXN1bHQS" + "HQoVcmVmcmVzaF9hZnRlcl9zZWNvbmRzGAIgASgFIpABChtNYW5hZ2VkR2V0" + "U2VydmVySW5mb1JlcXVlc3QSQwoMdXNlcl9jb250ZXh0GAEgASgLMi0uSHlk" + "cmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQSEwoL" + "c19jX2NvbnRleHQYAiABKAkSFwoPZ2FtZV9zZXNzaW9uX2lkGAMgASgJImoK" + "HE1hbmFnZWRHZXRTZXJ2ZXJJbmZvUmVzcG9uc2USSgoUYWNjZXB0X2NsaWVu" + "dF9yZXN1bHQYASABKAsyLC5IeWRyYS5BcGkuU2Vzc2lvbkNvbnRyb2wuQWNj" + "ZXB0Q2xpZW50UmVzdWx0ImkKFkFjdGl2YXRlU2Vzc2lvblJlcXVlc3QSFAoM" + "c2VydmVyX3Rva2VuGAEgASgJEjkKC3NlcnZlcl9pbmZvGAIgASgLMiQuSHlk" + "cmEuQXBpLlNlc3Npb25Db250cm9sLlNlcnZlckluZm8idwoXQWN0aXZhdGVT" + "ZXNzaW9uUmVzcG9uc2USRwoOc2VydmVyX2NvbnRleHQYASABKAsyLy5IeWRy" + "YS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5TZXJ2ZXJDb250ZXh0EhMK" + "C3NlcnZlcl9kYXRhGAIgASgJIn8KHUdldFNlc3Npb25NZW1iZXJFdmVudHNS" + "ZXF1ZXN0EkcKDnNlcnZlcl9jb250ZXh0GAEgASgLMi8uSHlkcmEuQXBpLklu" + "ZnJhc3RydWN0dXJlLkNvbnRleHQuU2VydmVyQ29udGV4dBIVCg1sYXN0X2V2" + "ZW50X2lkGAIgASgDInUKHkdldFNlc3Npb25NZW1iZXJFdmVudHNSZXNwb25z" + "ZRI8CgZldmVudHMYASADKAsyLC5IeWRyYS5BcGkuU2Vzc2lvbkNvbnRyb2wu" + "U2Vzc2lvbk1lbWJlckV2ZW50EhUKDWxhc3RfZXZlbnRfaWQYAiABKAMirgEK" + "IVByb2Nlc3NTZXNzaW9uTWVtYmVyRXZlbnRzUmVxdWVzdBJHCg5zZXJ2ZXJf" + "Y29udGV4dBgBIAEoCzIvLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250" + "ZXh0LlNlcnZlckNvbnRleHQSQAoEbGlzdBgCIAMoCzIyLkh5ZHJhLkFwaS5T" + "ZXNzaW9uQ29udHJvbC5TZXNzaW9uTWVtYmVyRXZlbnRSZXN1bHQiJAoiUHJv" + "Y2Vzc1Nlc3Npb25NZW1iZXJFdmVudHNSZXNwb25zZSJfChRGaW5pc2hTZXNz" + "aW9uUmVxdWVzdBJHCg5zZXJ2ZXJfY29udGV4dBgBIAEoCzIvLkh5ZHJhLkFw" + "aS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlNlcnZlckNvbnRleHQiFwoVRmlu" + "aXNoU2Vzc2lvblJlc3BvbnNlIjsKIUdldERhdGFDZW50ZXJFY2hvRW5kcG9p" + "bnRzUmVxdWVzdBIWCg5jbGllbnRfdmVyc2lvbhgBIAEoCSKCAQojR2V0RGF0" + "YUNlbnRlckVjaG9FbmRwb2ludHNWMlJlcXVlc3QSQwoMdXNlcl9jb250ZXh0" + "GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNl" + "ckNvbnRleHQSFgoOY2xpZW50X3ZlcnNpb24YAiABKAkiZgoMRWNob0VuZHBv" + "aW50EgoKAmlwGAEgASgJEgwKBHBvcnQYAiABKAUSPAoGc2NoZW1lGAMgASgO" + "MiwuSHlkcmEuQXBpLkVuZHBvaW50RGlzcGF0Y2hlci5FbmRwb2ludFNjaGVt" + "ZSJsChdEYXRhQ2VudGVyRWNob0VuZHBvaW50cxIWCg5kYXRhX2NlbnRlcl9p" + "ZBgBIAEoCRI5CgllbmRwb2ludHMYAiADKAsyJi5IeWRyYS5BcGkuU2Vzc2lv" + "bkNvbnRyb2wuRWNob0VuZHBvaW50ImoKIkdldERhdGFDZW50ZXJFY2hvRW5k" + "cG9pbnRzUmVzcG9uc2USRAoJZW5kcG9pbnRzGAEgAygLMjEuSHlkcmEuQXBp" + "LlNlc3Npb25Db250cm9sLkRhdGFDZW50ZXJFY2hvRW5kcG9pbnRzIi0KD1Nl" + "c3Npb25LZXlWYWx1ZRILCgNrZXkYASABKAkSDQoFdmFsdWUYAiABKAkiIwoT" + "SGVhcnRiZWF0U2VydmVyVGFncxIMCgRsaXN0GAEgAygJIlIKF0hlYXJ0YmVh" + "dFNlcnZlcktleVZhbHVlEjcKBGxpc3QYASADKAsyKS5IeWRyYS5BcGkuU2Vz" + "c2lvbkNvbnRyb2wuU2Vzc2lvbktleVZhbHVlIiYKFkhlYXJ0YmVhdFNlcnZl" + "ck1lbWJlcnMSDAoEbGlzdBgBIAMoCSKBBAoZU2VydmVyQnJvd3NpbmdTZXNz" + "aW9uRGF0YRI1CglnYW1lX21vZGUYASABKAsyIi5IeWRyYS5BcGkuTnVsbGFi" + "bGUuTnVsbGFibGVTdHJpbmcSNAoIZ2FtZV9tYXAYAiABKAsyIi5IeWRyYS5B" + "cGkuTnVsbGFibGUuTnVsbGFibGVTdHJpbmcSNwoLc2VydmVyX25hbWUYAyAB" + "KAsyIi5IeWRyYS5BcGkuTnVsbGFibGUuTnVsbGFibGVTdHJpbmcSPAoScGFz" + "c3dvcmRfcHJvdGVjdGVkGAQgASgLMiAuSHlkcmEuQXBpLk51bGxhYmxlLk51" + "bGxhYmxlQm9vbBI5ChBtYXhfcGxheWVyX2NvdW50GAUgASgLMh8uSHlkcmEu" + "QXBpLk51bGxhYmxlLk51bGxhYmxlSW50EjsKBHRhZ3MYBiABKAsyLS5IeWRy" + "YS5BcGkuU2Vzc2lvbkNvbnRyb2wuSGVhcnRiZWF0U2VydmVyVGFncxJFCgpr" + "ZXlfdmFsdWVzGAcgASgLMjEuSHlkcmEuQXBpLlNlc3Npb25Db250cm9sLkhl" + "YXJ0YmVhdFNlcnZlcktleVZhbHVlEkEKB21lbWJlcnMYCCABKAsyMC5IeWRy" + "YS5BcGkuU2Vzc2lvbkNvbnRyb2wuSGVhcnRiZWF0U2VydmVyTWVtYmVycyKS" + "AQoKU2VydmVyRGF0YRJJCgxzZXNzaW9uX2RhdGEYASABKAsyMy5IeWRyYS5B" + "cGkuU2Vzc2lvbkNvbnRyb2wuU2VydmVyQnJvd3NpbmdTZXNzaW9uRGF0YRI5" + "CgtzZXJ2ZXJfaW5mbxgCIAEoCzIkLkh5ZHJhLkFwaS5TZXNzaW9uQ29udHJv" + "bC5TZXJ2ZXJJbmZvInsKFUJyb3dzZVNlcnZlcnNSZXNwb25zZRITCgtzbG90" + "c190b3RhbBgBIAEoBRIWCg5zbG90c19vY2N1cGllZBgCIAEoBRI1CgdzZXJ2" + "ZXJzGAMgAygLMiQuSHlkcmEuQXBpLlNlc3Npb25Db250cm9sLlNlcnZlckRh" + "dGEiWAoTSGVhcnRiZWF0U2VydmVyRGF0YRJBCgRkYXRhGAEgASgLMjMuSHlk" + "cmEuQXBpLlNlc3Npb25Db250cm9sLlNlcnZlckJyb3dzaW5nU2Vzc2lvbkRh" + "dGEiyAEKE0NyZWF0ZVNlcnZlclJlcXVlc3QSFAoMc2VydmVyX3Rva2VuGAEg" + "ASgJEhYKDmNsaWVudF92ZXJzaW9uGAIgASgJEkgKC2NyZWF0ZV9kYXRhGAMg" + "ASgLMjMuSHlkcmEuQXBpLlNlc3Npb25Db250cm9sLlNlcnZlckJyb3dzaW5n" + "U2Vzc2lvbkRhdGESOQoLc2VydmVyX2luZm8YBCABKAsyJC5IeWRyYS5BcGku" + "U2Vzc2lvbkNvbnRyb2wuU2VydmVySW5mbyJfChRDcmVhdGVTZXJ2ZXJSZXNw" + "b25zZRJHCg5zZXJ2ZXJfY29udGV4dBgBIAEoCzIvLkh5ZHJhLkFwaS5JbmZy" + "YXN0cnVjdHVyZS5Db250ZXh0LlNlcnZlckNvbnRleHQioAEKFkhlYXJ0YmVh" + "dFNlcnZlclJlcXVlc3QSRwoOc2VydmVyX2NvbnRleHQYASABKAsyLy5IeWRy" + "YS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5TZXJ2ZXJDb250ZXh0Ej0K" + "BnVwZGF0ZRgCIAEoCzItLkh5ZHJhLkFwaS5TZXNzaW9uQ29udHJvbC5IZWFy" + "dGJlYXRTZXJ2ZXJEYXRhIjgKF0hlYXJ0YmVhdFNlcnZlclJlc3BvbnNlEh0K" + "FXJlZnJlc2hfYWZ0ZXJfc2Vjb25kcxgBIAEoBSJzChRCcm93c2VTZXJ2ZXJz" + "UmVxdWVzdBJDCgx1c2VyX2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5m" + "cmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dBIWCg5jbGllbnRfdmVy" + "c2lvbhgCIAEoCSJfChREZXN0cm95U2VydmVyUmVxdWVzdBJHCg5zZXJ2ZXJf" + "Y29udGV4dBgBIAEoCzIvLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250" + "ZXh0LlNlcnZlckNvbnRleHQiFwoVRGVzdHJveVNlcnZlclJlc3BvbnNlYgZw" + "cm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[5]
		{
			EndpointDispatcherContractsReflection.Descriptor,
			MemberEventReflection.Descriptor,
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor,
			NullableReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[34]
		{
			new GeneratedClrTypeInfo(typeof(CreateSessionRequest), CreateSessionRequest.Parser, new string[4] { "UserContext", "DataCenterId", "ClientVersion", "ServerData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(CreateSessionResponse), CreateSessionResponse.Parser, new string[1] { "GameSessionId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetServerInfoRequest), GetServerInfoRequest.Parser, new string[2] { "UserContext", "GameSessionId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetServerInfoResponse), GetServerInfoResponse.Parser, new string[2] { "AcceptClientResult", "RefreshAfterSeconds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ManagedGetServerInfoRequest), ManagedGetServerInfoRequest.Parser, new string[3] { "UserContext", "SCContext", "GameSessionId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ManagedGetServerInfoResponse), ManagedGetServerInfoResponse.Parser, new string[1] { "AcceptClientResult" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ActivateSessionRequest), ActivateSessionRequest.Parser, new string[2] { "ServerToken", "ServerInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ActivateSessionResponse), ActivateSessionResponse.Parser, new string[2] { "ServerContext", "ServerData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetSessionMemberEventsRequest), GetSessionMemberEventsRequest.Parser, new string[2] { "ServerContext", "LastEventId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetSessionMemberEventsResponse), GetSessionMemberEventsResponse.Parser, new string[2] { "Events", "LastEventId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ProcessSessionMemberEventsRequest), ProcessSessionMemberEventsRequest.Parser, new string[2] { "ServerContext", "List" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ProcessSessionMemberEventsResponse), ProcessSessionMemberEventsResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(FinishSessionRequest), FinishSessionRequest.Parser, new string[1] { "ServerContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(FinishSessionResponse), FinishSessionResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataCenterEchoEndpointsRequest), GetDataCenterEchoEndpointsRequest.Parser, new string[1] { "ClientVersion" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataCenterEchoEndpointsV2Request), GetDataCenterEchoEndpointsV2Request.Parser, new string[2] { "UserContext", "ClientVersion" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(EchoEndpoint), EchoEndpoint.Parser, new string[3] { "Ip", "Port", "Scheme" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DataCenterEchoEndpoints), DataCenterEchoEndpoints.Parser, new string[2] { "DataCenterId", "Endpoints" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataCenterEchoEndpointsResponse), GetDataCenterEchoEndpointsResponse.Parser, new string[1] { "Endpoints" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionKeyValue), SessionKeyValue.Parser, new string[2] { "Key", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(HeartbeatServerTags), HeartbeatServerTags.Parser, new string[1] { "List" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(HeartbeatServerKeyValue), HeartbeatServerKeyValue.Parser, new string[1] { "List" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(HeartbeatServerMembers), HeartbeatServerMembers.Parser, new string[1] { "List" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerBrowsingSessionData), ServerBrowsingSessionData.Parser, new string[8] { "GameMode", "GameMap", "ServerName", "PasswordProtected", "MaxPlayerCount", "Tags", "KeyValues", "Members" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServerData), ServerData.Parser, new string[2] { "SessionData", "ServerInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(BrowseServersResponse), BrowseServersResponse.Parser, new string[3] { "SlotsTotal", "SlotsOccupied", "Servers" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(HeartbeatServerData), HeartbeatServerData.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(CreateServerRequest), CreateServerRequest.Parser, new string[4] { "ServerToken", "ClientVersion", "CreateData", "ServerInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(CreateServerResponse), CreateServerResponse.Parser, new string[1] { "ServerContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(HeartbeatServerRequest), HeartbeatServerRequest.Parser, new string[2] { "ServerContext", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(HeartbeatServerResponse), HeartbeatServerResponse.Parser, new string[1] { "RefreshAfterSeconds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(BrowseServersRequest), BrowseServersRequest.Parser, new string[2] { "UserContext", "ClientVersion" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DestroyServerRequest), DestroyServerRequest.Parser, new string[1] { "ServerContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DestroyServerResponse), DestroyServerResponse.Parser, null, null, null, null, null)
		}));
	}
}
