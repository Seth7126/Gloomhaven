using System.IO;
using UnityEngine;

namespace ScenarioRuleLibrary;

public class LazyLoadingConstants
{
	public const string RawCampaign = "CampaignRaw";

	public const string RawGuildMaster = "GuildmasterRaw";

	public const string RawCommon = "CommonRaw";

	public static string RulesetPath = Path.Combine(Application.streamingAssetsPath, "Rulebase");
}
