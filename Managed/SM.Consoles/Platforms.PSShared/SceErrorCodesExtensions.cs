namespace Platforms.PSShared;

public static class SceErrorCodesExtensions
{
	public static string ToHexErrorCode(this int intErrorCode)
	{
		string text = intErrorCode.ToString("X8");
		return "0x" + text;
	}
}
