namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www;

public interface IWwwTranslationExtractionContext : IWwwTranslationContext, ITranslationContextBase
{
	string ResponseData { get; }

	void Complete(string translatedText);

	void Complete(string[] translatedTexts);
}
