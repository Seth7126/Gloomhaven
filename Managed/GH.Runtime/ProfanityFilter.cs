using System;
using System.Collections.Generic;
using Platforms.Profanity;
using Platforms.Utils;
using UnityEngine;

public class ProfanityFilter : MonoBehaviour, IPlatformProfanity
{
	private IPlatformProfanity _profanityFilter;

	public void Initialize()
	{
		List<IPlatformProfanity> list = new List<IPlatformProfanity>();
		list.Add(new ProfanityFilterCache(PlatformLayer.Platform.PlatformProfanity));
		_profanityFilter = new ProfanityFallbackFiltersList(list);
	}

	public void CheckBadWordsAsync(string text, Action<OperationResult, bool> resultCallback)
	{
		_profanityFilter.CheckBadWordsAsync(text, resultCallback);
	}

	public void MaskBadWordsAsync(string text, Action<OperationResult, string> resultCallback)
	{
		_profanityFilter.MaskBadWordsAsync(text, resultCallback);
	}
}
