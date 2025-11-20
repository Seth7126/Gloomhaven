namespace XUnity.AutoTranslator.Plugin.Core.Endpoints;

public interface IInitializationContext
{
	string TranslatorDirectory { get; }

	string SourceLanguage { get; }

	string DestinationLanguage { get; }

	T GetOrCreateSetting<T>(string section, string key, T defaultValue);

	T GetOrCreateSetting<T>(string section, string key);

	void SetSetting<T>(string section, string key, T value);

	void DisableCertificateChecksFor(params string[] hosts);

	void DisableSpamChecks();

	void SetTranslationDelay(float delayInSeconds);
}
