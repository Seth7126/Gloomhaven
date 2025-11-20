using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core;

public interface ITranslationRegistry
{
	void RegisterPluginSpecificTranslations(Assembly assembly, StreamTranslationPackage package);

	void RegisterPluginSpecificTranslations(Assembly assembly, KeyValuePairTranslationPackage package);

	void EnablePluginTranslationFallback(Assembly assembly);
}
