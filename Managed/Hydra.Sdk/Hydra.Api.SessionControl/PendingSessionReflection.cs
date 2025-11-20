using System;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;

namespace Hydra.Api.SessionControl;

public static class PendingSessionReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PendingSessionReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CitTZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyL1BlbmRpbmdTZXNzaW9uLnByb3Rv" + "EhhIeWRyYS5BcGkuU2Vzc2lvbkNvbnRyb2waNEVuZHBvaW50RGlzcGF0Y2hl" + "ci9FbmRwb2ludERpc3BhdGNoZXJDb250cmFjdHMucHJvdG8i2gEKDlBlbmRp" + "bmdTZXNzaW9uEkEKDWF1dGhfZW5kcG9pbnQYASABKAsyKi5IeWRyYS5BcGku" + "RW5kcG9pbnREaXNwYXRjaGVyLkVuZHBvaW50SW5mbxIZChFoeWRyYV9hdXRo" + "X3RpY2tldBgCIAEoCRIUCgxzZXJ2ZXJfdG9rZW4YAyABKAkSDwoHdmVyc2lv" + "bhgEIAEoCRIXCg9nYW1lX3Nlc3Npb25faWQYBSABKAkSEAoIdGl0bGVfaWQY" + "BiABKAkSGAoQZXhjbHVkZWRfZHNtX2lkcxgHIAMoCWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { EndpointDispatcherContractsReflection.Descriptor }, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PendingSession), PendingSession.Parser, new string[7] { "AuthEndpoint", "HydraAuthTicket", "ServerToken", "Version", "GameSessionId", "TitleId", "ExcludedDsmIds" }, null, null, null, null)
		}));
	}
}
