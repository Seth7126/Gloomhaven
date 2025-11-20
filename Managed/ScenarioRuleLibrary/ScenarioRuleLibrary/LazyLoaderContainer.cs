using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SM.Utils;

namespace ScenarioRuleLibrary;

public class LazyLoaderContainer<T>
{
	private string _rulebasePath = LazyLoadingConstants.RulesetPath;

	private LazyLoaderYML<T> _campaignLazyLoader;

	private LazyLoaderYML<T> _guildmasterLazyLoader;

	public LazyLoaderYML<T> GuildmasterLazyLoader => _guildmasterLazyLoader;

	public LazyLoaderYML<T> CampaignLazyLoader => _campaignLazyLoader;

	public LazyLoaderContainer(string YMLtype, Func<StreamReader, string, Dictionary<string, T>, bool> parseMethod)
	{
		string[] commonFiles = GetCommonFiles(YMLtype);
		string[] guilmasterFiles = GetGuilmasterFiles(YMLtype);
		string[] campaignFiles = GetCampaignFiles(YMLtype);
		if (commonFiles.Length == 0 && guilmasterFiles.Length == 0 && campaignFiles.Length == 0)
		{
			LogUtils.LogError("Raw files for lazy loading not found. Try build rulebase");
		}
		_guildmasterLazyLoader = new LazyLoaderYML<T>(guilmasterFiles.Concat(commonFiles).ToArray(), parseMethod);
		_campaignLazyLoader = new LazyLoaderYML<T>(campaignFiles.Concat(commonFiles).ToArray(), parseMethod);
	}

	private string[] GetCommonFiles(string YMLtype)
	{
		string commonPath = GetCommonPath(YMLtype);
		if (Directory.Exists(commonPath))
		{
			return Directory.GetFiles(commonPath, "*.yml", SearchOption.AllDirectories);
		}
		return Array.Empty<string>();
	}

	private string[] GetGuilmasterFiles(string YMLtype)
	{
		string guilmasterPath = GetGuilmasterPath(YMLtype);
		if (Directory.Exists(guilmasterPath))
		{
			return Directory.GetFiles(guilmasterPath, "*.yml", SearchOption.AllDirectories);
		}
		return Array.Empty<string>();
	}

	private string[] GetCampaignFiles(string YMLtype)
	{
		string campaignPath = GetCampaignPath(YMLtype);
		if (Directory.Exists(campaignPath))
		{
			return Directory.GetFiles(campaignPath, "*.yml", SearchOption.AllDirectories);
		}
		return Array.Empty<string>();
	}

	private string GetCampaignPath(string YMLtype)
	{
		return Path.Combine(_rulebasePath, "CampaignRaw", YMLtype);
	}

	private string GetGuilmasterPath(string YMLtype)
	{
		return Path.Combine(_rulebasePath, "GuildmasterRaw", YMLtype);
	}

	private string GetCommonPath(string YMLtype)
	{
		return Path.Combine(_rulebasePath, "CommonRaw", YMLtype);
	}

	public T GetData(string id, ScenarioManager.EDLLMode mode, bool removeSuffix)
	{
		switch (mode)
		{
		case ScenarioManager.EDLLMode.Campaign:
			return _campaignLazyLoader.GetYML(id, removeSuffix);
		case ScenarioManager.EDLLMode.Guildmaster:
			return _guildmasterLazyLoader.GetYML(id, removeSuffix);
		default:
			LogUtils.LogError($"[MonsterYML] Errror. Unhandled EDLLMode: {mode}");
			return default(T);
		}
	}
}
