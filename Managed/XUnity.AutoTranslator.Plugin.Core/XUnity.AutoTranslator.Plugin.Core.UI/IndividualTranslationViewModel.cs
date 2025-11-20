using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class IndividualTranslationViewModel
{
	private string[] _notTranslated = new string[1] { "Not translated yet." };

	private string[] _requestingTranslation = new string[1] { "Requesting translation..." };

	private List<Translation> _translations;

	private TranslatorViewModel _translator;

	private bool _hasStartedTranslation;

	private bool _isTranslated;

	public IEnumerable<string> Translations
	{
		get
		{
			if (_isTranslated)
			{
				return _translations.Select((Translation x) => x.TranslatedText);
			}
			if (_hasStartedTranslation)
			{
				return _requestingTranslation;
			}
			return _notTranslated;
		}
	}

	public IndividualTranslationViewModel(TranslatorViewModel translator, List<Translation> translations)
	{
		_translator = translator;
		_translations = translations;
	}

	public void StartTranslations()
	{
		if (!_translator.IsEnabled || _hasStartedTranslation)
		{
			return;
		}
		_hasStartedTranslation = true;
		foreach (Translation translation in _translations)
		{
			translation.PerformTranslation(_translator.Endpoint);
		}
	}

	public void CheckCompleted()
	{
		if (_translator.IsEnabled && !_isTranslated && _translations.All((Translation x) => x.TranslatedText != null))
		{
			_isTranslated = true;
		}
	}

	public void CopyToClipboard()
	{
		if (_isTranslated)
		{
			ClipboardHelper.CopyToClipboard(_translations.Select((Translation x) => x.TranslatedText), 32767);
		}
	}

	public bool CanCopyToClipboard()
	{
		return _isTranslated;
	}
}
