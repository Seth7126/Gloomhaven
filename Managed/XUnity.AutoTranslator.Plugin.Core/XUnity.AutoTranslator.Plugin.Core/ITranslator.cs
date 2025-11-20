using System;

namespace XUnity.AutoTranslator.Plugin.Core;

public interface ITranslator
{
	void TranslateAsync(string untranslatedText, Action<TranslationResult> onCompleted);

	void TranslateAsync(string untranslatedText, int scope, Action<TranslationResult> onCompleted);

	bool TryTranslate(string untranslatedText, out string translatedText);

	bool TryTranslate(string untranslatedText, int scope, out string translatedText);

	void IgnoreTextComponent(object textComponent);

	void UnignoreTextComponent(object textComponent);

	void RegisterOnTranslatingCallback(Action<ComponentTranslationContext> context);

	void UnregisterOnTranslatingCallback(Action<ComponentTranslationContext> context);
}
