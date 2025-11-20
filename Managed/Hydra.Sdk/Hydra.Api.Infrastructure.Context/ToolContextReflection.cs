using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public static class ToolContextReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ToolContextReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChlDb250ZXh0L1Rvb2xDb250ZXh0LnByb3RvEiBIeWRyYS5BcGkuSW5mcmFz" + "dHJ1Y3R1cmUuQ29udGV4dCJhCgtUb29sQ29udGV4dBI/CgRkYXRhGAEgASgL" + "MjEuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVG9vbENvbnRl" + "eHREYXRhEhEKCXNpZ25hdHVyZRgCIAEoDCI+Cg9Ub29sQ29udGV4dERhdGES" + "EAoIdGl0bGVfaWQYASABKAkSGQoRa2VybmVsX3Nlc3Npb25faWQYAiABKAli" + "BnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(ToolContext), ToolContext.Parser, new string[2] { "Data", "Signature" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ToolContextData), ToolContextData.Parser, new string[2] { "TitleId", "KernelSessionId" }, null, null, null, null)
		}));
	}
}
