using System;
using System.Collections.Generic;
using Platforms.Profanity;

namespace Platforms.Utils;

public class ProfanityFallbackFiltersList : IPlatformProfanity
{
	private readonly IReadOnlyList<IPlatformProfanity> _filters;

	public ProfanityFallbackFiltersList(IReadOnlyList<IPlatformProfanity> filters)
	{
		_filters = filters;
	}

	public void CheckBadWordsAsync(string text, Action<OperationResult, bool> resultCallback)
	{
		CheckBadWordsAsync(0, text, resultCallback);
	}

	public void MaskBadWordsAsync(string text, Action<OperationResult, string> resultCallback)
	{
		MaskBadWordsAsync(0, text, resultCallback);
	}

	private void CheckBadWordsAsync(int filterIndex, string text, Action<OperationResult, bool> resultCallback)
	{
		if (filterIndex >= _filters.Count)
		{
			resultCallback?.Invoke(OperationResult.UnspecifiedError, arg2: false);
			return;
		}
		_filters[filterIndex].CheckBadWordsAsync(text, delegate(OperationResult opRes, bool foundBadWord)
		{
			int num = filterIndex + 1;
			if (opRes == OperationResult.Success || num == _filters.Count)
			{
				resultCallback(opRes, foundBadWord);
			}
			else
			{
				CheckBadWordsAsync(num, text, resultCallback);
			}
		});
	}

	private void MaskBadWordsAsync(int filterIndex, string text, Action<OperationResult, string> resultCallback)
	{
		if (filterIndex >= _filters.Count)
		{
			resultCallback?.Invoke(OperationResult.UnspecifiedError, text);
			return;
		}
		_filters[filterIndex].MaskBadWordsAsync(text, delegate(OperationResult opRes, string filteredText)
		{
			int num = filterIndex + 1;
			if (opRes == OperationResult.Success || num == _filters.Count)
			{
				resultCallback(opRes, filteredText);
			}
			else
			{
				MaskBadWordsAsync(num, text, resultCallback);
			}
		});
	}
}
