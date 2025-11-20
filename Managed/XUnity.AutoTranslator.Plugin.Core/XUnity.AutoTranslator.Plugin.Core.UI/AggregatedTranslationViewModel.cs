using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class AggregatedTranslationViewModel
{
	private List<Translation> _translations;

	private TranslationAggregatorViewModel _parent;

	private float? _started;

	public List<IndividualTranslatorTranslationViewModel> AggregatedTranslations { get; set; }

	public IEnumerable<string> DefaultTranslations => _translations.Select((Translation x) => x.TranslatedText);

	public IEnumerable<string> OriginalTexts => _translations.Select((Translation x) => x.OriginalText);

	public AggregatedTranslationViewModel(TranslationAggregatorViewModel parent, List<Translation> translations)
	{
		_parent = parent;
		_translations = translations;
		AggregatedTranslations = parent.AvailableTranslators.Select((TranslatorViewModel x) => new IndividualTranslatorTranslationViewModel(x, new IndividualTranslationViewModel(x, translations.Select((Translation y) => new Translation(y.OriginalText, null)).ToList()))).ToList();
	}

	public void CopyDefaultTranslationToClipboard()
	{
		ClipboardHelper.CopyToClipboard(DefaultTranslations, 32767);
	}

	public void CopyOriginalTextToClipboard()
	{
		ClipboardHelper.CopyToClipboard(OriginalTexts, 32767);
	}

	public void Update()
	{
		if (!_parent.IsShown)
		{
			return;
		}
		if (_parent.Manager.OngoingTranslations == 0 && _parent.Manager.UnstartedTranslations == 0)
		{
			if (_started.HasValue)
			{
				if (Time.realtimeSinceStartup - _started.Value > 1f)
				{
					foreach (IndividualTranslatorTranslationViewModel aggregatedTranslation in AggregatedTranslations)
					{
						aggregatedTranslation.Translation.StartTranslations();
					}
				}
			}
			else
			{
				_started = Time.realtimeSinceStartup;
			}
		}
		foreach (IndividualTranslatorTranslationViewModel aggregatedTranslation2 in AggregatedTranslations)
		{
			aggregatedTranslation2.Translation.CheckCompleted();
		}
	}
}
