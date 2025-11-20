using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SM.Utils;

[Serializable]
public class EditorProperties : ISerializable
{
	private static EditorProperties s_Instance;

	public static EditorProperties Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new EditorProperties();
				s_Instance.Load();
			}
			return s_Instance;
		}
	}

	public bool DisplayBasicTilesAtRuntimeEditor { get; private set; }

	public bool DisableRoomHiding { get; private set; }

	public bool MMBPanning { get; private set; }

	public bool AutotestPlayback { get; private set; }

	public bool AnalyticsDebug { get; private set; }

	public bool SteamInEditor { get; private set; }

	public bool GoGGalaxyInEditor { get; private set; }

	public bool EOSInEditor { get; private set; }

	public bool LogCameraDetails { get; private set; }

	public bool EnableFogOfWar { get; private set; }

	public string CustomEditorDataPath { get; private set; }

	public bool LocalMultiplayer { get; private set; }

	public bool WriteSaveAssemblyInfo { get; private set; }

	public bool CompileRulebaseOnPlay { get; private set; }

	public bool MPEndOfTurnCompare { get; set; }

	public bool LoadMostRecentSave { get; private set; }

	public bool EnableJoTLDLC { get; private set; }

	public bool EnableSoloScenariosDLC { get; private set; }

	public bool EnableUseAssetBundlesInEditor { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("DisplayBasicTilesAtRuntimeEditor", DisplayBasicTilesAtRuntimeEditor);
		info.AddValue("DisableRoomHiding", DisableRoomHiding);
		info.AddValue("MMBPanning", MMBPanning);
		info.AddValue("AutotestPlayback", AutotestPlayback);
		info.AddValue("LogCameraDetails", LogCameraDetails);
		info.AddValue("EnableFogOfWar", EnableFogOfWar);
		info.AddValue("CustomEditorDataPath", CustomEditorDataPath);
		info.AddValue("LocalMultiplayer", LocalMultiplayer);
		info.AddValue("WriteSaveAssemblyInfo", WriteSaveAssemblyInfo);
		info.AddValue("CompileRulebaseOnPlay", CompileRulebaseOnPlay);
		info.AddValue("MPEndOfTurnCompare", MPEndOfTurnCompare);
		info.AddValue("LoadMostRecentSave", LoadMostRecentSave);
		info.AddValue("EnableJoTLDLC", EnableJoTLDLC);
		info.AddValue("EnableSoloScenariosDLC", EnableSoloScenariosDLC);
	}

	private EditorProperties(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "DisplayBasicTilesAtRuntimeEditor":
					DisplayBasicTilesAtRuntimeEditor = info.GetBoolean("DisplayBasicTilesAtRuntimeEditor");
					break;
				case "DisableRoomHiding":
					DisableRoomHiding = info.GetBoolean("DisableRoomHiding");
					break;
				case "MMBPanning":
					MMBPanning = info.GetBoolean("MMBPanning");
					break;
				case "AutotestPlayback":
					AutotestPlayback = info.GetBoolean("AutotestPlayback");
					break;
				case "LogCameraDetails":
					LogCameraDetails = info.GetBoolean("LogCameraDetails");
					break;
				case "EnableFogOfWar":
					EnableFogOfWar = info.GetBoolean("EnableFogOfWar");
					break;
				case "CustomEditorDataPath":
					CustomEditorDataPath = info.GetString("CustomEditorDataPath");
					break;
				case "LocalMultiplayer":
					LocalMultiplayer = info.GetBoolean("LocalMultiplayer");
					break;
				case "WriteSaveAssemblyInfo":
					WriteSaveAssemblyInfo = info.GetBoolean("WriteSaveAssemblyInfo");
					break;
				case "CompileRulebaseOnPlay":
					CompileRulebaseOnPlay = info.GetBoolean("CompileRulebaseOnPlay");
					break;
				case "MPEndOfTurnCompare":
					MPEndOfTurnCompare = info.GetBoolean("MPEndOfTurnCompare");
					break;
				case "LoadMostRecentSave":
					LoadMostRecentSave = info.GetBoolean("LoadMostRecentSave");
					break;
				case "EnableJoTLDLC":
					EnableJoTLDLC = info.GetBoolean("EnableJoTLDLC");
					break;
				case "EnableSoloScenariosDLC":
					EnableSoloScenariosDLC = info.GetBoolean("EnableSoloScenariosDLC");
					break;
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize EditorProperties entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public EditorProperties()
	{
		DisplayBasicTilesAtRuntimeEditor = false;
		DisableRoomHiding = false;
		MMBPanning = false;
		AutotestPlayback = false;
		AnalyticsDebug = IsDefineSet("DEBUG_ANALYTICS");
		SteamInEditor = IsDefineSet("STEAM") && IsDefineSet("FACEPUNCHAGENT");
		GoGGalaxyInEditor = IsDefineSet("GOGGALAXY");
		EOSInEditor = IsDefineSet("EPICSTORE");
		LogCameraDetails = false;
		EnableFogOfWar = false;
		CustomEditorDataPath = string.Empty;
		LocalMultiplayer = false;
		WriteSaveAssemblyInfo = false;
		CompileRulebaseOnPlay = false;
		MPEndOfTurnCompare = false;
		LoadMostRecentSave = false;
		EnableJoTLDLC = true;
		EnableSoloScenariosDLC = true;
		EnableUseAssetBundlesInEditor = IsDefineSet("DEBUG_ASSET_BUNDLES");
	}

	public void Save()
	{
	}

	public void Load()
	{
	}

	private void SetProperties(EditorProperties props)
	{
		DisplayBasicTilesAtRuntimeEditor = props.DisplayBasicTilesAtRuntimeEditor;
		DisableRoomHiding = props.DisableRoomHiding;
		MMBPanning = props.MMBPanning;
		AutotestPlayback = props.AutotestPlayback;
		AnalyticsDebug = IsDefineSet("DEBUG_ANALYTICS");
		SteamInEditor = IsDefineSet("STEAM") && IsDefineSet("FACEPUNCHAGENT");
		GoGGalaxyInEditor = IsDefineSet("GOGGALAXY");
		LogCameraDetails = props.LogCameraDetails;
		EnableFogOfWar = props.EnableFogOfWar;
		CustomEditorDataPath = props.CustomEditorDataPath;
		LocalMultiplayer = props.LocalMultiplayer;
		WriteSaveAssemblyInfo = props.WriteSaveAssemblyInfo;
		CompileRulebaseOnPlay = props.CompileRulebaseOnPlay;
		MPEndOfTurnCompare = props.MPEndOfTurnCompare;
		LoadMostRecentSave = props.LoadMostRecentSave;
		EnableJoTLDLC = props.EnableJoTLDLC;
		EnableSoloScenariosDLC = props.EnableSoloScenariosDLC;
		EnableUseAssetBundlesInEditor = IsDefineSet("DEBUG_ASSET_BUNDLES");
	}

	public void SetProperties(bool displayBasicTilesAtRuntime, bool disableRoomHiding, bool mmbPanning, bool autotestPlayback, bool logCameraDetails, bool enableFogOfWar, string customEditorDataPath, bool localMultiplayer, bool writeSaveAssemblyInfo, bool compileRulebaseOnPlay, bool mpEndOfTurnCompare, bool loadMostRecentSave, bool enableJoTLDLC, bool enableSoloScenariosDLC, bool enableAssetBundlesInEditor)
	{
		DisplayBasicTilesAtRuntimeEditor = displayBasicTilesAtRuntime;
		DisableRoomHiding = disableRoomHiding;
		MMBPanning = mmbPanning;
		AutotestPlayback = autotestPlayback;
		AnalyticsDebug = IsDefineSet("DEBUG_ANALYTICS");
		SteamInEditor = IsDefineSet("STEAM") && IsDefineSet("FACEPUNCHAGENT");
		GoGGalaxyInEditor = IsDefineSet("GOGGALAXY");
		EOSInEditor = IsDefineSet("EPICSTORE");
		LogCameraDetails = logCameraDetails;
		EnableFogOfWar = enableFogOfWar;
		CustomEditorDataPath = customEditorDataPath;
		LocalMultiplayer = localMultiplayer;
		WriteSaveAssemblyInfo = writeSaveAssemblyInfo;
		CompileRulebaseOnPlay = compileRulebaseOnPlay;
		MPEndOfTurnCompare = mpEndOfTurnCompare;
		LoadMostRecentSave = loadMostRecentSave;
		EnableJoTLDLC = enableJoTLDLC;
		EnableSoloScenariosDLC = enableSoloScenariosDLC;
		EnableUseAssetBundlesInEditor = IsDefineSet("DEBUG_ASSET_BUNDLES");
	}

	private bool IsDefineSet(string define)
	{
		return false;
	}
}
