using System.Threading;

namespace System.Text;

/// <summary>Provides a failure-handling mechanism, called a fallback, for an input character that cannot be converted to an encoded output byte sequence. </summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public abstract class EncoderFallback
{
	private static EncoderFallback s_replacementFallback;

	private static EncoderFallback s_exceptionFallback;

	/// <summary>Gets an object that outputs a substitute string in place of an input character that cannot be encoded.</summary>
	/// <returns>A type derived from the <see cref="T:System.Text.EncoderFallback" /> class. The default value is a <see cref="T:System.Text.EncoderReplacementFallback" /> object that replaces unknown input characters with the QUESTION MARK character ("?", U+003F).</returns>
	/// <filterpriority>1</filterpriority>
	public static EncoderFallback ReplacementFallback
	{
		get
		{
			if (s_replacementFallback == null)
			{
				Interlocked.CompareExchange(ref s_replacementFallback, new EncoderReplacementFallback(), null);
			}
			return s_replacementFallback;
		}
	}

	/// <summary>Gets an object that throws an exception when an input character cannot be encoded.</summary>
	/// <returns>A type derived from the <see cref="T:System.Text.EncoderFallback" /> class. The default value is a <see cref="T:System.Text.EncoderExceptionFallback" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public static EncoderFallback ExceptionFallback
	{
		get
		{
			if (s_exceptionFallback == null)
			{
				Interlocked.CompareExchange(ref s_exceptionFallback, new EncoderExceptionFallback(), null);
			}
			return s_exceptionFallback;
		}
	}

	/// <summary>When overridden in a derived class, gets the maximum number of characters the current <see cref="T:System.Text.EncoderFallback" /> object can return.</summary>
	/// <returns>The maximum number of characters the current <see cref="T:System.Text.EncoderFallback" /> object can return.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int MaxCharCount { get; }

	/// <summary>When overridden in a derived class, initializes a new instance of the <see cref="T:System.Text.EncoderFallbackBuffer" /> class. </summary>
	/// <returns>An object that provides a fallback buffer for an encoder.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract EncoderFallbackBuffer CreateFallbackBuffer();

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderFallback" /> class.</summary>
	protected EncoderFallback()
	{
	}
}
