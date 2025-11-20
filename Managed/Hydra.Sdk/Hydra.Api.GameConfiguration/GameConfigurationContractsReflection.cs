using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.GameConfiguration;

public static class GameConfigurationContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static GameConfigurationContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CjJHYW1lQ29uZmlndXJhdGlvbi9HYW1lQ29uZmlndXJhdGlvbkNvbnRyYWN0" + "cy5wcm90bxIbSHlkcmEuQXBpLkdhbWVDb25maWd1cmF0aW9uGhlDb250ZXh0" + "L1VzZXJDb250ZXh0LnByb3RvGiJDb250ZXh0L0NvbmZpZ3VyYXRpb25Db250" + "ZXh0LnByb3RvIqkBCh5HZXRDb25maWd1cmF0aW9uQ29udGV4dFJlcXVlc3QS" + "PgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5D" + "b250ZXh0LlVzZXJDb250ZXh0EkcKCmNvbXBvbmVudHMYAiADKAsyMy5IeWRy" + "YS5BcGkuR2FtZUNvbmZpZ3VyYXRpb24uQ29uZmlndXJhdGlvbkNvbXBvbmVu" + "dCK6AQofR2V0Q29uZmlndXJhdGlvbkNvbnRleHRSZXNwb25zZRJHCgdjb250" + "ZXh0GAEgASgLMjYuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQu" + "Q29uZmlndXJhdGlvbkNvbnRleHQSTgoJc25hcHNob3RzGAIgAygLMjsuSHlk" + "cmEuQXBpLkdhbWVDb25maWd1cmF0aW9uLkNvbmZpZ3VyYXRpb25Db21wb25l" + "bnRTbmFwc2hvdCK4AQokR2V0Q29uZmlndXJhdGlvbkNvbXBvbmVudERhdGFS" + "ZXF1ZXN0EkcKB2NvbnRleHQYASABKAsyNi5IeWRyYS5BcGkuSW5mcmFzdHJ1" + "Y3R1cmUuQ29udGV4dC5Db25maWd1cmF0aW9uQ29udGV4dBJHCgpjb21wb25l" + "bnRzGAIgAygLMjMuSHlkcmEuQXBpLkdhbWVDb25maWd1cmF0aW9uLkNvbmZp" + "Z3VyYXRpb25Db21wb25lbnQiagolR2V0Q29uZmlndXJhdGlvbkNvbXBvbmVu" + "dERhdGFSZXNwb25zZRJBCgdyZXN1bHRzGAEgAygLMjAuSHlkcmEuQXBpLkdh" + "bWVDb25maWd1cmF0aW9uLkNvbXBvbmVudERhdGFSZXN1bHQiNwoWQ29uZmln" + "dXJhdGlvbkNvbXBvbmVudBIMCgRuYW1lGAEgASgJEg8KB3ZlcnNpb24YAiAB" + "KAkigAEKHkNvbmZpZ3VyYXRpb25Db21wb25lbnRTbmFwc2hvdBJGCgljb21w" + "b25lbnQYASABKAsyMy5IeWRyYS5BcGkuR2FtZUNvbmZpZ3VyYXRpb24uQ29u" + "ZmlndXJhdGlvbkNvbXBvbmVudBIWCg5jb21wb25lbnRfaGFzaBgCIAEoCSLG" + "AQoTQ29tcG9uZW50RGF0YVJlc3VsdBJXChJjb21wb25lbnRfc25hcHNob3QY" + "ASABKAsyOy5IeWRyYS5BcGkuR2FtZUNvbmZpZ3VyYXRpb24uQ29uZmlndXJh" + "dGlvbkNvbXBvbmVudFNuYXBzaG90EhMKC2RhdGFfcmVzdWx0GAIgASgMEkEK" + "CWRhdGFfdHlwZRgDIAEoDjIuLkh5ZHJhLkFwaS5HYW1lQ29uZmlndXJhdGlv" + "bi5Db21wb25lbnREYXRhVHlwZSoxChFDb21wb25lbnREYXRhVHlwZRIcChhD" + "T01QT05FTlRfREFUQV9UWVBFX0pTT04QAGIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			UserContextReflection.Descriptor,
			ConfigurationContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(new Type[1] { typeof(ComponentDataType) }, null, new GeneratedClrTypeInfo[7]
		{
			new GeneratedClrTypeInfo(typeof(GetConfigurationContextRequest), GetConfigurationContextRequest.Parser, new string[2] { "Context", "Components" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetConfigurationContextResponse), GetConfigurationContextResponse.Parser, new string[2] { "Context", "Snapshots" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetConfigurationComponentDataRequest), GetConfigurationComponentDataRequest.Parser, new string[2] { "Context", "Components" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetConfigurationComponentDataResponse), GetConfigurationComponentDataResponse.Parser, new string[1] { "Results" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConfigurationComponent), ConfigurationComponent.Parser, new string[2] { "Name", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConfigurationComponentSnapshot), ConfigurationComponentSnapshot.Parser, new string[2] { "Component", "ComponentHash" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ComponentDataResult), ComponentDataResult.Parser, new string[3] { "ComponentSnapshot", "DataResult", "DataType" }, null, null, null, null)
		}));
	}
}
