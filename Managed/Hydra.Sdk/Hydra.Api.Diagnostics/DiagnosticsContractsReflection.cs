using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Diagnostics;

public static class DiagnosticsContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static DiagnosticsContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiZEaWFnbm9zdGljcy9EaWFnbm9zdGljc0NvbnRyYWN0cy5wcm90bxIVSHlk" + "cmEuQXBpLkRpYWdub3N0aWNzGhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3Rv" + "GhtDb250ZXh0L1NlcnZlckNvbnRleHQucHJvdG8ivwIKGVdyaXRlQ3Jhc2hE" + "dW1wVXNlclJlcXVlc3QSQwoMdXNlcl9jb250ZXh0GAEgASgLMi0uSHlkcmEu" + "QXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQSPgoKcHJv" + "cGVydGllcxgCIAMoCzIqLkh5ZHJhLkFwaS5EaWFnbm9zdGljcy5EaWFnbm9z" + "dGljc1Byb3BlcnR5EhEKCWR1bXBfaGFzaBgDIAEoCRIMCgRkYXRhGAQgASgM" + "Ej0KCWRhdGFfdHlwZRgFIAEoDjIqLkh5ZHJhLkFwaS5EaWFnbm9zdGljcy5E" + "aWFnbm9zdGljc0RhdGFUeXBlEhAKCHByb3ZpZGVyGAYgASgJEhYKDmNsaWVu" + "dF92ZXJzaW9uGAcgASgJEhMKC3Nka192ZXJzaW9uGAggASgJIhwKGldyaXRl" + "Q3Jhc2hEdW1wVXNlclJlc3BvbnNlIqwCChtXcml0ZUNyYXNoRHVtcFNlcnZl" + "clJlcXVlc3QSRwoOc2VydmVyX2NvbnRleHQYASABKAsyLy5IeWRyYS5BcGku" + "SW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5TZXJ2ZXJDb250ZXh0Ej4KCnByb3Bl" + "cnRpZXMYAiADKAsyKi5IeWRyYS5BcGkuRGlhZ25vc3RpY3MuRGlhZ25vc3Rp" + "Y3NQcm9wZXJ0eRIRCglkdW1wX2hhc2gYAyABKAkSDAoEZGF0YRgEIAEoDBI9" + "CglkYXRhX3R5cGUYBSABKA4yKi5IeWRyYS5BcGkuRGlhZ25vc3RpY3MuRGlh" + "Z25vc3RpY3NEYXRhVHlwZRIPCgd2ZXJzaW9uGAYgASgJEhMKC3Nka192ZXJz" + "aW9uGAcgASgJIh4KHFdyaXRlQ3Jhc2hEdW1wU2VydmVyUmVzcG9uc2UiMgoT" + "RGlhZ25vc3RpY3NQcm9wZXJ0eRIMCgRuYW1lGAEgASgJEg0KBXZhbHVlGAIg" + "ASgJKm4KDUNyYXNoRHVtcFR5cGUSGwoXQ1JBU0hfRFVNUF9UWVBFX1VOS05P" + "V04QABIfChtDUkFTSF9EVU1QX1RZUEVfR0FNRV9DTElFTlQQARIfChtDUkFT" + "SF9EVU1QX1RZUEVfR0FNRV9TRVJWRVIQAipXChNEaWFnbm9zdGljc0RhdGFU" + "eXBlEiAKHERJQUdOT1NUSUNTX0RBVEFfVFlQRV9CSU5BUlkQABIeChpESUFH" + "Tk9TVElDU19EQVRBX1RZUEVfVEVYVBABYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(new Type[2]
		{
			typeof(CrashDumpType),
			typeof(DiagnosticsDataType)
		}, null, new GeneratedClrTypeInfo[5]
		{
			new GeneratedClrTypeInfo(typeof(WriteCrashDumpUserRequest), WriteCrashDumpUserRequest.Parser, new string[8] { "UserContext", "Properties", "DumpHash", "Data", "DataType", "Provider", "ClientVersion", "SdkVersion" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteCrashDumpUserResponse), WriteCrashDumpUserResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteCrashDumpServerRequest), WriteCrashDumpServerRequest.Parser, new string[7] { "ServerContext", "Properties", "DumpHash", "Data", "DataType", "Version", "SdkVersion" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(WriteCrashDumpServerResponse), WriteCrashDumpServerResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DiagnosticsProperty), DiagnosticsProperty.Parser, new string[2] { "Name", "Value" }, null, null, null, null)
		}));
	}
}
