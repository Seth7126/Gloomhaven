using System.IO;
using System.Linq;
using Platforms;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using Steamworks;
using UnityEngine;

public class PlatformDLC : MonoBehaviour, IPlatformDLC
{
	private const uint c_JOTLSteamAppId = 1809490u;

	private const uint c_SoloSteamAppId = 1958560u;

	private const uint c_JOTLAltSkinsSteamAppId = 2584170u;

	public bool CanPlayPartyData(PartyAdventureData partyData)
	{
		return DLCRegistry.GetDLCListForFlag((partyData?.AdventureMapState?.DLCEnabled).GetValueOrDefault()).All((DLCRegistry.EDLCKey d) => d == DLCRegistry.EDLCKey.None || PlatformLayer.DLC.CanPlayDLC(d));
	}

	public bool CanPlayCustomLevel(CCustomLevelData levelData)
	{
		return DLCRegistry.GetDLCListForFlag(levelData.DLCUsed).All((DLCRegistry.EDLCKey d) => d == DLCRegistry.EDLCKey.None || PlatformLayer.DLC.CanPlayDLC(d));
	}

	public bool CanPlayDLC(DLCRegistry.EDLCKey dlcFlag)
	{
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != DLCRegistry.EDLCKey.None && dlcFlag.HasFlag(eDLCKey))
			{
				if (!PlatformLayer.DLC.UserInstalledDLC(eDLCKey))
				{
					return false;
				}
				if (!File.Exists(SceneController.Instance.YML.DLCGlobalRulesetZip(eDLCKey)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public DLCRegistry.EDLCKey GetInvalidDLCs(DLCRegistry.EDLCKey comparingDlcs)
	{
		DLCRegistry.EDLCKey eDLCKey = DLCRegistry.EDLCKey.None;
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey eDLCKey2 in dLCKeys)
		{
			if (comparingDlcs.HasFlag(eDLCKey2) && !PlatformLayer.DLC.UserInstalledDLC(eDLCKey2))
			{
				eDLCKey |= eDLCKey2;
			}
		}
		return eDLCKey;
	}

	public DLCRegistry.EDLCKey OwnedDLCAsFlag()
	{
		DLCRegistry.EDLCKey eDLCKey = DLCRegistry.EDLCKey.None;
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey eDLCKey2 in dLCKeys)
		{
			if (eDLCKey2 != DLCRegistry.EDLCKey.None && UserInstalledDLC(eDLCKey2))
			{
				eDLCKey |= eDLCKey2;
			}
		}
		return eDLCKey;
	}

	public T1 GetDLCGUIAssetFromBundle<T1>(DLCRegistry.EDLCKey dlc, string fileName, string folderPath = "", string fileType = "prefab", bool suppressError = false, bool ignoreDlcOwnership = false, bool alwaysLoaded = false) where T1 : Object
	{
		string text = Path.Combine("Assets", "_AssetBundles", GloomUtility.GetEnumCategory(dlc), folderPath, fileName + "." + fileType).Replace("\\", "/");
		if (ignoreDlcOwnership || CanPlayDLC(dlc))
		{
			string assetBundleName = string.Format("{0}_{1}_misc_gui", "dlc", dlc.ToString().ToLower());
			AssetBundleManager instance = AssetBundleManager.Instance;
			string empty = string.Empty;
			string explicitPath = text;
			return instance.LoadAssetFromBundle<T1>(assetBundleName, fileName, empty, fileType, explicitPath, suppressError, alwaysLoaded);
		}
		Debug.LogErrorFormat("[PLATFORM LAYER] - Trying to fetch asset {0} from bundle {1} when the user does not own it", fileName, dlc.ToString());
		return null;
	}

	public void Initialize(IPlatform platform)
	{
	}

	public bool UserInstalledDLC(DLCRegistry.EDLCKey dlc)
	{
		if (SteamClient.IsValid)
		{
			return dlc switch
			{
				DLCRegistry.EDLCKey.None => false, 
				DLCRegistry.EDLCKey.DLC1 => SteamApps.IsDlcInstalled(1809490u), 
				DLCRegistry.EDLCKey.DLC2 => SteamApps.IsDlcInstalled(1958560u), 
				DLCRegistry.EDLCKey.DLC3 => SteamApps.IsDlcInstalled(2584170u), 
				_ => false, 
			};
		}
		return false;
	}

	public bool UserOwnsDLC(DLCRegistry.EDLCKey dlc)
	{
		return UserInstalledDLC(dlc);
	}

	public void OpenPlatformStoreDLCOverlay(DLCRegistry.EDLCKey dlc)
	{
		if (SteamClient.IsValid)
		{
			switch (dlc)
			{
			case DLCRegistry.EDLCKey.DLC1:
				SteamFriends.OpenStoreOverlay(1809490u);
				break;
			case DLCRegistry.EDLCKey.DLC2:
				SteamFriends.OpenStoreOverlay(1958560u);
				break;
			default:
				SteamFriends.OpenStoreOverlay(SteamClient.AppId);
				break;
			}
		}
	}
}
