using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public static class PushVersionReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushVersionReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChZQdXNoL1B1c2hWZXJzaW9uLnByb3RvEg5IeWRyYS5BcGkuUHVzaBoaUHVz" + "aC9QdXNoTWVzc2FnZVR5cGUucHJvdG8iVQoLUHVzaFZlcnNpb24SNQoMdmVy" + "c2lvbl90eXBlGAEgASgOMh8uSHlkcmEuQXBpLlB1c2guUHVzaE1lc3NhZ2VU" + "eXBlEg8KB3ZlcnNpb24YAiABKAViBnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { PushMessageTypeReflection.Descriptor }, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PushVersion), PushVersion.Parser, new string[2] { "VersionType", "Version" }, null, null, null, null)
		}));
	}
}
