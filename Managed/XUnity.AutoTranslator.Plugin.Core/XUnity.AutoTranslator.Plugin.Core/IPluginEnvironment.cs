using ExIni;

namespace XUnity.AutoTranslator.Plugin.Core;

public interface IPluginEnvironment
{
	string TranslationPath { get; }

	string ConfigPath { get; }

	IniFile Preferences { get; }

	bool AllowDefaultInitializeHarmonyDetourBridge { get; }

	void SaveConfig();
}
