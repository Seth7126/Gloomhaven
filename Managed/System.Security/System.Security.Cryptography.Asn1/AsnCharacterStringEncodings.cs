using System.Text;

namespace System.Security.Cryptography.Asn1;

internal static class AsnCharacterStringEncodings
{
	private static readonly Encoding s_utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private static readonly Encoding s_bmpEncoding = new BMPEncoding();

	private static readonly Encoding s_ia5Encoding = new IA5Encoding();

	private static readonly Encoding s_visibleStringEncoding = new VisibleStringEncoding();

	private static readonly Encoding s_printableStringEncoding = new PrintableStringEncoding();

	internal static Encoding GetEncoding(UniversalTagNumber encodingType)
	{
		return encodingType switch
		{
			UniversalTagNumber.UTF8String => s_utf8Encoding, 
			UniversalTagNumber.PrintableString => s_printableStringEncoding, 
			UniversalTagNumber.IA5String => s_ia5Encoding, 
			UniversalTagNumber.VisibleString => s_visibleStringEncoding, 
			UniversalTagNumber.BMPString => s_bmpEncoding, 
			_ => throw new ArgumentOutOfRangeException("encodingType", encodingType, null), 
		};
	}
}
