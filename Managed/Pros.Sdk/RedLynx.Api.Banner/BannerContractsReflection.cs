using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;
using RedLynx.Api.Localization;

namespace RedLynx.Api.Banner;

public static class BannerContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static BannerContractsReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CihyZWRseW54LWFwaS9CYW5uZXIvQmFubmVyQ29udHJhY3RzLnByb3RvEhJS" + "ZWRMeW54LkFwaS5CYW5uZXIaGUNvbnRleHQvVXNlckNvbnRleHQucHJvdG8a" + "IkxvY2FsaXphdGlvbi9Mb2NhbGl6ZWRTdHJpbmcucHJvdG8ihAEKEUdldEJh" + "bm5lcnNSZXF1ZXN0EkMKDHVzZXJfY29udGV4dBgBIAEoCzItLkh5ZHJhLkFw" + "aS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0EhQKDGVudGl0" + "bGVtZW50cxgCIAMoCRIUCgxyZWZlcmVuY2VfaWQYAyABKAkidwoSR2V0QmFu" + "bmVyc1Jlc3BvbnNlEjAKBmNvbmZpZxgBIAEoCzIgLlJlZEx5bnguQXBpLkJh" + "bm5lci5CYW5uZXJDb25maWcSLwoHYmFubmVycxgCIAMoCzIeLlJlZEx5bngu" + "QXBpLkJhbm5lci5CYW5uZXJJbmZvIp8CCgpCYW5uZXJJbmZvEhMKC2NhbXBh" + "aWduX2lkGAEgASgJEhEKCWltYWdlX3VybBgCIAEoCRIUCgxyZWRpcmVjdF91" + "cmwYAyABKAkSMwoIdXJsX3R5cGUYByABKA4yIS5SZWRMeW54LkFwaS5CYW5u" + "ZXIuQmFubmVyVXJsVHlwZRI0CghzZXZlcml0eRgEIAEoDjIiLlJlZEx5bngu" + "QXBpLkJhbm5lci5CYW5uZXJTZXZlcml0eRIzCgtiYW5uZXJfdHlwZRgFIAEo" + "DjIeLlJlZEx5bnguQXBpLkJhbm5lci5CYW5uZXJUeXBlEjMKCG1lc3NhZ2Vz" + "GAYgAygLMiEuUmVkTHlueC5BcGkuQmFubmVyLkJhbm5lck1lc3NhZ2UirAEK" + "DEJhbm5lckNvbmZpZxIbChNkaXNwbGF5X2R1cmF0aW9uX21zGAEgASgFEkQK" + "FG1pbl9zZXZlcml0eV9kaXNwbGF5GAIgASgOMiIuUmVkTHlueC5BcGkuQmFu" + "bmVyLkJhbm5lclNldmVyaXR5QgIYARI5CgtzeXN0ZW1fbW9kZRgDIAEoDjIk" + "LlJlZEx5bnguQXBpLkJhbm5lci5CYW5uZXJTeXN0ZW1Nb2RlIlwKDUJhbm5l" + "ck1lc3NhZ2USCgoCaWQYASABKAkSPwoMZGVzY3JpcHRpb25zGAIgAygLMiku" + "UmVkTHlueC5BcGkuTG9jYWxpemF0aW9uLkxvY2FsaXplZFN0cmluZyqHAQoN" + "QmFubmVyVXJsVHlwZRIgChxCQU5ORVJfVVJMX1RZUEVfRVhURVJOQUxfVVJM" + "EAASJwojQkFOTkVSX1VSTF9UWVBFX0lOX0FQUF9HQU1FX0NPTlRFTlQQARIr" + "CidCQU5ORVJfVVJMX1RZUEVfSU5fQVBQX0VYVEVSTkFMX0NPTlRFTlQQAipJ" + "Cg5CYW5uZXJTZXZlcml0eRIYChRCQU5ORVJfU0VWRVJJVFlfTk9ORRAAEh0K" + "GUJBTk5FUl9TRVZFUklUWV9JTVBPUlRBTlQQASo5CgpCYW5uZXJUeXBlEhQK" + "EEJBTk5FUl9UWVBFX1RJUFMQABIVChFCQU5ORVJfVFlQRV9QUk9NTxABKlIK" + "EEJhbm5lclN5c3RlbU1vZGUSHQoZQkFOTkVSX1NZU1RFTV9NT0RFX05PUk1B" + "TBAAEh8KG0JBTk5FUl9TWVNURU1fTU9ERV9UVVJOX09GRhABYgZwcm90bzM="), new FileDescriptor[2]
		{
			UserContextReflection.Descriptor,
			LocalizedStringReflection.Descriptor
		}, new GeneratedClrTypeInfo(new Type[4]
		{
			typeof(BannerUrlType),
			typeof(BannerSeverity),
			typeof(BannerType),
			typeof(BannerSystemMode)
		}, null, new GeneratedClrTypeInfo[5]
		{
			new GeneratedClrTypeInfo(typeof(GetBannersRequest), GetBannersRequest.Parser, new string[3] { "UserContext", "Entitlements", "ReferenceId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetBannersResponse), GetBannersResponse.Parser, new string[2] { "Config", "Banners" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(BannerInfo), BannerInfo.Parser, new string[7] { "CampaignId", "ImageUrl", "RedirectUrl", "UrlType", "Severity", "BannerType", "Messages" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(BannerConfig), BannerConfig.Parser, new string[3] { "DisplayDurationMs", "MinSeverityDisplay", "SystemMode" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(BannerMessage), BannerMessage.Parser, new string[2] { "Id", "Descriptions" }, null, null, null, null)
		}));
	}
}
