using System.IO;
using ExIni;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class DefaultPluginEnvironment : IPluginEnvironment
{
	private IniFile _file;

	private string _configPath;

	private string _dataFolder;

	public IniFile Preferences => _file ?? (_file = ReloadConfig());

	public string TranslationPath => _dataFolder;

	public string ConfigPath => _dataFolder;

	public bool AllowDefaultInitializeHarmonyDetourBridge { get; }

	public DefaultPluginEnvironment(bool allowDefaultInitializeHarmonyDetourBridge)
	{
		_dataFolder = Path.Combine(Paths.GameRoot, "AutoTranslator");
		_configPath = Path.Combine(_dataFolder, "Config.ini");
		AllowDefaultInitializeHarmonyDetourBridge = allowDefaultInitializeHarmonyDetourBridge;
	}

	public void SaveConfig()
	{
		_file.Save(_configPath);
	}

	public IniFile ReloadConfig()
	{
		if (!File.Exists(_configPath))
		{
			return _file ?? new IniFile();
		}
		IniFile iniFile = IniFile.FromFile(_configPath);
		if (_file == null)
		{
			return _file = iniFile;
		}
		_file.Merge(iniFile);
		return _file;
	}
}
