using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public static class KeyContainerReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static KeyContainerReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiFTZXNzaW9uQ29udHJvbC9LZXlDb250YWluZXIucHJvdG8SGEh5ZHJhLkFw" + "aS5TZXNzaW9uQ29udHJvbCJXCgxLZXlDb250YWluZXISCwoDa2V5GAEgASgM" + "EhAKCGhhc2hfa2V5GAIgASgMEhkKEWluaXRfdmVjdG9yX3ZhbHVlGAMgASgM" + "Eg0KBW5vbmNlGAQgASgMYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(KeyContainer), KeyContainer.Parser, new string[4] { "Key", "HashKey", "InitVectorValue", "Nonce" }, null, null, null, null)
		}));
	}
}
