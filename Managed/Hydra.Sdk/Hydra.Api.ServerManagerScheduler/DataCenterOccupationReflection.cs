using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.ServerManagerScheduler;

public static class DataCenterOccupationReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static DataCenterOccupationReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CjFTZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyL0RhdGFDZW50ZXJPY2N1cGF0aW9u" + "LnByb3RvEiBIeWRyYS5BcGkuU2VydmVyTWFuYWdlclNjaGVkdWxlciJCChRE" + "YXRhQ2VudGVyT2NjdXBhdGlvbhIWCg5kYXRhX2NlbnRlcl9pZBgBIAEoCRIS" + "CgpvY2N1cGF0aW9uGAIgASgFYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DataCenterOccupation), DataCenterOccupation.Parser, new string[2] { "DataCenterId", "Occupation" }, null, null, null, null)
		}));
	}
}
