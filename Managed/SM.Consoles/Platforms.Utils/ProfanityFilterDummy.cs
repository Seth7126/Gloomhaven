using System;
using Platforms.Profanity;

namespace Platforms.Utils;

public class ProfanityFilterDummy : IPlatformProfanity
{
	public OperationResult Result { get; set; }

	public string OverrideFilteredText { get; set; }

	public ProfanityFilterDummy(OperationResult result, string overrideFilteredText = null)
	{
		Result = result;
		OverrideFilteredText = overrideFilteredText;
	}

	public void CheckBadWordsAsync(string text, Action<OperationResult, bool> resultCallback)
	{
		resultCallback?.Invoke(Result, OverrideFilteredText != null);
	}

	public void MaskBadWordsAsync(string text, Action<OperationResult, string> resultCallback)
	{
		resultCallback?.Invoke(Result, OverrideFilteredText ?? text);
	}
}
