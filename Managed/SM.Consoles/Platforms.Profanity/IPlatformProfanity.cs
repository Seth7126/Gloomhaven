using System;

namespace Platforms.Profanity;

public interface IPlatformProfanity
{
	void CheckBadWordsAsync(string text, Action<OperationResult, bool> resultCallback);

	void MaskBadWordsAsync(string text, Action<OperationResult, string> resultCallback);
}
