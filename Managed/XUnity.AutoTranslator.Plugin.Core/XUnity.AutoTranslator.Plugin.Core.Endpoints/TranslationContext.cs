using System;
using System.Linq;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints;

internal class TranslationContext : ITranslationContext, ITranslationContextBase
{
	private Action<string[]> _complete;

	private Action<string, Exception> _fail;

	private string[] _untranslatedTexts;

	public string UntranslatedText => UntranslatedTexts[0];

	public string[] UntranslatedTexts => _untranslatedTexts ?? (_untranslatedTexts = UntranslatedTextInfos.Select((UntranslatedTextInfo x) => x.UntranslatedText).ToArray());

	public UntranslatedTextInfo UntranslatedTextInfo => UntranslatedTextInfos[0];

	public UntranslatedTextInfo[] UntranslatedTextInfos { get; }

	public string SourceLanguage { get; }

	public string DestinationLanguage { get; }

	internal bool IsDone { get; private set; }

	public object UserState { get; set; }

	public TranslationContext(UntranslatedTextInfo[] untranslatedTextInfos, string sourceLanguage, string destinationLanguage, Action<string[]> complete, Action<string, Exception> fail)
	{
		UntranslatedTextInfos = untranslatedTextInfos;
		SourceLanguage = sourceLanguage;
		DestinationLanguage = destinationLanguage;
		_complete = complete;
		_fail = fail;
	}

	public void Complete(string translatedText)
	{
		Complete(new string[1] { translatedText });
	}

	public void Complete(string[] translatedTexts)
	{
		if (IsDone)
		{
			return;
		}
		try
		{
			if (translatedTexts.Length == 0)
			{
				_fail("Received empty translation from translator.", null);
				return;
			}
			for (int i = 0; i < translatedTexts.Length; i++)
			{
				if (string.IsNullOrEmpty(translatedTexts[0]))
				{
					_fail("Received empty translation from translator.", null);
					return;
				}
			}
			_complete(translatedTexts);
		}
		finally
		{
			IsDone = true;
		}
	}

	public void Fail(string reason, Exception exception)
	{
		if (IsDone)
		{
			return;
		}
		try
		{
			_fail(reason, exception);
			throw new TranslationContextException(reason, exception);
		}
		finally
		{
			IsDone = true;
		}
	}

	public void Fail(string reason)
	{
		if (IsDone)
		{
			return;
		}
		try
		{
			_fail(reason, null);
			throw new TranslationContextException(reason);
		}
		finally
		{
			IsDone = true;
		}
	}

	internal void FailWithoutThrowing(string reason, Exception exception)
	{
		if (IsDone)
		{
			return;
		}
		try
		{
			_fail(reason, exception);
		}
		finally
		{
			IsDone = true;
		}
	}

	internal void FailIfNotCompleted()
	{
		if (!IsDone)
		{
			FailWithoutThrowing("The translation request was not completed before returning from translator.", null);
		}
	}
}
