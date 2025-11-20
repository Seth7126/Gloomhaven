using System;
using Platforms.Profanity;

public static class ProfanityFilterExtensions
{
	public static void GetCensoredStringAsync(this string text, Action<string> callback)
	{
		PlatformLayer.ProfanityFilter.MaskBadWordsAsync(text, delegate(OperationResult _, string censoredUsername)
		{
			callback(censoredUsername);
		});
	}
}
