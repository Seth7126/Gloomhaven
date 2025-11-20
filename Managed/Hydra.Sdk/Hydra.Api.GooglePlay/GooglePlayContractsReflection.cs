using System;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.GooglePlay;

public static class GooglePlayContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static GooglePlayContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CiRHb29nbGVQbGF5L0dvb2dsZVBsYXlDb250cmFjdHMucHJvdG8SFEh5ZHJh" + "LkFwaS5Hb29nbGVQbGF5GhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvIqAB" + "Ch1WYWxpZGF0ZUdvb2dsZVB1cmNoYXNlUmVxdWVzdBJDCgx1c2VyX2NvbnRl" + "eHQYASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5V" + "c2VyQ29udGV4dBI6CghwdXJjaGFzZRgCIAEoCzIoLkh5ZHJhLkFwaS5Hb29n" + "bGVQbGF5Lkdvb2dsZVBsYXlQdXJjaGFzZSJlCh5WYWxpZGF0ZUdvb2dsZVB1" + "cmNoYXNlUmVzcG9uc2USQwoNcHVyY2hhc2VfaW5mbxgBIAEoCzIsLkh5ZHJh" + "LkFwaS5Hb29nbGVQbGF5Lkdvb2dsZVBsYXlQdXJjaGFzZUluZm8iiAEKEkdv" + "b2dsZVBsYXlQdXJjaGFzZRISCgpwcm9kdWN0X2lkGAEgASgJEhYKDnB1cmNo" + "YXNlX3Rva2VuGAIgASgJEhUKDWN1cnJlbmN5X2NvZGUYAyABKAkSFQoNbnVt" + "ZXJpY19wcmljZRgEIAEoAhIYChBzYWxlX2luc3RhbmNlX2lkGAUgASgJIm8K" + "Fkdvb2dsZVBsYXlQdXJjaGFzZUluZm8SQwoNcHVyY2hhc2VfdHlwZRgBIAEo" + "DjIsLkh5ZHJhLkFwaS5Hb29nbGVQbGF5Lkdvb2dsZVBsYXlQdXJjaGFzZVR5" + "cGUSEAoIb3JkZXJfaWQYAiABKAkq2wEKFkdvb2dsZVBsYXlQdXJjaGFzZVR5" + "cGUSIgoeR09PR0xFX1BMQVlfUFVSQ0hBU0VfVFlQRV9URVNUEAASIwofR09P" + "R0xFX1BMQVlfUFVSQ0hBU0VfVFlQRV9QUk9NTxABEiYKIkdPT0dMRV9QTEFZ" + "X1BVUkNIQVNFX1RZUEVfUkVXQVJERUQQAhIoCiRHT09HTEVfUExBWV9QVVJD" + "SEFTRV9UWVBFX1JFQUxfTU9ORVkQZBImCiFHT09HTEVfUExBWV9QVVJDSEFT" + "RV9UWVBFX1VOS05PV04QyAFiBnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { UserContextReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[1] { typeof(GooglePlayPurchaseType) }, null, new GeneratedClrTypeInfo[4]
		{
			new GeneratedClrTypeInfo(typeof(ValidateGooglePurchaseRequest), ValidateGooglePurchaseRequest.Parser, new string[2] { "UserContext", "Purchase" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ValidateGooglePurchaseResponse), ValidateGooglePurchaseResponse.Parser, new string[1] { "PurchaseInfo" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GooglePlayPurchase), GooglePlayPurchase.Parser, new string[5] { "ProductId", "PurchaseToken", "CurrencyCode", "NumericPrice", "SaleInstanceId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GooglePlayPurchaseInfo), GooglePlayPurchaseInfo.Parser, new string[2] { "PurchaseType", "OrderId" }, null, null, null, null)
		}));
	}
}
