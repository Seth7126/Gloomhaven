using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace RedLynx.Api.Telemetry;

public static class TelemetryContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TelemetryContractsReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CjVyZWRseW54LWFwaS9UZWxlbWV0cnlTZXJ2aWNlL1RlbGVtZXRyeUNvbnRy" + "YWN0cy5wcm90bxIVUmVkTHlueC5BcGkuVGVsZW1ldHJ5GhlDb250ZXh0L1Vz" + "ZXJDb250ZXh0LnByb3RvItQBChhTZW5kVGVsZW1ldHJ5UGFja1JlcXVlc3QS" + "QwoMdXNlcl9jb250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0" + "dXJlLkNvbnRleHQuVXNlckNvbnRleHQSOgoGaGVhZGVyGAIgASgLMiouUmVk" + "THlueC5BcGkuVGVsZW1ldHJ5LlRlbGVtZXRyeVBhY2tIZWFkZXISDwoHZW50" + "cmllcxgDIAEoDBIMCgRkYXRhGAQgASgMEhgKEGV2ZW50X2dlbmVyYXRpb24Y" + "BSABKAUiGwoZU2VuZFRlbGVtZXRyeVBhY2tSZXNwb25zZSLbAgoTVGVsZW1l" + "dHJ5UGFja0hlYWRlchI4Cgdjb250ZXh0GAEgAygLMicuUmVkTHlueC5BcGku" + "VGVsZW1ldHJ5LlRlbGVtZXRyeUNvbnRleHQSEwoLcGFja19udW1iZXIYAiAB" + "KAUSEgoKc3RhcnRfdGltZRgDIAEoAxIQCghlbmRfdGltZRgEIAEoAxIRCglp" + "bml0X3RpbWUYBSABKAMSOgoGZm9ybWF0GAcgASgOMiouUmVkTHlueC5BcGku" + "VGVsZW1ldHJ5LlRlbGVtZXRyeVBhY2tGb3JtYXQSQgoHdmVyc2lvbhgIIAEo" + "CzIxLlJlZEx5bnguQXBpLlRlbGVtZXRyeS5UZWxlbWV0cnlQYWNrRm9ybWF0" + "VmVyc2lvbhI8CgdvcHRpb25zGAkgASgLMisuUmVkTHlueC5BcGkuVGVsZW1l" + "dHJ5LlRlbGVtZXRyeVBhY2tPcHRpb25zIlYKFFRlbGVtZXRyeVBhY2tPcHRp" + "b25zEhAKCGlzX2ZpbmFsGAEgASgIEhUKDWlzX2xvY2FsX3RpbWUYAiABKAgS" + "FQoNaXNfY29tcHJlc3NlZBgDIAEoCCJBChBUZWxlbWV0cnlDb250ZXh0EhUK" + "DXByb3BlcnR5X25hbWUYASABKAkSFgoOcHJvcGVydHlfdmFsdWUYAiABKAki" + "OgoaVGVsZW1ldHJ5UGFja0Zvcm1hdFZlcnNpb24SDQoFbWFqb3IYASABKAUS" + "DQoFbWlub3IYAiABKAUiZgoXVGVsZW1ldHJ5RXZlbnRCYXNlRW50cnkSEgoK" + "ZXZlbnRfdHlwZRgBIAEoCRIRCglldmVudF91aWQYAiABKAkSDwoHdmVyc2lv" + "bhgDIAEoBRITCgtqc29uX3BhcmFtcxgEIAEoCSpeChNUZWxlbWV0cnlQYWNr" + "Rm9ybWF0EiEKHVRFTEVNRVRSWV9QQUNLX0ZPUk1BVF9VTktOT1dOEAASJAog" + "VEVMRU1FVFJZX1BBQ0tfRk9STUFUX0pTT05fQkFTRUQQAWIGcHJvdG8z"), new FileDescriptor[1] { UserContextReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[1] { typeof(TelemetryPackFormat) }, null, new GeneratedClrTypeInfo[7]
		{
			new GeneratedClrTypeInfo(typeof(SendTelemetryPackRequest), SendTelemetryPackRequest.Parser, new string[5] { "UserContext", "Header", "Entries", "Data", "EventGeneration" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendTelemetryPackResponse), SendTelemetryPackResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryPackHeader), TelemetryPackHeader.Parser, new string[8] { "Context", "PackNumber", "StartTime", "EndTime", "InitTime", "Format", "Version", "Options" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryPackOptions), TelemetryPackOptions.Parser, new string[3] { "IsFinal", "IsLocalTime", "IsCompressed" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryContext), TelemetryContext.Parser, new string[2] { "PropertyName", "PropertyValue" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryPackFormatVersion), TelemetryPackFormatVersion.Parser, new string[2] { "Major", "Minor" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(TelemetryEventBaseEntry), TelemetryEventBaseEntry.Parser, new string[4] { "EventType", "EventUid", "Version", "JsonParams" }, null, null, null, null)
		}));
	}
}
