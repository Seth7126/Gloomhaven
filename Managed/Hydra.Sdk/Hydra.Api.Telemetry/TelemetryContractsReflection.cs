using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Telemetry;

public static class TelemetryContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TelemetryContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiJUZWxlbWV0cnkvVGVsZW1ldHJ5Q29udHJhY3RzLnByb3RvEhNIeWRyYS5B" + "cGkuVGVsZW1ldHJ5GhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvGhtDb250" + "ZXh0L1NlcnZlckNvbnRleHQucHJvdG8icQoaU2VuZFRlbGVtZXRyeUV2ZW50" + "c1JlcXVlc3QSDgoGZXZlbnRzGAEgAygJEkMKDHVzZXJfY29udGV4dBgCIAEo" + "CzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250" + "ZXh0Ih0KG1NlbmRUZWxlbWV0cnlFdmVudHNSZXNwb25zZSLSAQoYU2VuZFRl" + "bGVtZXRyeVBhY2tSZXF1ZXN0EkMKDHVzZXJfY29udGV4dBgBIAEoCzItLkh5" + "ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0EjgK" + "BmhlYWRlchgCIAEoCzIoLkh5ZHJhLkFwaS5UZWxlbWV0cnkuVGVsZW1ldHJ5" + "UGFja0hlYWRlchIPCgdlbnRyaWVzGAMgASgMEgwKBGRhdGEYBCABKAwSGAoQ" + "ZXZlbnRfZ2VuZXJhdGlvbhgFIAEoBSIbChlTZW5kVGVsZW1ldHJ5UGFja1Jl" + "c3BvbnNlItMCChNUZWxlbWV0cnlQYWNrSGVhZGVyEjYKB2NvbnRleHQYASAD" + "KAsyJS5IeWRyYS5BcGkuVGVsZW1ldHJ5LlRlbGVtZXRyeUNvbnRleHQSEwoL" + "cGFja19udW1iZXIYAiABKAUSEgoKc3RhcnRfdGltZRgDIAEoAxIQCghlbmRf" + "dGltZRgEIAEoAxIRCglpbml0X3RpbWUYBSABKAMSOAoGZm9ybWF0GAcgASgO" + "MiguSHlkcmEuQXBpLlRlbGVtZXRyeS5UZWxlbWV0cnlQYWNrRm9ybWF0EkAK" + "B3ZlcnNpb24YCCABKAsyLy5IeWRyYS5BcGkuVGVsZW1ldHJ5LlRlbGVtZXRy" + "eVBhY2tGb3JtYXRWZXJzaW9uEjoKB29wdGlvbnMYCSABKAsyKS5IeWRyYS5B" + "cGkuVGVsZW1ldHJ5LlRlbGVtZXRyeVBhY2tPcHRpb25zInEKFFRlbGVtZXRy" + "eVBhY2tPcHRpb25zEhUKDWlzX2xvY2FsX3RpbWUYAiABKAgSQgoLY29tcHJl" + "c3Npb24YBCABKA4yLS5IeWRyYS5BcGkuVGVsZW1ldHJ5LlRlbGVtZXRyeVBh" + "Y2tDb21wcmVzc2lvbiJBChBUZWxlbWV0cnlDb250ZXh0EhUKDXByb3BlcnR5" + "X25hbWUYASABKAkSFgoOcHJvcGVydHlfdmFsdWUYAiABKAkiOgoaVGVsZW1l" + "dHJ5UGFja0Zvcm1hdFZlcnNpb24SDQoFbWFqb3IYASABKAUSDQoFbWlub3IY" + "AiABKAUiZgoXVGVsZW1ldHJ5RXZlbnRCYXNlRW50cnkSEgoKZXZlbnRfdHlw" + "ZRgBIAEoCRIRCglldmVudF91aWQYAiABKAkSDwoHdmVyc2lvbhgDIAEoBRIT" + "Cgtqc29uX3BhcmFtcxgEIAEoCSJ7CiBTZW5kVGVsZW1ldHJ5U2VydmVyRXZl" + "bnRzUmVxdWVzdBJHCg5zZXJ2ZXJfY29udGV4dBgBIAEoCzIvLkh5ZHJhLkFw" + "aS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlNlcnZlckNvbnRleHQSDgoGZXZl" + "bnRzGAIgAygJIiMKIVNlbmRUZWxlbWV0cnlTZXJ2ZXJFdmVudHNSZXNwb25z" + "ZSpeChNUZWxlbWV0cnlQYWNrRm9ybWF0EiEKHVRFTEVNRVRSWV9QQUNLX0ZP" + "Uk1BVF9VTktOT1dOEAASJAogVEVMRU1FVFJZX1BBQ0tfRk9STUFUX0pTT05f" + "QkFTRUQQASqJAQoYVGVsZW1ldHJ5UGFja0NvbXByZXNzaW9uEiMKH1RFTEVN" + "RVRSWV9QQUNLX0NPTVBSRVNTSU9OX05PTkUQABIjCh9URUxFTUVUUllfUEFD" + "S19DT01QUkVTU0lPTl9aTElCEAESIwofVEVMRU1FVFJZX1BBQ0tfQ09NUFJF" + "U1NJT05fR1pJUBACYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(new Type[2]
		{
			typeof(TelemetryPackFormat),
			typeof(TelemetryPackCompression)
		}, null, new GeneratedClrTypeInfo[11]
		{
			new GeneratedClrTypeInfo(typeof(SendTelemetryEventsRequest), SendTelemetryEventsRequest.Parser, new string[2] { "Events", "UserContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendTelemetryEventsResponse), SendTelemetryEventsResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendTelemetryPackRequest), SendTelemetryPackRequest.Parser, new string[5] { "UserContext", "Header", "Entries", "Data", "EventGeneration" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendTelemetryPackResponse), SendTelemetryPackResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryPackHeader), TelemetryPackHeader.Parser, new string[8] { "Context", "PackNumber", "StartTime", "EndTime", "InitTime", "Format", "Version", "Options" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryPackOptions), TelemetryPackOptions.Parser, new string[2] { "IsLocalTime", "Compression" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryContext), TelemetryContext.Parser, new string[2] { "PropertyName", "PropertyValue" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryPackFormatVersion), TelemetryPackFormatVersion.Parser, new string[2] { "Major", "Minor" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryEventBaseEntry), TelemetryEventBaseEntry.Parser, new string[4] { "EventType", "EventUid", "Version", "JsonParams" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendTelemetryServerEventsRequest), SendTelemetryServerEventsRequest.Parser, new string[2] { "ServerContext", "Events" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendTelemetryServerEventsResponse), SendTelemetryServerEventsResponse.Parser, null, null, null, null, null)
		}));
	}
}
