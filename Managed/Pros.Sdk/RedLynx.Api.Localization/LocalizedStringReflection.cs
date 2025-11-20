using System;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Localization;

public static class LocalizedStringReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static LocalizedStringReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci5yZWRseW54LWFwaS9Mb2NhbGl6YXRpb24vTG9jYWxpemVkU3RyaW5nLnBy" + "b3RvEhhSZWRMeW54LkFwaS5Mb2NhbGl6YXRpb24iTAoPTG9jYWxpemVkU3Ry" + "aW5nEhUKDWxhbmd1YWdlX2NvZGUYASABKAkSFAoMY291bnRyeV9jb2RlGAIg" + "ASgJEgwKBHRleHQYAyABKAliBnByb3RvMw=="), new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LocalizedString), LocalizedString.Parser, new string[3] { "LanguageCode", "CountryCode", "Text" }, null, null, null, null)
		}));
	}
}
