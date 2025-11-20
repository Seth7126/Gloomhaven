using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.User.Zen;

public static class ZenEconomyApiContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static ZenEconomyApiContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiFVc2VyL1plbkVjb25vbXlBcGlDb250cmFjdHMucHJvdG8SEkh5ZHJhLkFw" + "aS5Vc2VyLlplbhoZQ29udGV4dC9Vc2VyQ29udGV4dC5wcm90bxoiQ29udGV4" + "dC9Db25maWd1cmF0aW9uQ29udGV4dC5wcm90bxobVXNlci9FY29ub215Q29u" + "dHJhY3RzLnByb3RvIt8BChJBcHBseU9mZmVyc1JlcXVlc3QSQwoMdXNlcl9j" + "b250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRl" + "eHQuVXNlckNvbnRleHQSVQoVY29uZmlndXJhdGlvbl9jb250ZXh0GAIgASgL" + "MjYuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuQ29uZmlndXJh" + "dGlvbkNvbnRleHQSLQoGb2ZmZXJzGAMgAygLMh0uSHlkcmEuQXBpLlVzZXIu" + "T2ZmZXJMaXN0SXRlbSIVChNBcHBseU9mZmVyc1Jlc3BvbnNlIvQBChpBcHBs" + "eURpc2NvdW50T2ZmZXJzUmVxdWVzdBJDCgx1c2VyX2NvbnRleHQYASABKAsy" + "LS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29udGV4" + "dBJVChVjb25maWd1cmF0aW9uX2NvbnRleHQYAiABKAsyNi5IeWRyYS5BcGku" + "SW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Db25maWd1cmF0aW9uQ29udGV4dBI6" + "Cg9kaXNjb3VudF9vZmZlcnMYAyADKAsyIS5IeWRyYS5BcGkuVXNlci5aZW4u" + "RGlzY291bnRPZmZlciIdChtBcHBseURpc2NvdW50T2ZmZXJzUmVzcG9uc2Ui" + "ewoNRGlzY291bnRPZmZlchIsCgVvZmZlchgBIAEoCzIdLkh5ZHJhLkFwaS5V" + "c2VyLk9mZmVyTGlzdEl0ZW0SKQoFcHJpY2UYAiABKAsyGi5IeWRyYS5BcGku" + "VXNlci5JbnQ2NFZhbHVlEhEKCWRpc2NvdW50cxgDIAMoCSKTAQoZU2VuZFBs" + "YXl0aW1lRXZlbnRzUmVxdWVzdBIxCgZldmVudHMYASADKAsyIS5IeWRyYS5B" + "cGkuVXNlci5aZW4uUGxheXRpbWVFdmVudBJDCgx1c2VyX2NvbnRleHQYAiAB" + "KAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29u" + "dGV4dCIcChpTZW5kUGxheXRpbWVFdmVudHNSZXNwb25zZSI3Cg1QbGF5dGlt" + "ZUV2ZW50Eg4KBnBpdF9pZBgBIAEoCRIWCg5hY3RpdmVfc2Vjb25kcxgCIAEo" + "BWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[3]
		{
			UserContextReflection.Descriptor,
			ConfigurationContextReflection.Descriptor,
			EconomyContractsReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[8]
		{
			new GeneratedClrTypeInfo(typeof(ApplyOffersRequest), ApplyOffersRequest.Parser, new string[3] { "UserContext", "ConfigurationContext", "Offers" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ApplyOffersResponse), ApplyOffersResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ApplyDiscountOffersRequest), ApplyDiscountOffersRequest.Parser, new string[3] { "UserContext", "ConfigurationContext", "DiscountOffers" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ApplyDiscountOffersResponse), ApplyDiscountOffersResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DiscountOffer), DiscountOffer.Parser, new string[3] { "Offer", "Price", "Discounts" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendPlaytimeEventsRequest), SendPlaytimeEventsRequest.Parser, new string[2] { "Events", "UserContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SendPlaytimeEventsResponse), SendPlaytimeEventsResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PlaytimeEvent), PlaytimeEvent.Parser, new string[2] { "PitId", "ActiveSeconds" }, null, null, null, null)
		}));
	}
}
