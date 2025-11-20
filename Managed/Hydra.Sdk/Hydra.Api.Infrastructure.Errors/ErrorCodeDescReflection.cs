using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Errors;

public static class ErrorCodeDescReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ErrorCodeDescReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChpFcnJvcnMvRXJyb3JDb2RlRGVzYy5wcm90bxIfSHlkcmEuQXBpLkluZnJh" + "c3RydWN0dXJlLkVycm9ycxogZ29vZ2xlL3Byb3RvYnVmL2Rlc2NyaXB0b3Iu" + "cHJvdG8iswEKDUVycm9yQ29kZURlc2MSEgoKZXJyb3JfdGV4dBgBIAEoCRJG" + "Cg5lcnJvcl9zZXZlcml0eRgCIAEoDjIuLkh5ZHJhLkFwaS5JbmZyYXN0cnVj" + "dHVyZS5FcnJvcnMuRXJyb3JTZXZlcml0eRJGCg5lcnJvcl9jYXRlZ29yeRgD" + "IAEoDjIuLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5FcnJvcnMuRXJyb3JD" + "YXRlZ29yeSp8Cg1FcnJvckNhdGVnb3J5EhgKFEVSUk9SX0NBVEVHT1JZX0ZB" + "VEFMEAASGwoXRVJST1JfQ0FURUdPUllfQ1JJVElDQUwQARIbChdFUlJPUl9D" + "QVRFR09SWV9URVJNSU5BTBACEhcKE0VSUk9SX0NBVEVHT1JZX05PTkUQAyq1" + "AQoNRXJyb3JTZXZlcml0eRIcChhFUlJPUl9TRVZFUklUWV9VTkRFRklORUQQ" + "ABIYChRFUlJPUl9TRVZFUklUWV9GQVRBTBABEhsKF0VSUk9SX1NFVkVSSVRZ" + "X0NSSVRJQ0FMEAISGAoURVJST1JfU0VWRVJJVFlfTUFKT1IQAxIYChRFUlJP" + "Ul9TRVZFUklUWV9NSU5PUhAEEhsKF0VSUk9SX1NFVkVSSVRZX0VYUEVDVEVE" + "EAU6YAoEZGVzYxIhLmdvb2dsZS5wcm90b2J1Zi5FbnVtVmFsdWVPcHRpb25z" + "GNIJIAEoCzIuLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5FcnJvcnMuRXJy" + "b3JDb2RlRGVzY2IGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { DescriptorReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[2]
		{
			typeof(ErrorCategory),
			typeof(ErrorSeverity)
		}, new Extension[1] { ErrorCodeDescExtensions.Desc }, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ErrorCodeDesc), ErrorCodeDesc.Parser, new string[3] { "ErrorText", "ErrorSeverity", "ErrorCategory" }, null, null, null, null)
		}));
	}
}
