using System;
using System.Collections.Generic;
using System.IO;
using ScenarioRuleLibrary;
using UnityEngine;

[CreateAssetMenu(menuName = "Asset Bundles/GH Asset Bundle Load Settings")]
public class BundleLoadSettings : ScriptableObject
{
	[Serializable]
	public class BundleLoadConfig : IEquatable<BundleLoadConfig>
	{
		[Serializable]
		public enum EBundleConfigType
		{
			None,
			Misc,
			Hero,
			NPC
		}

		[ReadOnlyField]
		public string AssetBundleName;

		[ReadOnlyField]
		public EBundleConfigType BundleConfigType;

		public DLCRegistry.EDLCKey BundleDLC;

		public bool AlwaysLoaded;

		public List<ECharacter> AssociatedCharacters;

		public List<CClass.ENPCModel> AssociatedNPCModels;

		public string AssetsBundleLoadPath
		{
			get
			{
				string path = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
				if (BundleDLC != DLCRegistry.EDLCKey.None)
				{
					path = RootSaveData.DLCPackageFolder(BundleDLC);
				}
				return BundleConfigType switch
				{
					EBundleConfigType.Misc => Path.Combine(path, "MiscBundles", AssetBundleName), 
					EBundleConfigType.Hero => Path.Combine(path, "HeroBundles", AssetBundleName), 
					EBundleConfigType.NPC => Path.Combine(path, "NPCBundles", AssetBundleName), 
					_ => null, 
				};
			}
		}

		public BundleLoadConfig(string bundleName)
		{
			AssetBundleName = bundleName;
		}

		public bool Equals(BundleLoadConfig other)
		{
			return AssetBundleName == other.AssetBundleName;
		}

		public override int GetHashCode()
		{
			return AssetBundleName.GetHashCode();
		}
	}

	public const string cMiscBundlePrefix = "misc";

	public const string cMiscBundleDirectoryName = "MiscBundles";

	public const string cHeroBundlePrefix = "hero";

	public const string cHeroBundleDirectoryName = "HeroBundles";

	public const string cNPCBundlePrefix = "npc";

	public const string cNPCBundleDirectoryName = "NPCBundles";

	public const string cDLCBundlePrefix = "dlc";

	public const string cDLCBundleDirectoryName = "DLCBundles";

	public List<BundleLoadConfig> BundleConfigs;

	public List<BundleLoadConfig> DLCBundleConfigs;

	public List<BundleLoadConfig> GetBundlesNeededForRequirements(List<ECharacter> charactersNeeded, List<CClass.ENPCModel> npcModelsNeeded)
	{
		List<BundleLoadConfig> list = new List<BundleLoadConfig>();
		if (charactersNeeded != null)
		{
			foreach (ECharacter item in charactersNeeded)
			{
				foreach (BundleLoadConfig bundleConfig in BundleConfigs)
				{
					if (!list.Contains(bundleConfig) && bundleConfig.AssociatedCharacters.Contains(item))
					{
						list.Add(bundleConfig);
					}
				}
				foreach (BundleLoadConfig dLCBundleConfig in DLCBundleConfigs)
				{
					if (!list.Contains(dLCBundleConfig) && dLCBundleConfig.AssociatedCharacters.Contains(item))
					{
						list.Add(dLCBundleConfig);
					}
				}
			}
		}
		if (npcModelsNeeded != null)
		{
			foreach (CClass.ENPCModel item2 in npcModelsNeeded)
			{
				foreach (BundleLoadConfig bundleConfig2 in BundleConfigs)
				{
					if (!list.Contains(bundleConfig2) && bundleConfig2.AssociatedNPCModels.Contains(item2))
					{
						list.Add(bundleConfig2);
					}
				}
				foreach (BundleLoadConfig dLCBundleConfig2 in DLCBundleConfigs)
				{
					if (!list.Contains(dLCBundleConfig2) && dLCBundleConfig2.AssociatedNPCModels.Contains(item2))
					{
						list.Add(dLCBundleConfig2);
					}
				}
			}
		}
		return list;
	}

	public List<BundleLoadConfig> GetBundlesNeededForUnload(List<ECharacter> charactersNeeded)
	{
		List<BundleLoadConfig> list = new List<BundleLoadConfig>();
		if (charactersNeeded != null)
		{
			foreach (ECharacter item in charactersNeeded)
			{
				foreach (BundleLoadConfig bundleConfig in BundleConfigs)
				{
					if (!list.Contains(bundleConfig) && bundleConfig.AssociatedCharacters.Contains(item))
					{
						list.Add(bundleConfig);
					}
				}
				foreach (BundleLoadConfig dLCBundleConfig in DLCBundleConfigs)
				{
					if (!list.Contains(dLCBundleConfig) && dLCBundleConfig.AssociatedCharacters.Contains(item))
					{
						list.Add(dLCBundleConfig);
					}
				}
			}
		}
		return list;
	}
}
