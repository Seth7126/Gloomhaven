using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public static class SessionFinishReasonReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static SessionFinishReasonReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CihTZXNzaW9uQ29udHJvbC9TZXNzaW9uRmluaXNoUmVhc29uLnByb3RvEhhI" + "eWRyYS5BcGkuU2Vzc2lvbkNvbnRyb2wq8AIKE1Nlc3Npb25GaW5pc2hSZWFz" + "b24SHgoaU0VTU0lPTl9GSU5JU0hfUkVBU09OX05PTkUQABIgChxTRVNTSU9O" + "X0ZJTklTSF9SRUFTT05fTk9STUFMEAESKQolU0VTU0lPTl9GSU5JU0hfUkVB" + "U09OX05PX01BVENISU5HX0RTTRACEioKJlNFU1NJT05fRklOSVNIX1JFQVNP" + "Tl9USU1FT1VUX0FDVElWQVRFEAMSJAogU0VTU0lPTl9GSU5JU0hfUkVBU09O" + "X1RJTUVPVVRfRFMQBBIiCh5TRVNTSU9OX0ZJTklTSF9SRUFTT05fUkVKRUNU" + "RUQQBRImCiJTRVNTSU9OX0ZJTklTSF9SRUFTT05fTk9fU0xPVFNfRFNNEAYS" + "JQohU0VTU0lPTl9GSU5JU0hfUkVBU09OX05PX1BST1ZJREVSEAcSJwojU0VT" + "U0lPTl9GSU5JU0hfUkVBU09OX1RJTUVPVVRfUVVFVUUQCGIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(SessionFinishReason) }, null, null));
	}
}
