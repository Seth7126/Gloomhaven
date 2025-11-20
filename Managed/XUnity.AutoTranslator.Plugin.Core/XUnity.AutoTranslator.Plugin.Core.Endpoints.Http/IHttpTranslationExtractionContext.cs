namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;

public interface IHttpTranslationExtractionContext : IHttpResponseInspectionContext, IHttpTranslationContext, ITranslationContextBase
{
	void Complete(string translatedText);

	void Complete(string[] translatedTexts);
}
