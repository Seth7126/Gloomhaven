using System.Security.Cryptography.Asn1;

namespace System.Security.Cryptography.Pkcs.Asn1;

internal struct AlgorithmIdentifierAsn
{
	internal static readonly ReadOnlyMemory<byte> ExplicitDerNull = new byte[2] { 5, 0 };

	[ObjectIdentifier(PopulateFriendlyName = true)]
	public Oid Algorithm;

	[OptionalValue]
	[AnyValue]
	public ReadOnlyMemory<byte>? Parameters;

	internal bool Equals(ref AlgorithmIdentifierAsn other)
	{
		if (Algorithm.Value != other.Algorithm.Value)
		{
			return false;
		}
		bool flag = RepresentsNull(Parameters);
		bool flag2 = RepresentsNull(other.Parameters);
		if (flag != flag2)
		{
			return false;
		}
		if (flag)
		{
			return true;
		}
		ReadOnlyMemory<byte> value = Parameters.Value;
		ReadOnlySpan<byte> span = value.Span;
		value = other.Parameters.Value;
		return span.SequenceEqual(value.Span);
	}

	private static bool RepresentsNull(ReadOnlyMemory<byte>? parameters)
	{
		if (!parameters.HasValue)
		{
			return true;
		}
		ReadOnlySpan<byte> span = parameters.Value.Span;
		if (span.Length != 2)
		{
			return false;
		}
		if (span[0] != 5)
		{
			return false;
		}
		return span[1] == 0;
	}
}
