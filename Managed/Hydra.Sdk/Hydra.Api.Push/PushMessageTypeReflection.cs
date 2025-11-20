using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public static class PushMessageTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushMessageTypeReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChpQdXNoL1B1c2hNZXNzYWdlVHlwZS5wcm90bxIOSHlkcmEuQXBpLlB1c2gq" + "9QIKD1B1c2hNZXNzYWdlVHlwZRIfChtQVVNIX01FU1NBR0VfVFlQRV9VTkRF" + "RklORUQQABIqCiZQVVNIX01FU1NBR0VfVFlQRV9QUkVTRU5DRV9VU0VSX1VQ" + "REFURRABEisKJ1BVU0hfTUVTU0FHRV9UWVBFX1BSRVNFTkNFX1BBUlRZX1VQ" + "REFURRACEi0KKVBVU0hfTUVTU0FHRV9UWVBFX1BSRVNFTkNFX1NFU1NJT05f" + "VVBEQVRFEAMSNgoyUFVTSF9NRVNTQUdFX1RZUEVfRUNPTk9NWV9VU0VSX1RS" + "QU5TQUNUSU9OU19VUERBVEUQBBIfChtQVVNIX01FU1NBR0VfVFlQRV9TSUdO" + "QUxJTkcQBRIrCidQVVNIX01FU1NBR0VfVFlQRV9NRVNTQUdJTkdfVVNFUl9V" + "UERBVEUQBhIzCi9QVVNIX01FU1NBR0VfVFlQRV9DSEFMTEVOR0VTX0lOQ1JF" + "TUVOVEFMX1VQREFURRAHYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(PushMessageType) }, null, null));
	}
}
