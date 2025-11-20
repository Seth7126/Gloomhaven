namespace System.Security.Cryptography.Asn1;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class DefaultValueAttribute : AsnEncodingRuleAttribute
{
	internal byte[] EncodedBytes { get; }

	public ReadOnlyMemory<byte> EncodedValue => EncodedBytes;

	public DefaultValueAttribute(params byte[] encodedValue)
	{
		EncodedBytes = encodedValue;
	}
}
