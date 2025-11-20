using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Facts;

public static class FactsContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static FactsContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChpGYWN0cy9GYWN0c0NvbnRyYWN0cy5wcm90bxIPSHlkcmEuQXBpLkZhY3Rz" + "Gg1PcHRpb25zLnByb3RvGhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvGhlD" + "b250ZXh0L1Rvb2xDb250ZXh0LnByb3RvGhtDb250ZXh0L1NlcnZlckNvbnRl" + "eHQucHJvdG8isgEKGldyaXRlQmluYXJ5UGFja1VzZXJSZXF1ZXN0EkMKDHVz" + "ZXJfY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5D" + "b250ZXh0LlVzZXJDb250ZXh0Eg8KB2VudHJpZXMYAiABKAwSDAoEZGF0YRgD" + "IAEoDBIwCgZoZWFkZXIYBCABKAsyIC5IeWRyYS5BcGkuRmFjdHMuRmFjdHNQ" + "YWNrSGVhZGVyIh0KG1dyaXRlQmluYXJ5UGFja1VzZXJSZXNwb25zZSKvAgoP" + "RmFjdHNQYWNrSGVhZGVyEi4KB2NvbnRleHQYAiADKAsyHS5IeWRyYS5BcGku" + "RmFjdHMuRmFjdHNDb250ZXh0EhMKC3BhY2tfbnVtYmVyGAMgASgFEhIKCnN0" + "YXJ0X3RpbWUYBCABKAMSEAoIZW5kX3RpbWUYBSABKAMSEQoJaW5pdF90aW1l" + "GAYgASgDEjAKBmZvcm1hdBgHIAEoDjIgLkh5ZHJhLkFwaS5GYWN0cy5GYWN0" + "c1BhY2tGb3JtYXQSOAoHdmVyc2lvbhgIIAEoCzInLkh5ZHJhLkFwaS5GYWN0" + "cy5GYWN0c1BhY2tGb3JtYXRWZXJzaW9uEjIKB29wdGlvbnMYCSABKAsyIS5I" + "eWRyYS5BcGkuRmFjdHMuRmFjdHNQYWNrT3B0aW9ucyJSChBGYWN0c1BhY2tP" + "cHRpb25zEhAKCGlzX2ZpbmFsGAEgASgIEhUKDWlzX2xvY2FsX3RpbWUYAiAB" + "KAgSFQoNaXNfY29tcHJlc3NlZBgDIAEoCCI9CgxGYWN0c0NvbnRleHQSFQoN" + "cHJvcGVydHlfbmFtZRgBIAEoCRIWCg5wcm9wZXJ0eV92YWx1ZRgCIAEoCSI2" + "ChZGYWN0c1BhY2tGb3JtYXRWZXJzaW9uEg0KBW1ham9yGAEgASgFEg0KBW1p" + "bm9yGAIgASgFIrIBChpXcml0ZUJpbmFyeVBhY2tUb29sUmVxdWVzdBJDCgx0" + "b29sX2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUu" + "Q29udGV4dC5Ub29sQ29udGV4dBIPCgdlbnRyaWVzGAIgASgMEgwKBGRhdGEY" + "AyABKAwSMAoGaGVhZGVyGAQgASgLMiAuSHlkcmEuQXBpLkZhY3RzLkZhY3Rz" + "UGFja0hlYWRlciIdChtXcml0ZUJpbmFyeVBhY2tUb29sUmVzcG9uc2UiuAEK" + "HFdyaXRlQmluYXJ5UGFja1NlcnZlclJlcXVlc3QSRwoOc2VydmVyX2NvbnRl" + "eHQYASABKAsyLy5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5T" + "ZXJ2ZXJDb250ZXh0Eg8KB2VudHJpZXMYAiABKAwSDAoEZGF0YRgDIAEoDBIw" + "CgZoZWFkZXIYBCABKAsyIC5IeWRyYS5BcGkuRmFjdHMuRmFjdHNQYWNrSGVh" + "ZGVyIh8KHVdyaXRlQmluYXJ5UGFja1NlcnZlclJlc3BvbnNlKnkKD0ZhY3Rz" + "UGFja0Zvcm1hdBIdChlGQUNUU19QQUNLX0ZPUk1BVF9VTktOT1dOEAASJQoh" + "RkFDVFNfUEFDS19GT1JNQVRfTkVUX0NMSUVOVF9QQUNLEAESIAocRkFDVFNf" + "UEFDS19GT1JNQVRfUFJJTlRGX1JBVxACKvsFCgxDb250ZXh0VmFsdWUSGQoV" + "Q09OVEVYVF9WQUxVRV9VTktOT1dOEAASRwojQ09OVEVYVF9WQUxVRV9HTE9C" + "QUxfQ09OVEVYVF9VUERBVEUQARoemk0bChlBREYvR0xPQkFMX0NPTlRFWFQv" + "VVBEQVRFEkcKI0NPTlRFWFRfVkFMVUVfR0xPQkFMX0NPTlRFWFRfUkVNT1ZF" + "EAIaHppNGwoZQURGL0dMT0JBTF9DT05URVhUL1JFTU9WRRIvChlDT05URVhU" + "X1ZBTFVFX1NES19WRVJTSU9OEAMaEJpNDQoLU0RLX1ZFUlNJT04SNQocQ09O" + "VEVYVF9WQUxVRV9DTElFTlRfVkVSU0lPThAEGhOaTRAKDkNMSUVOVF9WRVJT" + "SU9OEikKFkNPTlRFWFRfVkFMVUVfUExBVEZPUk0QBRoNmk0KCghQTEFURk9S" + "TRI7Ch9DT05URVhUX1ZBTFVFX0tFUk5FTF9TRVNTSU9OX0lEEAYaFppNEwoR" + "S0VSTkVMX1NFU1NJT05fSUQSIwoTQ09OVEVYVF9WQUxVRV9LU0lWQRAHGgqa" + "TQcKBUtTSVZBEj0KIENPTlRFWFRfVkFMVUVfR0FNRV9DT05GSUdVUkFUSU9O" + "EAgaF5pNFAoSR0FNRV9DT05GSUdVUkFUSU9OEiEKEkNPTlRFWFRfVkFMVUVf" + "Uk9MRRAJGgmaTQYKBFJPTEUSKQoWQ09OVEVYVF9WQUxVRV9USVRMRV9JRBAK" + "Gg2aTQoKCFRJVExFX0lEEjUKHENPTlRFWFRfVkFMVUVfRU5WSVJPTk1FTlRf" + "SUQQCxoTmk0QCg5FTlZJUk9OTUVOVF9JRBInChVDT05URVhUX1ZBTFVFX1VT" + "RVJfSUQQDBoMmk0JCgdVU0VSX0lEEjEKGkNPTlRFWFRfVkFMVUVfQUNDT1VO" + "VF9OQU1FEA0aEZpNDgoMQUNDT1VOVF9OQU1FEikKFkNPTlRFWFRfVkFMVUVf" + "UFJPVklERVIQDhoNmk0KCghQUk9WSURFUipkCghGYWN0Um9sZRIrChVGQUNU" + "X1JPTEVfR0FNRV9DTElFTlQQABoQmk0NCgtHQU1FX0NMSUVOVBIrChVGQUNU" + "X1JPTEVfR0FNRV9TRVJWRVIQARoQmk0NCgtHQU1FX1NFUlZFUmIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[4]
		{
			OptionsReflection.Descriptor,
			UserContextReflection.Descriptor,
			ToolContextReflection.Descriptor,
			ServerContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(new Type[3]
		{
			typeof(FactsPackFormat),
			typeof(ContextValue),
			typeof(FactRole)
		}, null, new GeneratedClrTypeInfo[10]
		{
			new GeneratedClrTypeInfo(typeof(WriteBinaryPackUserRequest), WriteBinaryPackUserRequest.Parser, new string[4] { "UserContext", "Entries", "Data", "Header" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteBinaryPackUserResponse), WriteBinaryPackUserResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(FactsPackHeader), FactsPackHeader.Parser, new string[8] { "Context", "PackNumber", "StartTime", "EndTime", "InitTime", "Format", "Version", "Options" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(FactsPackOptions), FactsPackOptions.Parser, new string[3] { "IsFinal", "IsLocalTime", "IsCompressed" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(FactsContext), FactsContext.Parser, new string[2] { "PropertyName", "PropertyValue" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(FactsPackFormatVersion), FactsPackFormatVersion.Parser, new string[2] { "Major", "Minor" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteBinaryPackToolRequest), WriteBinaryPackToolRequest.Parser, new string[4] { "ToolContext", "Entries", "Data", "Header" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteBinaryPackToolResponse), WriteBinaryPackToolResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteBinaryPackServerRequest), WriteBinaryPackServerRequest.Parser, new string[4] { "ServerContext", "Entries", "Data", "Header" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteBinaryPackServerResponse), WriteBinaryPackServerResponse.Parser, null, null, null, null, null)
		}));
	}
}
