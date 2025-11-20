using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace RedLynx.Api.Account;

public static class AccountContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static AccountContractsReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CipyZWRseW54LWFwaS9BY2NvdW50L0FjY291bnRDb250cmFjdHMucHJvdG8S" + "E1JlZEx5bnguQXBpLkFjY291bnQaGUNvbnRleHQvVXNlckNvbnRleHQucHJv" + "dG8iYwocR2V0UmVnaXN0cmF0aW9uUVJDb2RlUmVxdWVzdBJDCgx1c2VyX2Nv" + "bnRleHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4" + "dC5Vc2VyQ29udGV4dCLiAgodR2V0UmVnaXN0cmF0aW9uUVJDb2RlUmVzcG9u" + "c2USMQoGY29uZmlnGAcgASgLMiEuUmVkTHlueC5BcGkuQWNjb3VudC5RUkNv" + "ZGVDb25maWcSGQoRcmVnaXN0cmF0aW9uX2xpbmsYASABKAkSEwoLcXJfY29k" + "ZV9wbmcYAiABKAwSDAoEY29kZRgDIAEoCRI0CgtsaW5rX3N0YXR1cxgEIAEo" + "DjIfLlJlZEx5bnguQXBpLkFjY291bnQuTGlua1N0YXR1cxIdChVleHBpcmF0" + "aW9uX2luX3NlY29uZHMYBSABKAUSQwoTcmV0cmlldmFsX2ludGVydmFscxgG" + "IAMoCzImLlJlZEx5bnguQXBpLkFjY291bnQuUmV0cmlldmFsSW50ZXJ2YWwS" + "NgoMYWNjb3VudF9pbmZvGAggASgLMiAuUmVkTHlueC5BcGkuQWNjb3VudC5B" + "Y2NvdW50SW5mbyJKCgxRUkNvZGVDb25maWcSOgoLc3lzdGVtX21vZGUYASAB" + "KA4yJS5SZWRMeW54LkFwaS5BY2NvdW50LlFSQ29kZVN5c3RlbU1vZGUiWQoR" + "UmV0cmlldmFsSW50ZXJ2YWwSDQoFb3JkZXIYASABKAUSGwoTZHVyYXRpb25f" + "aW5fc2Vjb25kcxgCIAEoBRIYChBkZWxheV9pbl9zZWNvbmRzGAMgASgFInEK" + "HEdldFJlZ2lzdHJhdGlvblN0YXR1c1JlcXVlc3QSQwoMdXNlcl9jb250ZXh0" + "GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNl" + "ckNvbnRleHQSDAoEY29kZRgCIAEoCSKNAQodR2V0UmVnaXN0cmF0aW9uU3Rh" + "dHVzUmVzcG9uc2USNAoLbGlua19zdGF0dXMYASABKA4yHy5SZWRMeW54LkFw" + "aS5BY2NvdW50LkxpbmtTdGF0dXMSNgoMYWNjb3VudF9pbmZvGAIgASgLMiAu" + "UmVkTHlueC5BcGkuQWNjb3VudC5BY2NvdW50SW5mbyJOCgtBY2NvdW50SW5m" + "bxIQCgh1c2VybmFtZRgBIAEoCRIYChB1c2VybmFtZV9wb3N0Zml4GAIgASgJ" + "EhMKC2F2YXRhcl9saW5rGAMgASgJKlQKEFFSQ29kZVN5c3RlbU1vZGUSHgoa" + "UVJfQ09ERV9TWVNURU1fTU9ERV9OT1JNQUwQABIgChxRUl9DT0RFX1NZU1RF" + "TV9NT0RFX1RVUk5fT0ZGEAEqkgEKCkxpbmtTdGF0dXMSFwoTTElOS19TVEFU" + "VVNfVU5LTk9XThAAEhoKFkxJTktfU1RBVFVTX05PVF9MSU5LRUQQARIWChJM" + "SU5LX1NUQVRVU19MSU5LRUQQAhIcChhMSU5LX1NUQVRVU19DT0RFX1RJTUVP" + "VVQQAxIZChVMSU5LX1NUQVRVU19DT0RFX1JFQUQQBGIGcHJvdG8z"), new FileDescriptor[1] { UserContextReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[2]
		{
			typeof(QRCodeSystemMode),
			typeof(LinkStatus)
		}, null, new GeneratedClrTypeInfo[7]
		{
			new GeneratedClrTypeInfo(typeof(GetRegistrationQRCodeRequest), GetRegistrationQRCodeRequest.Parser, new string[1] { "UserContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetRegistrationQRCodeResponse), GetRegistrationQRCodeResponse.Parser, new string[8] { "Config", "RegistrationLink", "QrCodePng", "Code", "LinkStatus", "ExpirationInSeconds", "RetrievalIntervals", "AccountInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(QRCodeConfig), QRCodeConfig.Parser, new string[1] { "SystemMode" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RetrievalInterval), RetrievalInterval.Parser, new string[3] { "Order", "DurationInSeconds", "DelayInSeconds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetRegistrationStatusRequest), GetRegistrationStatusRequest.Parser, new string[2] { "UserContext", "Code" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetRegistrationStatusResponse), GetRegistrationStatusResponse.Parser, new string[2] { "LinkStatus", "AccountInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(AccountInfo), AccountInfo.Parser, new string[3] { "Username", "UsernamePostfix", "AvatarLink" }, null, null, null, null)
		}));
	}
}
