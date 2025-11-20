using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class TranslationJob
{
	public bool IsTranslatable { get; }

	public bool SaveResultGlobally { get; private set; }

	public bool AllowFallback { get; private set; }

	public TranslationEndpointManager Endpoint { get; internal set; }

	public HashSet<ParserTranslationContext> Contexts { get; private set; }

	public List<KeyAnd<object>> Components { get; private set; }

	public HashSet<KeyAnd<InternalTranslationResult>> TranslationResults { get; private set; }

	public UntranslatedText Key { get; private set; }

	public UntranslatedTextInfo UntranslatedTextInfo { get; set; }

	public string TranslatedText { get; set; }

	public string ErrorMessage { get; set; }

	public TranslationJobState State { get; set; }

	public TranslationType TranslationType { get; set; }

	public bool ShouldPersistTranslation
	{
		get
		{
			bool flag = (TranslationType & TranslationType.Full) == TranslationType.Full;
			if (!flag)
			{
				return Contexts.Any((ParserTranslationContext x) => x.Result.PersistTokenResult);
			}
			return flag;
		}
	}

	public TranslationJob(TranslationEndpointManager endpoint, UntranslatedText key, bool saveResult, bool isTranslatable)
	{
		Endpoint = endpoint;
		Key = key;
		SaveResultGlobally = saveResult;
		IsTranslatable = isTranslatable;
		Components = new List<KeyAnd<object>>();
		Contexts = new HashSet<ParserTranslationContext>();
		TranslationResults = new HashSet<KeyAnd<InternalTranslationResult>>();
	}

	public void Associate(UntranslatedText key, object ui, InternalTranslationResult translationResult, ParserTranslationContext context, UntranslatedTextInfo untranslatedTextInfo, bool saveResultGlobally, bool allowFallback)
	{
		AllowFallback = allowFallback || AllowFallback;
		if (UntranslatedTextInfo == null && untranslatedTextInfo != null)
		{
			UntranslatedTextInfo = untranslatedTextInfo;
		}
		SaveResultGlobally |= saveResultGlobally;
		if (context != null)
		{
			Contexts.Add(context);
			context.Jobs.Add(this);
			TranslationType |= TranslationType.Token;
			return;
		}
		if (ui != null && !ui.IsSpammingComponent())
		{
			Components.Add(new KeyAnd<object>(key, ui));
		}
		if (translationResult != null)
		{
			TranslationResults.Add(new KeyAnd<InternalTranslationResult>(key, translationResult));
		}
		TranslationType |= TranslationType.Full;
	}
}
