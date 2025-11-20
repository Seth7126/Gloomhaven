using System;
using System.Linq;

namespace Script.PlatformLayer;

public static class PathExtensions
{
	public static string ParsePath(this string path)
	{
		return path;
	}

	private static string GameCoreParse(string path)
	{
		string value = global::PlatformLayer.Platform.PlatformData.GetPersistantDataPath() + "GloomSaves";
		if (path.Contains(value) && path.Contains("\\Campaign"))
		{
			if (path.EndsWith("\\Campaign", StringComparison.Ordinal))
			{
				return "C_";
			}
			path = path.Substring(path.IndexOf("\\Campaign\\", StringComparison.Ordinal) + "\\Campaign\\".Length);
			path = (path.Contains("\\Checkpoints") ? ParseMapCheckpoints(path) : ((!path.Contains("\\ScenarioCheckpoints")) ? path.Substring(path.IndexOf('\\') + 1) : ParseScenarioCheckpoints(path)));
			path = path.Replace("Campaign_", "C_");
		}
		else if (path.Contains(value) && path.Contains("\\Guildmaster"))
		{
			if (path.EndsWith("\\Guildmaster", StringComparison.Ordinal))
			{
				return "G_";
			}
			path = path.Substring(path.IndexOf("\\Guildmaster\\", StringComparison.Ordinal) + "\\Guildmaster\\".Length);
			path = (path.Contains("\\Checkpoints") ? ParseMapCheckpoints(path) : ((!path.Contains("\\ScenarioCheckpoints")) ? path.Substring(path.IndexOf('\\') + 1) : ParseScenarioCheckpoints(path)));
			path = path.Replace("Guildmaster_", "G_");
		}
		else if (path.Contains(value) && path.Contains("\\SingleScenarios"))
		{
			if (path.EndsWith("\\SingleScenario", StringComparison.Ordinal))
			{
				return "SS_";
			}
			path = path.Substring(path.IndexOf("\\SingleScenarios\\", StringComparison.Ordinal) + "\\SingleScenarios\\".Length);
			path = path.Substring(path.IndexOf('\\') + 1);
			path = path.Replace("SingleScenario_SandboxScenario", "SS_SBS");
		}
		return path;
	}

	private static string ParseMapCheckpoints(string path)
	{
		if (path.EndsWith("\\Checkpoints", StringComparison.Ordinal))
		{
			return "CP\\" + path.Split('\\').First();
		}
		path = path.Substring(path.LastIndexOf('\\') + 1);
		return "CP\\" + path;
	}

	private static string ParseScenarioCheckpoints(string path)
	{
		if (path.EndsWith("\\ScenarioCheckpoints", StringComparison.Ordinal))
		{
			return "SCP\\" + path.Split('\\').First();
		}
		path = path.Substring(path.LastIndexOf('\\') + 1);
		return "SCP\\" + path;
	}
}
