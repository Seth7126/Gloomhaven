using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Infrastructure.Context;
using RedLynx.Api.Localization;

namespace RedLynx.Api.Entitlement;

public static class EntitlementContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EntitlementContractsReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CjJyZWRseW54LWFwaS9FbnRpdGxlbWVudC9FbnRpdGxlbWVudENvbnRyYWN0" + "cy5wcm90bxIXUmVkTHlueC5BcGkuRW50aXRsZW1lbnQaH2dvb2dsZS9wcm90" + "b2J1Zi90aW1lc3RhbXAucHJvdG8aGUNvbnRleHQvVXNlckNvbnRleHQucHJv" + "dG8aIkxvY2FsaXphdGlvbi9Mb2NhbGl6ZWRTdHJpbmcucHJvdG8iXQoWR2V0" + "RW50aXRsZW1lbnRzUmVxdWVzdBJDCgx1c2VyX2NvbnRleHQYASABKAsyLS5I" + "eWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dCJV" + "ChdHZXRFbnRpdGxlbWVudHNSZXNwb25zZRI6CgxlbnRpdGxlbWVudHMYASAD" + "KAsyJC5SZWRMeW54LkFwaS5FbnRpdGxlbWVudC5FbnRpdGxlbWVudCJ6ChpD" + "b25zdW1lRW50aXRsZW1lbnRzUmVxdWVzdBJDCgx1c2VyX2NvbnRleHQYASAB" + "KAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29u" + "dGV4dBIXCg9lbnRpdGxlbWVudF9pZHMYAiADKAkiWQobQ29uc3VtZUVudGl0" + "bGVtZW50c1Jlc3BvbnNlEjoKDGVudGl0bGVtZW50cxgBIAMoCzIkLlJlZEx5" + "bnguQXBpLkVudGl0bGVtZW50LkVudGl0bGVtZW50IsoDCgtFbnRpdGxlbWVu" + "dBIKCgJpZBgBIAEoCRIMCgRuYW1lGAIgASgJEjsKCHVfaV9uYW1lGAMgAygL" + "MikuUmVkTHlueC5BcGkuTG9jYWxpemF0aW9uLkxvY2FsaXplZFN0cmluZxI7" + "Cgh1X2lfZGVzYxgEIAMoCzIpLlJlZEx5bnguQXBpLkxvY2FsaXphdGlvbi5M" + "b2NhbGl6ZWRTdHJpbmcSEwoLcGljdHVyZV91cmwYBSABKAkSQgoQZW50aXRs" + "ZW1lbnRfdHlwZRgGIAEoDjIoLlJlZEx5bnguQXBpLkVudGl0bGVtZW50LkVu" + "dGl0bGVtZW50VHlwZRJGChJlbnRpdGxlbWVudF9zdGF0dXMYByABKA4yKi5S" + "ZWRMeW54LkFwaS5FbnRpdGxlbWVudC5FbnRpdGxlbWVudFN0YXR1cxIrCgdj" + "cmVhdGVkGAggASgLMhouZ29vZ2xlLnByb3RvYnVmLlRpbWVzdGFtcBIrCgd1" + "cGRhdGVkGAkgASgLMhouZ29vZ2xlLnByb3RvYnVmLlRpbWVzdGFtcBIVCg1j" + "b25zdW1lX2xpbWl0GAogASgFEhUKDWNvbnN1bWVfY291bnQYCyABKAUqbgoP" + "RW50aXRsZW1lbnRUeXBlEhwKGEVOVElUTEVNRU5UX1RZUEVfVU5LTk9XThAA" + "EhwKGEVOVElUTEVNRU5UX1RZUEVfRFVSQUJMRRABEh8KG0VOVElUTEVNRU5U" + "X1RZUEVfQ09OU1VNQUJMRRACKnMKEUVudGl0bGVtZW50U3RhdHVzEh4KGkVO" + "VElUTEVNRU5UX1NUQVRVU19VTktOT1dOEAASIAocRU5USVRMRU1FTlRfU1RB" + "VFVTX1VOQ0xBSU1FRBABEhwKGEVOVElUTEVNRU5UX1NUQVRVU19PV05FRBAC" + "YgZwcm90bzM="), new FileDescriptor[3]
		{
			TimestampReflection.Descriptor,
			UserContextReflection.Descriptor,
			LocalizedStringReflection.Descriptor
		}, new GeneratedClrTypeInfo(new System.Type[2]
		{
			typeof(EntitlementType),
			typeof(EntitlementStatus)
		}, null, new GeneratedClrTypeInfo[5]
		{
			new GeneratedClrTypeInfo(typeof(GetEntitlementsRequest), GetEntitlementsRequest.Parser, new string[1] { "UserContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetEntitlementsResponse), GetEntitlementsResponse.Parser, new string[1] { "Entitlements" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConsumeEntitlementsRequest), ConsumeEntitlementsRequest.Parser, new string[2] { "UserContext", "EntitlementIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConsumeEntitlementsResponse), ConsumeEntitlementsResponse.Parser, new string[1] { "Entitlements" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(Entitlement), Entitlement.Parser, new string[11]
			{
				"Id", "Name", "UIName", "UIDesc", "PictureUrl", "EntitlementType", "EntitlementStatus", "Created", "Updated", "ConsumeLimit",
				"ConsumeCount"
			}, null, null, null, null)
		}));
	}
}
