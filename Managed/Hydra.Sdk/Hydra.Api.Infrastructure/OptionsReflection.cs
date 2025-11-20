using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure;

public static class OptionsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static OptionsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Cg1PcHRpb25zLnByb3RvEhhIeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUaIGdv" + "b2dsZS9wcm90b2J1Zi9kZXNjcmlwdG9yLnByb3RvIiUKFEVudW1WYWx1ZURl" + "c2NyaXB0aW9uEg0KBXZhbHVlGAEgASgJInEKEFNlcnZpY2VBdHRyaWJ1dGUS" + "OQoHdmVyc2lvbhgBIAEoCzIoLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5T" + "ZXJ2aWNlVmVyc2lvbhITCgthY2Nlc3Nfcm9sZRgCIAEoCRINCgVzY29wZRgD" + "IAEoCSI1Cg9NZXRob2RBdHRyaWJ1dGUSEwoLYWNjZXNzX3JvbGUYASABKAkS" + "DQoFc2NvcGUYAiABKAkiPAoOU2VydmljZVZlcnNpb24SDQoFbWFqb3IYASAB" + "KAUSDQoFbWlub3IYAiABKAUSDAoEaGFzaBgDIAEoCSrVBAoRU2VydmljZUFj" + "Y2Vzc1JvbGUSCwoHVU5LTk9XThAAEhUKBUdVRVNUEAEaCppNBwoFR3Vlc3QS" + "IAoLR0FNRV9DTElFTlQQAhoPmk0MCgpHYW1lQ2xpZW50EiAKC0dBTUVfU0VS" + "VkVSEAMaD5pNDAoKR2FtZVNlcnZlchIeCgpXRUJfUE9SVEFMEAQaDppNCwoJ" + "V2ViUG9ydGFsEjEKFFNFUlZFUl9NQU5BR0VSX0FHRU5UEAUaF5pNFAoSU2Vy" + "dmVyTWFuYWdlckFnZW50EiwKEVNUQU5EQUxPTkVfU0VSVkVSEAYaFZpNEgoQ" + "U3RhbmRhbG9uZVNlcnZlchIgCgtCT1RfTUFOQUdFUhAHGg+aTQwKCkJvdE1h" + "bmFnZXISFQoFQURNSU4QCBoKmk0HCgVBZG1pbhIXCgZQTFVHSU4QCRoLmk0I" + "CgZQbHVnaW4SIgoMUExVR0lOX0FETUlOEAoaEJpNDQoLUGx1Z2luQWRtaW4S" + "NAoVQ09ORklHVVJBVElPTl9NQU5BR0VSEAsaGZpNFgoUQ29uZmlndXJhdGlv" + "bk1hbmFnZXISLgoSRElBR05PU1RJQ19NQU5BR0VSEAwaFppNEwoRRGlhZ25v" + "c3RpY01hbmFnZXISJQoOR0FNRV9ORVhVU19BUEkQDRoRmk0OCgxHYW1lTmV4" + "dXNBcGkSOwoZREVESUNBVEVEX1NFUlZFUl9VUExPQURFUhAOGhyaTRkKF0Rl" + "ZGljYXRlZFNlcnZlclVwbG9hZGVyEhcKBkFUSEVOQRAPGguaTQgKBkF0aGVu" + "YSpSCg9WaXNpYmlsaXR5U2NvcGUSEwoETk9ORRAAGgmaTQYKBE5vbmUSEwoE" + "QkFTRRABGgmaTQYKBEJhc2USFQoFVElUTEUQAhoKmk0HCgVUaXRsZTpeCgdz" + "ZXJ2aWNlEh8uZ29vZ2xlLnByb3RvYnVmLlNlcnZpY2VPcHRpb25zGNCGAyAB" + "KAsyKi5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuU2VydmljZUF0dHJpYnV0" + "ZTpbCgZtZXRob2QSHi5nb29nbGUucHJvdG9idWYuTWV0aG9kT3B0aW9ucxjS" + "hgMgASgLMikuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLk1ldGhvZEF0dHJp" + "YnV0ZTpgCgRkZXNjEiEuZ29vZ2xlLnByb3RvYnVmLkVudW1WYWx1ZU9wdGlv" + "bnMY0wkgASgLMi4uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkVudW1WYWx1" + "ZURlc2NyaXB0aW9uYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { DescriptorReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[2]
		{
			typeof(ServiceAccessRole),
			typeof(VisibilityScope)
		}, new Extension[3]
		{
			OptionsExtensions.Service,
			OptionsExtensions.Method,
			OptionsExtensions.Desc
		}, new GeneratedClrTypeInfo[4]
		{
			new GeneratedClrTypeInfo(typeof(EnumValueDescription), EnumValueDescription.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServiceAttribute), ServiceAttribute.Parser, new string[3] { "Version", "AccessRole", "Scope" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(MethodAttribute), MethodAttribute.Parser, new string[2] { "AccessRole", "Scope" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ServiceVersion), ServiceVersion.Parser, new string[3] { "Major", "Minor", "Hash" }, null, null, null, null)
		}));
	}
}
