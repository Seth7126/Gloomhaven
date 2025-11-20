using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.AbstractData;

public static class AbstractDataServiceContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static AbstractDataServiceContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Ci9BYnN0cmFjdERhdGEvQWJzdHJhY3REYXRhU2VydmljZUNvbnRyYWN0cy5w" + "cm90bxIWSHlkcmEuQXBpLkFic3RyYWN0RGF0YRoZQ29udGV4dC9Vc2VyQ29u" + "dGV4dC5wcm90bxobQ29udGV4dC9TZXJ2ZXJDb250ZXh0LnByb3RvIkMKEkFi" + "c3RyYWN0RGF0YVJlY29yZBIOCgZsYXlvdXQYASABKAMSDwoHdmVyc2lvbhgC" + "IAEoAxIMCgRkYXRhGAMgASgMIm8KGUFic3RyYWN0RGF0YUNvbnRhaW5lckRh" + "dGESFgoOY29udGFpbmVyX25hbWUYASABKAkSOgoGcmVjb3JkGAIgASgLMiou" + "SHlkcmEuQXBpLkFic3RyYWN0RGF0YS5BYnN0cmFjdERhdGFSZWNvcmQibwoZ" + "QWJzdHJhY3REYXRhS2V5Q29udGFpbmVycxILCgNrZXkYASABKAkSRQoKY29u" + "dGFpbmVycxgCIAMoCzIxLkh5ZHJhLkFwaS5BYnN0cmFjdERhdGEuQWJzdHJh" + "Y3REYXRhQ29udGFpbmVyRGF0YSJFCh1BYnN0cmFjdERhdGFLZXlDb250YWlu" + "ZXJOYW1lcxILCgNrZXkYASABKAkSFwoPY29udGFpbmVyX25hbWVzGAIgAygJ" + "IqQBCg5HZXREYXRhUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEu" + "QXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQSUgoTa2V5" + "X2NvbnRhaW5lcl9uYW1lcxgCIAMoCzI1Lkh5ZHJhLkFwaS5BYnN0cmFjdERh" + "dGEuQWJzdHJhY3REYXRhS2V5Q29udGFpbmVyTmFtZXMiUgoPR2V0RGF0YVJl" + "c3BvbnNlEj8KBGRhdGEYASADKAsyMS5IeWRyYS5BcGkuQWJzdHJhY3REYXRh" + "LkFic3RyYWN0RGF0YUtleUNvbnRhaW5lcnMikQEKDlNldERhdGFSZXF1ZXN0" + "Ej4KB2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUu" + "Q29udGV4dC5Vc2VyQ29udGV4dBI/CgRkYXRhGAIgAygLMjEuSHlkcmEuQXBp" + "LkFic3RyYWN0RGF0YS5BYnN0cmFjdERhdGFLZXlDb250YWluZXJzIhEKD1Nl" + "dERhdGFSZXNwb25zZSKsAQoUR2V0RGF0YVNlcnZlclJlcXVlc3QSQAoHY29u" + "dGV4dBgBIAEoCzIvLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0" + "LlNlcnZlckNvbnRleHQSUgoTa2V5X2NvbnRhaW5lcl9uYW1lcxgCIAMoCzI1" + "Lkh5ZHJhLkFwaS5BYnN0cmFjdERhdGEuQWJzdHJhY3REYXRhS2V5Q29udGFp" + "bmVyTmFtZXMiWAoVR2V0RGF0YVNlcnZlclJlc3BvbnNlEj8KBGRhdGEYASAD" + "KAsyMS5IeWRyYS5BcGkuQWJzdHJhY3REYXRhLkFic3RyYWN0RGF0YUtleUNv" + "bnRhaW5lcnMimQEKFFNldERhdGFTZXJ2ZXJSZXF1ZXN0EkAKB2NvbnRleHQY" + "ASABKAsyLy5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5TZXJ2" + "ZXJDb250ZXh0Ej8KBGRhdGEYAiADKAsyMS5IeWRyYS5BcGkuQWJzdHJhY3RE" + "YXRhLkFic3RyYWN0RGF0YUtleUNvbnRhaW5lcnMiFwoVU2V0RGF0YVNlcnZl" + "clJlc3BvbnNlYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[12]
		{
			new GeneratedClrTypeInfo(typeof(AbstractDataRecord), AbstractDataRecord.Parser, new string[3] { "Layout", "Version", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(AbstractDataContainerData), AbstractDataContainerData.Parser, new string[2] { "ContainerName", "Record" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(AbstractDataKeyContainers), AbstractDataKeyContainers.Parser, new string[2] { "Key", "Containers" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(AbstractDataKeyContainerNames), AbstractDataKeyContainerNames.Parser, new string[2] { "Key", "ContainerNames" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataRequest), GetDataRequest.Parser, new string[2] { "Context", "KeyContainerNames" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataResponse), GetDataResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SetDataRequest), SetDataRequest.Parser, new string[2] { "Context", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SetDataResponse), SetDataResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataServerRequest), GetDataServerRequest.Parser, new string[2] { "Context", "KeyContainerNames" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetDataServerResponse), GetDataServerResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SetDataServerRequest), SetDataServerRequest.Parser, new string[2] { "Context", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SetDataServerResponse), SetDataServerResponse.Parser, null, null, null, null, null)
		}));
	}
}
