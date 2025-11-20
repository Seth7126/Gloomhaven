using System.Collections;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints;

public interface ITranslateEndpoint
{
	string Id { get; }

	string FriendlyName { get; }

	int MaxConcurrency { get; }

	int MaxTranslationsPerRequest { get; }

	void Initialize(IInitializationContext context);

	IEnumerator Translate(ITranslationContext context);
}
