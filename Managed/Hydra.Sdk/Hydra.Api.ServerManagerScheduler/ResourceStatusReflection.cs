using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.ServerManagerScheduler;

public static class ResourceStatusReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ResourceStatusReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CitTZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyL1Jlc291cmNlU3RhdHVzLnByb3Rv" + "EiBIeWRyYS5BcGkuU2VydmVyTWFuYWdlclNjaGVkdWxlciItCg5SZXNvdXJj" + "ZVN0YXR1cxIMCgR1c2VkGAEgASgDEg0KBXRvdGFsGAIgASgDIpMBChBWbVJl" + "c291cmNlU3RhdHVzEj0KA2NwdRgBIAEoCzIwLkh5ZHJhLkFwaS5TZXJ2ZXJN" + "YW5hZ2VyU2NoZWR1bGVyLlJlc291cmNlU3RhdHVzEkAKBm1lbW9yeRgCIAEo" + "CzIwLkh5ZHJhLkFwaS5TZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyLlJlc291cmNl" + "U3RhdHVzYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(ResourceStatus), ResourceStatus.Parser, new string[2] { "Used", "Total" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(VmResourceStatus), VmResourceStatus.Parser, new string[2] { "Cpu", "Memory" }, null, null, null, null)
		}));
	}
}
