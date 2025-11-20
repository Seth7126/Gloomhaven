using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure;

namespace Hydra.Api.EndpointDispatcher;

public static class EndpointDispatcherContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EndpointDispatcherContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CjRFbmRwb2ludERpc3BhdGNoZXIvRW5kcG9pbnREaXNwYXRjaGVyQ29udHJh" + "Y3RzLnByb3RvEhxIeWRyYS5BcGkuRW5kcG9pbnREaXNwYXRjaGVyGg1PcHRp" + "b25zLnByb3RvIrQBChlHZXRFbnZpcm9ubWVudEluZm9SZXF1ZXN0EkQKDWF1" + "dGhvcml6YXRpb24YASABKAsyLS5IeWRyYS5BcGkuRW5kcG9pbnREaXNwYXRj" + "aGVyLlNlcnZpY2VJZGVudGl0eRI6CgV0aXRsZRgCIAEoCzIrLkh5ZHJhLkFw" + "aS5FbmRwb2ludERpc3BhdGNoZXIuVGl0bGVJZGVudGl0eRIVCg1idWlsZF92" + "ZXJzaW9uGAMgASgJIm4KGkdldEVudmlyb25tZW50SW5mb1Jlc3BvbnNlEgwK" + "BGRhdGUYASABKAMSQgoLZW52aXJvbm1lbnQYAiABKAsyLS5IeWRyYS5BcGku" + "RW5kcG9pbnREaXNwYXRjaGVyLkVudmlyb25tZW50SW5mbyJaCg9TZXJ2aWNl" + "SWRlbnRpdHkSDAoEbmFtZRgBIAEoCRI5Cgd2ZXJzaW9uGAIgASgLMiguSHlk" + "cmEuQXBpLkluZnJhc3RydWN0dXJlLlNlcnZpY2VWZXJzaW9uIjcKDVRpdGxl" + "SWRlbnRpdHkSEAoIdGl0bGVfaWQYASABKAkSFAoMdGl0bGVfc2VjcmV0GAIg" + "ASgJIq8BCgxFbmRwb2ludEluZm8SDAoEbmFtZRgBIAEoCRIKCgJpcBgCIAEo" + "CRIMCgRwb3J0GAMgASgFEjwKBnNjaGVtZRgEIAEoDjIsLkh5ZHJhLkFwaS5F" + "bmRwb2ludERpc3BhdGNoZXIuRW5kcG9pbnRTY2hlbWUSOQoHdmVyc2lvbhgF" + "IAEoCzIoLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5TZXJ2aWNlVmVyc2lv" + "biKcAQoPRW52aXJvbm1lbnRJbmZvEgoKAmlkGAEgASgJEj8KBnN0YXR1cxgC" + "IAEoDjIvLkh5ZHJhLkFwaS5FbmRwb2ludERpc3BhdGNoZXIuRW52aXJvbm1l" + "bnRTdGF0dXMSPAoIZW5kcG9pbnQYAyABKAsyKi5IeWRyYS5BcGkuRW5kcG9p" + "bnREaXNwYXRjaGVyLkVuZHBvaW50SW5mbyp1ChFFbnZpcm9ubWVudFN0YXR1" + "cxIeChpFTlZJUk9OTUVOVF9TVEFUVVNfVU5LTk9XThAAEhwKGEVOVklST05N" + "RU5UX1NUQVRVU19SRUFEWRABEiIKHkVOVklST05NRU5UX1NUQVRVU19NQUlO" + "VEVOQU5DRRACKmUKDkVuZHBvaW50U2NoZW1lEhsKF0VORFBPSU5UX1NDSEVN" + "RV9TRUNVUkVEEAASHQoZRU5EUE9JTlRfU0NIRU1FX1VOU0VDVVJFRBABEhcK" + "E0VORFBPSU5UX1NDSEVNRV9VRFAQAmIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { OptionsReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[2]
		{
			typeof(EnvironmentStatus),
			typeof(EndpointScheme)
		}, null, new GeneratedClrTypeInfo[6]
		{
			new GeneratedClrTypeInfo(typeof(GetEnvironmentInfoRequest), GetEnvironmentInfoRequest.Parser, new string[3] { "Authorization", "Title", "BuildVersion" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetEnvironmentInfoResponse), GetEnvironmentInfoResponse.Parser, new string[2] { "Date", "Environment" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServiceIdentity), ServiceIdentity.Parser, new string[2] { "Name", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TitleIdentity), TitleIdentity.Parser, new string[2] { "TitleId", "TitleSecret" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(EndpointInfo), EndpointInfo.Parser, new string[5] { "Name", "Ip", "Port", "Scheme", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(EnvironmentInfo), EnvironmentInfo.Parser, new string[3] { "Id", "Status", "Endpoint" }, null, null, null, null)
		}));
	}
}
