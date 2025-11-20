using System;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core;

internal interface IInternalTranslator : ITranslator
{
	void TranslateAsync(TranslationEndpointManager endpoint, string untranslatedText, Action<TranslationResult> onCompleted);
}
