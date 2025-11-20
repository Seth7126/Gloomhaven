namespace XUnity.AutoTranslator.Plugin.Core;

public class TranslationResult
{
	public bool Succeeded => ErrorMessage == null;

	public string TranslatedText { get; }

	public string ErrorMessage { get; }

	internal TranslationResult(string translatedText, string errorMessage)
	{
		TranslatedText = translatedText;
		ErrorMessage = errorMessage;
	}
}
