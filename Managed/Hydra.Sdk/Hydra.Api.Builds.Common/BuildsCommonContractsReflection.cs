using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Builds.Common;

public static class BuildsCommonContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static BuildsCommonContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiJCdWlsZHMvQnVpbGRzQ29tbW9uQ29udHJhY3RzLnByb3RvEhdIeWRyYS5B" + "cGkuQnVpbGRzLkNvbW1vbiI0ChVCdWlsZFZlcnNpb25XaXRoSWREdG8SCgoC" + "aWQYASABKAkSDwoHdmVyc2lvbhgCIAEoCWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BuildVersionWithIdDto), BuildVersionWithIdDto.Parser, new string[2] { "Id", "Version" }, null, null, null, null)
		}));
	}
}
