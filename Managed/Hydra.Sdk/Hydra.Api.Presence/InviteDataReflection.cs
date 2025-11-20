using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public static class InviteDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static InviteDataReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChlQcmVzZW5jZS9JbnZpdGVEYXRhLnByb3RvEhJIeWRyYS5BcGkuUHJlc2Vu" + "Y2UiWwoKSW52aXRlRGF0YRIRCglpbnZpdGVfaWQYASABKAkSEAoIcGFydHlf" + "aWQYAiABKAkSFAoMdXNlcl9pZF9mcm9tGAMgASgJEhIKCnVzZXJfaWRfdG8Y" + "BCABKAliBnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(InviteData), InviteData.Parser, new string[4] { "InviteId", "PartyId", "UserIdFrom", "UserIdTo" }, null, null, null, null)
		}));
	}
}
