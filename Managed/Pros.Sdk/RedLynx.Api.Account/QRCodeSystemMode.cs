using Google.Protobuf.Reflection;

namespace RedLynx.Api.Account;

public enum QRCodeSystemMode
{
	[OriginalName("QR_CODE_SYSTEM_MODE_NORMAL")]
	Normal,
	[OriginalName("QR_CODE_SYSTEM_MODE_TURN_OFF")]
	TurnOff
}
