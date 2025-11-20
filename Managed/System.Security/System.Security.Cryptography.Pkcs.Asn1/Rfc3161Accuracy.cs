using System.Security.Cryptography.Asn1;

namespace System.Security.Cryptography.Pkcs.Asn1;

internal struct Rfc3161Accuracy
{
	[OptionalValue]
	internal int? Seconds;

	[OptionalValue]
	[ExpectedTag(0)]
	internal int? Millis;

	[ExpectedTag(1)]
	[OptionalValue]
	internal int? Micros;

	internal long TotalMicros => 1000000L * (long)Seconds.GetValueOrDefault() + 1000L * (long)Millis.GetValueOrDefault() + Micros.GetValueOrDefault();

	internal Rfc3161Accuracy(long accuracyInMicroseconds)
	{
		if (accuracyInMicroseconds < 0)
		{
			throw new ArgumentOutOfRangeException("accuracyInMicroseconds");
		}
		long result;
		long result2;
		long num = Math.DivRem(Math.DivRem(accuracyInMicroseconds, 1000L, out result), 1000L, out result2);
		if (num != 0L)
		{
			Seconds = checked((int)num);
		}
		else
		{
			Seconds = null;
		}
		if (result2 != 0L)
		{
			Millis = (int)result2;
		}
		else
		{
			Millis = null;
		}
		if (result != 0L)
		{
			Micros = (int)result;
		}
		else
		{
			Micros = null;
		}
	}
}
