using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Auth;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.User;

public static class UserContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static UserContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChhVc2VyL1VzZXJDb250cmFjdHMucHJvdG8SDkh5ZHJhLkFwaS5Vc2VyGhlD" + "b250ZXh0L1VzZXJDb250ZXh0LnByb3RvGiFBdXRoL0F1dGhvcml6YXRpb25D" + "b250cmFjdHMucHJvdG8iuwEKJlVzZXJzUHVibGljRGF0YUJ5UHJvdmlkZXJV" + "c2VySWRSZXF1ZXN0EkMKDHVzZXJfY29udGV4dBgBIAEoCzItLkh5ZHJhLkFw" + "aS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0Ei0KC3Byb3Zp" + "ZGVyX2lkGAIgASgOMhguSHlkcmEuQXBpLkF1dGguUHJvdmlkZXISHQoVcHJv" + "dmlkZXJfdXNlcl9pZF9saXN0GAMgAygJIlUKJ1VzZXJzUHVibGljRGF0YUJ5" + "UHJvdmlkZXJVc2VySWRSZXNwb25zZRIqCgRkYXRhGAEgAygLMhwuSHlkcmEu" + "QXBpLlVzZXIuVXNlckJhc2VEYXRhInsKHlVzZXJzUHVibGljRGF0YUJ5VXNl" + "cklkUmVxdWVzdBJDCgx1c2VyX2NvbnRleHQYASABKAsyLS5IeWRyYS5BcGku" + "SW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4dBIUCgx1c2VyX2lk" + "X2xpc3QYAiADKAkiTQofVXNlcnNQdWJsaWNEYXRhQnlVc2VySWRSZXNwb25z" + "ZRIqCgRkYXRhGAEgAygLMhwuSHlkcmEuQXBpLlVzZXIuVXNlckJhc2VEYXRh" + "InoKDFVzZXJCYXNlRGF0YRIPCgd1c2VyX2lkGAEgASgJEi0KC3Byb3ZpZGVy" + "X2lkGAIgASgOMhguSHlkcmEuQXBpLkF1dGguUHJvdmlkZXISGAoQcHJvdmlk" + "ZXJfdXNlcl9pZBgDIAEoCRIQCghwbGF0Zm9ybRgEIAEoCWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[2]
		{
			UserContextReflection.Descriptor,
			AuthorizationContractsReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[5]
		{
			new GeneratedClrTypeInfo(typeof(UsersPublicDataByProviderUserIdRequest), UsersPublicDataByProviderUserIdRequest.Parser, new string[3] { "UserContext", "ProviderId", "ProviderUserIdList" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UsersPublicDataByProviderUserIdResponse), UsersPublicDataByProviderUserIdResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UsersPublicDataByUserIdRequest), UsersPublicDataByUserIdRequest.Parser, new string[2] { "UserContext", "UserIdList" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UsersPublicDataByUserIdResponse), UsersPublicDataByUserIdResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserBaseData), UserBaseData.Parser, new string[4] { "UserId", "ProviderId", "ProviderUserId", "Platform" }, null, null, null, null)
		}));
	}
}
