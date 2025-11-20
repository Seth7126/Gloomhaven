using System;
using Platforms;
using ScenarioRuleLibrary.CustomLevels;

public interface IPlatformModding
{
	bool ModdingSupported { get; }

	void Initialize(IPlatform platform);

	void RefreshMods();

	void UploadMod(string modName);

	void RefreshLevels();

	void UploadLevel(CCustomLevelData levelToUpload, IProgress<float> progress, Action<string> onFail = null, Action onSuccessful = null);
}
