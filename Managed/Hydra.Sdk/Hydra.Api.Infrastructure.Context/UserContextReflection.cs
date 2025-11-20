using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public static class UserContextReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static UserContextReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvEiBIeWRyYS5BcGkuSW5mcmFz" + "dHJ1Y3R1cmUuQ29udGV4dCJhCgtVc2VyQ29udGV4dBI/CgRkYXRhGAEgASgL" + "MjEuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRl" + "eHREYXRhEhEKCXNpZ25hdHVyZRgCIAEoDCLMAQoPVXNlckNvbnRleHREYXRh" + "EhUKDXVzZXJfaWRlbnRpdHkYASABKAkSEAoIdGl0bGVfaWQYAiABKAkSEAoI" + "cGxhdGZvcm0YAyABKAkSGQoRa2VybmVsX3Nlc3Npb25faWQYBCABKAkSEwoL" + "cHJvdmlkZXJfaWQYBSABKAkSTgoSdXNlcl9pZGVudGl0eV90eXBlGAYgASgO" + "MjIuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlcklkZW50" + "aXR5VHlwZSp9ChBVc2VySWRlbnRpdHlUeXBlEh4KGlVTRVJfSURFTlRJVFlf" + "VFlQRV9VTktOT1dOEAASIwofVVNFUl9JREVOVElUWV9UWVBFX1BVUkVfVVNF" + "Ul9JRBABEiQKIFVTRVJfSURFTlRJVFlfVFlQRV9BTk9OWU1PVVNfSldFEAJi" + "BnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(UserIdentityType) }, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(UserContext), UserContext.Parser, new string[2] { "Data", "Signature" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserContextData), UserContextData.Parser, new string[6] { "UserIdentity", "TitleId", "Platform", "KernelSessionId", "ProviderId", "UserIdentityType" }, null, null, null, null)
		}));
	}
}
