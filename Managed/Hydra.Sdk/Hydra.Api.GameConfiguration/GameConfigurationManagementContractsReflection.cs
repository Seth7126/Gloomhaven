using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.GameConfiguration;

public static class GameConfigurationManagementContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static GameConfigurationManagementContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CjxHYW1lQ29uZmlndXJhdGlvbi9HYW1lQ29uZmlndXJhdGlvbk1hbmFnZW1l" + "bnRDb250cmFjdHMucHJvdG8SG0h5ZHJhLkFwaS5HYW1lQ29uZmlndXJhdGlv" + "bhoZQ29udGV4dC9Ub29sQ29udGV4dC5wcm90bxoiQ29udGV4dC9Db25maWd1" + "cmF0aW9uQ29udGV4dC5wcm90bxoyR2FtZUNvbmZpZ3VyYXRpb24vR2FtZUNv" + "bmZpZ3VyYXRpb25Db250cmFjdHMucHJvdG8ihAEKKFVwbG9hZENvbmZpZ3Vy" + "YXRpb25Db21wb25lbnRQYWNrc1JlcXVlc3QSEAoIdGl0bGVfaWQYASABKAkS" + "RgoFcGFja3MYAiADKAsyNy5IeWRyYS5BcGkuR2FtZUNvbmZpZ3VyYXRpb24u" + "Q29uZmlndXJhdGlvbkNvbXBvbmVudFBhY2sidAopVXBsb2FkQ29uZmlndXJh" + "dGlvbkNvbXBvbmVudFBhY2tzUmVzcG9uc2USRwoHY29udGV4dBgBIAEoCzI2" + "Lkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LkNvbmZpZ3VyYXRp" + "b25Db250ZXh0Ij4KKkRvd25sb2FkQ29uZmlndXJhdGlvbkNvbXBvbmVudFBh" + "Y2tzUmVxdWVzdBIQCgh0aXRsZV9pZBgBIAEoCSJ1CitEb3dubG9hZENvbmZp" + "Z3VyYXRpb25Db21wb25lbnRQYWNrc1Jlc3BvbnNlEkYKBXBhY2tzGAEgAygL" + "MjcuSHlkcmEuQXBpLkdhbWVDb25maWd1cmF0aW9uLkNvbmZpZ3VyYXRpb25D" + "b21wb25lbnRQYWNrIrUBChpDb25maWd1cmF0aW9uQ29tcG9uZW50UGFjaxJM" + "CgdzY2hlbWFzGAEgAygLMjsuSHlkcmEuQXBpLkdhbWVDb25maWd1cmF0aW9u" + "LkNvbmZpZ3VyYXRpb25Db21wb25lbnREYXRhSXRlbRJJCgRkYXRhGAIgASgL" + "MjsuSHlkcmEuQXBpLkdhbWVDb25maWd1cmF0aW9uLkNvbmZpZ3VyYXRpb25D" + "b21wb25lbnREYXRhSXRlbSJ9Ch5Db25maWd1cmF0aW9uQ29tcG9uZW50RGF0" + "YUl0ZW0SDAoEbmFtZRgBIAEoCRIPCgdjb250ZW50GAIgASgMEjwKBHR5cGUY" + "AyABKA4yLi5IeWRyYS5BcGkuR2FtZUNvbmZpZ3VyYXRpb24uQ29tcG9uZW50" + "RGF0YVR5cGUiuwEKLFVwbG9hZENvbmZpZ3VyYXRpb25Db21wb25lbnRQYWNr" + "c1Rvb2xSZXF1ZXN0EkMKDHRvb2xfY29udGV4dBgBIAEoCzItLkh5ZHJhLkFw" + "aS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlRvb2xDb250ZXh0EkYKBXBhY2tz" + "GAIgAygLMjcuSHlkcmEuQXBpLkdhbWVDb25maWd1cmF0aW9uLkNvbmZpZ3Vy" + "YXRpb25Db21wb25lbnRQYWNrIngKLVVwbG9hZENvbmZpZ3VyYXRpb25Db21w" + "b25lbnRQYWNrc1Rvb2xSZXNwb25zZRJHCgdjb250ZXh0GAEgASgLMjYuSHlk" + "cmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuQ29uZmlndXJhdGlvbkNv" + "bnRleHQidQouRG93bmxvYWRDb25maWd1cmF0aW9uQ29tcG9uZW50UGFja3NU" + "b29sUmVxdWVzdBJDCgx0b29sX2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGku" + "SW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Ub29sQ29udGV4dCJ5Ci9Eb3dubG9h" + "ZENvbmZpZ3VyYXRpb25Db21wb25lbnRQYWNrc1Rvb2xSZXNwb25zZRJGCgVw" + "YWNrcxgBIAMoCzI3Lkh5ZHJhLkFwaS5HYW1lQ29uZmlndXJhdGlvbi5Db25m" + "aWd1cmF0aW9uQ29tcG9uZW50UGFjayJ8CjVEb3dubG9hZERlZmF1bHRDb25m" + "aWd1cmF0aW9uQ29tcG9uZW50UGFja3NUb29sUmVxdWVzdBJDCgx0b29sX2Nv" + "bnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4" + "dC5Ub29sQ29udGV4dCKAAQo2RG93bmxvYWREZWZhdWx0Q29uZmlndXJhdGlv" + "bkNvbXBvbmVudFBhY2tzVG9vbFJlc3BvbnNlEkYKBXBhY2tzGAEgAygLMjcu" + "SHlkcmEuQXBpLkdhbWVDb25maWd1cmF0aW9uLkNvbmZpZ3VyYXRpb25Db21w" + "b25lbnRQYWNrYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[3]
		{
			ToolContextReflection.Descriptor,
			ConfigurationContextReflection.Descriptor,
			GameConfigurationContractsReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[12]
		{
			new GeneratedClrTypeInfo(typeof(UploadConfigurationComponentPacksRequest), UploadConfigurationComponentPacksRequest.Parser, new string[2] { "TitleId", "Packs" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UploadConfigurationComponentPacksResponse), UploadConfigurationComponentPacksResponse.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DownloadConfigurationComponentPacksRequest), DownloadConfigurationComponentPacksRequest.Parser, new string[1] { "TitleId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DownloadConfigurationComponentPacksResponse), DownloadConfigurationComponentPacksResponse.Parser, new string[1] { "Packs" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConfigurationComponentPack), ConfigurationComponentPack.Parser, new string[2] { "Schemas", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConfigurationComponentDataItem), ConfigurationComponentDataItem.Parser, new string[3] { "Name", "Content", "Type" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UploadConfigurationComponentPacksToolRequest), UploadConfigurationComponentPacksToolRequest.Parser, new string[2] { "ToolContext", "Packs" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UploadConfigurationComponentPacksToolResponse), UploadConfigurationComponentPacksToolResponse.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DownloadConfigurationComponentPacksToolRequest), DownloadConfigurationComponentPacksToolRequest.Parser, new string[1] { "ToolContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DownloadConfigurationComponentPacksToolResponse), DownloadConfigurationComponentPacksToolResponse.Parser, new string[1] { "Packs" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DownloadDefaultConfigurationComponentPacksToolRequest), DownloadDefaultConfigurationComponentPacksToolRequest.Parser, new string[1] { "ToolContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DownloadDefaultConfigurationComponentPacksToolResponse), DownloadDefaultConfigurationComponentPacksToolResponse.Parser, new string[1] { "Packs" }, null, null, null, null)
		}));
	}
}
