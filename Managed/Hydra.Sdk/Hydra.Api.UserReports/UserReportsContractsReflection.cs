using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.UserReports;

public static class UserReportsContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static UserReportsContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiZVc2VyUmVwb3J0cy9Vc2VyUmVwb3J0c0NvbnRyYWN0cy5wcm90bxIVSHlk" + "cmEuQXBpLlVzZXJSZXBvcnRzGh9nb29nbGUvcHJvdG9idWYvdGltZXN0YW1w" + "LnByb3RvGhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvIjIKE1VzZXJSZXBv" + "cnRzUHJvcGVydHkSDAoEbmFtZRgBIAEoCRINCgV2YWx1ZRgCIAEoCSLtAQoV" + "U2VuZFVzZXJSZXBvcnRSZXF1ZXN0EkMKDHVzZXJfY29udGV4dBgBIAEoCzIt" + "Lkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0" + "EhIKCnRvX3VzZXJfaWQYAiABKAkSGAoQcmVwb3J0X3JlYXNvbl9pZBgDIAEo" + "CRIUCgx1c2VyX21lc3NhZ2UYBCABKAkSSwoXdXNlcl9yZXBvcnRzX3Byb3Bl" + "cnRpZXMYBSADKAsyKi5IeWRyYS5BcGkuVXNlclJlcG9ydHMuVXNlclJlcG9y" + "dHNQcm9wZXJ0eSJtChZTZW5kVXNlclJlcG9ydFJlc3BvbnNlEjsKBnJlc3Vs" + "dBgBIAEoDjIrLkh5ZHJhLkFwaS5Vc2VyUmVwb3J0cy5TZW5kVXNlclJlcG9y" + "dFJlc3VsdBIWCg51c2VyX3JlcG9ydF9pZBgCIAEoCSq+AQoUU2VuZFVzZXJS" + "ZXBvcnRSZXN1bHQSIAocU0VORF9VU0VSX1JFUE9SVF9SRVNVTFRfTk9ORRAA" + "EiMKH1NFTkRfVVNFUl9SRVBPUlRfUkVTVUxUX1NVQ0NFU1MQARIvCitTRU5E" + "X1VTRVJfUkVQT1JUX1JFU1VMVF9EQUlMWV9MSU1JVF9SRUFDSEVEEAISLgoq" + "U0VORF9VU0VSX1JFUE9SVF9SRVNVTFRfVVNFUl9MSU1JVF9SRUFDSEVEEANi" + "BnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			TimestampReflection.Descriptor,
			UserContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(new System.Type[1] { typeof(SendUserReportResult) }, null, new GeneratedClrTypeInfo[3]
		{
			new GeneratedClrTypeInfo(typeof(UserReportsProperty), UserReportsProperty.Parser, new string[2] { "Name", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendUserReportRequest), SendUserReportRequest.Parser, new string[5] { "UserContext", "ToUserId", "ReportReasonId", "UserMessage", "UserReportsProperties" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendUserReportResponse), SendUserReportResponse.Parser, new string[2] { "Result", "UserReportId" }, null, null, null, null)
		}));
	}
}
