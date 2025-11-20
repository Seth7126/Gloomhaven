using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public static class DLCRegistry
{
	[Serializable]
	[Flags]
	public enum EDLCKey
	{
		[Category("NULL")]
		[Description("NULL")]
		None = 0,
		[Category("DLC_JoTL")]
		[Description("Jaws of the Lion")]
		DLC1 = 1,
		[Category("DLC_Solo")]
		[Description("Solo Scenarios")]
		DLC2 = 2,
		[Category("DLC_4Skins")]
		[Description("Extra Skins")]
		DLC3 = 4
	}

	public static string[] DLCNames = new string[2] { "JOTL", "SOLO" };

	public static EDLCKey[] DLCKeys = (EDLCKey[])Enum.GetValues(typeof(EDLCKey));

	public static EDLCKey AllDLCFlag = EDLCKey.DLC1 | EDLCKey.DLC2;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.EBiome>> m_DLCSpecificBiomes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ESubBiome>> m_DLCSpecificSubBiomes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ETheme>> m_DLCSpecificThemes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ESubTheme>> m_DLCSpecificSubThemes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ETone>> m_DLCSpecificTones;

	private static Dictionary<EDLCKey, List<ECharacter>> m_DLCSpecificCharacters;

	private static Dictionary<EDLCKey, List<CClass.ENPCModel>> m_DLCSpecificNpcModel;

	private static Dictionary<EDLCKey, List<string>> m_DLCSpecificItemGuids;

	public static Dictionary<EDLCKey, List<ECharacter>> DLCCharactersWithConfigUIs;

	public static Dictionary<EDLCKey, List<string>> DLCSpecificSpriteAssets;

	public static Dictionary<EDLCKey, List<ECharacter>> AllDLCCharacters => m_DLCSpecificCharacters;

	public static string GetEntitlementID(EDLCKey key)
	{
		return key switch
		{
			EDLCKey.DLC1 => "JOTL", 
			EDLCKey.DLC2 => "SOLO", 
			EDLCKey.None => string.Empty, 
			_ => string.Empty, 
		};
	}

	public static void FillDLCSpecificMatrices()
	{
		m_DLCSpecificBiomes = new Dictionary<EDLCKey, List<ScenarioPossibleRoom.EBiome>>();
		m_DLCSpecificSubBiomes = new Dictionary<EDLCKey, List<ScenarioPossibleRoom.ESubBiome>>();
		m_DLCSpecificThemes = new Dictionary<EDLCKey, List<ScenarioPossibleRoom.ETheme>>();
		m_DLCSpecificSubThemes = new Dictionary<EDLCKey, List<ScenarioPossibleRoom.ESubTheme>>();
		m_DLCSpecificTones = new Dictionary<EDLCKey, List<ScenarioPossibleRoom.ETone>>();
		m_DLCSpecificCharacters = new Dictionary<EDLCKey, List<ECharacter>>();
		m_DLCSpecificNpcModel = new Dictionary<EDLCKey, List<CClass.ENPCModel>>();
		m_DLCSpecificItemGuids = new Dictionary<EDLCKey, List<string>>();
		DLCCharactersWithConfigUIs = new Dictionary<EDLCKey, List<ECharacter>>();
		DLCSpecificSpriteAssets = new Dictionary<EDLCKey, List<string>>();
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != EDLCKey.None)
			{
				m_DLCSpecificBiomes.Add(eDLCKey, new List<ScenarioPossibleRoom.EBiome>());
				m_DLCSpecificSubBiomes.Add(eDLCKey, new List<ScenarioPossibleRoom.ESubBiome>());
				m_DLCSpecificThemes.Add(eDLCKey, new List<ScenarioPossibleRoom.ETheme>());
				m_DLCSpecificSubThemes.Add(eDLCKey, new List<ScenarioPossibleRoom.ESubTheme>());
				m_DLCSpecificTones.Add(eDLCKey, new List<ScenarioPossibleRoom.ETone>());
				m_DLCSpecificCharacters.Add(eDLCKey, new List<ECharacter>());
				m_DLCSpecificNpcModel.Add(eDLCKey, new List<CClass.ENPCModel>());
				m_DLCSpecificItemGuids.Add(eDLCKey, new List<string>());
				DLCCharactersWithConfigUIs.Add(eDLCKey, new List<ECharacter>());
				DLCSpecificSpriteAssets.Add(eDLCKey, new List<string>());
				switch (eDLCKey)
				{
				case EDLCKey.DLC1:
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_Ship);
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_Tunnels);
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_Platform);
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_RuinedSewer);
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_Void);
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_Arena);
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_Custom_01);
					m_DLCSpecificSubBiomes[eDLCKey].Add(ScenarioPossibleRoom.ESubBiome.DLC_SB_Custom_02);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Vermling);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_BlackSludge);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Abbatoir);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Gore);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Lab);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_AbWarehouse);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_TownGate);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Warehouse);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Custom_01);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Custom_02);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Custom_03);
					m_DLCSpecificThemes[eDLCKey].Add(ScenarioPossibleRoom.ETheme.DLC_TH_Custom_04);
					m_DLCSpecificSubThemes[eDLCKey].Add(ScenarioPossibleRoom.ESubTheme.DLC_SubTH_BloodCult);
					m_DLCSpecificSubThemes[eDLCKey].Add(ScenarioPossibleRoom.ESubTheme.DLC_SubTH_Custom_01);
					m_DLCSpecificSubThemes[eDLCKey].Add(ScenarioPossibleRoom.ESubTheme.DLC_SubTH_Custom_02);
					m_DLCSpecificTones[eDLCKey].Add(ScenarioPossibleRoom.ETone.DLC_Tone_Custom_01);
					m_DLCSpecificTones[eDLCKey].Add(ScenarioPossibleRoom.ETone.DLC_Tone_Custom_01);
					m_DLCSpecificTones[eDLCKey].Add(ScenarioPossibleRoom.ETone.DLC_Tone_Custom_01);
					m_DLCSpecificCharacters[eDLCKey].Add(ECharacter.Voidwarden);
					m_DLCSpecificCharacters[eDLCKey].Add(ECharacter.RedGuard);
					m_DLCSpecificCharacters[eDLCKey].Add(ECharacter.Hatchet);
					m_DLCSpecificCharacters[eDLCKey].Add(ECharacter.Demolitionist);
					m_DLCSpecificCharacters[eDLCKey].Add(ECharacter.DemolitionistMech);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BlackSludge);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BlackSludgeElite);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.FilthySludge);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BloodImp);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BloodImpElite);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BloodMonstrosity);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BloodMonstrosityElite);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.ChaosDemon);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.ChaosDemonElite);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.RatMonstrosity);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.RatMonstrosityElite);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.VermlingRaider);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.VermlingRaiderElite);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Zealot);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.ZealotElite);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.EntropyDemon);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd01);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd02);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd03);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd04);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd05);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd06);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd07);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Crowd08);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BloodHorror);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.BloodTumor);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.FirstOfTheOrder);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.SandDevil);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Ward);
					DLCCharactersWithConfigUIs[eDLCKey] = m_DLCSpecificCharacters[eDLCKey].ToList();
					DLCCharactersWithConfigUIs[eDLCKey].Remove(ECharacter.DemolitionistMech);
					DLCSpecificSpriteAssets[eDLCKey].Add("TMP_JOTL_AbilityCardSpriteAtlas");
					break;
				case EDLCKey.DLC2:
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.Rikharn);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.EarthLord);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.InoxNecromancer);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.SpiritBear);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.SpiritOfXorn);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.SongOfTheDeep);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.GhostWolf);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.DeepEarth);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.HighFlame);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.WoundedGuard);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.CityGuardLieutenant);
					m_DLCSpecificNpcModel[eDLCKey].Add(CClass.ENPCModel.VermlingDruid);
					break;
				}
			}
		}
	}

	public static EDLCKey GetDLCUsedInLevel(CCustomLevelData levelToCheck)
	{
		EDLCKey eDLCKey = EDLCKey.None;
		foreach (CApparanceOverrideDetails apparanceOverride in levelToCheck.ApparanceOverrideList)
		{
			eDLCKey |= GetDLCUsedInApparanceOverride(apparanceOverride);
		}
		foreach (PlayerState player in levelToCheck.ScenarioState.Players)
		{
			eDLCKey |= GetDLCUsedByPlayerState(player);
		}
		foreach (EnemyState allEnemyState in levelToCheck.ScenarioState.AllEnemyStates)
		{
			eDLCKey |= GetDLCUsedByEnemyState(allEnemyState);
		}
		return eDLCKey;
	}

	public static EDLCKey GetDLCUsedInApparanceOverride(CApparanceOverrideDetails overrideDetail)
	{
		EDLCKey eDLCKey = EDLCKey.None;
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey2 in dLCKeys)
		{
			if (eDLCKey2 != EDLCKey.None && (m_DLCSpecificBiomes[eDLCKey2].Contains(overrideDetail.OverrideBiome) || m_DLCSpecificSubBiomes[eDLCKey2].Contains(overrideDetail.OverrideSubBiome) || m_DLCSpecificThemes[eDLCKey2].Contains(overrideDetail.OverrideTheme) || m_DLCSpecificSubThemes[eDLCKey2].Contains(overrideDetail.OverrideSubTheme) || m_DLCSpecificTones[eDLCKey2].Contains(overrideDetail.OverrideTone)))
			{
				eDLCKey |= eDLCKey2;
			}
		}
		return eDLCKey;
	}

	public static EDLCKey GetDLCUsedByPlayerState(PlayerState playerState)
	{
		ECharacter characterModel = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass m) => m.ID == playerState.ClassID).CharacterModel;
		EDLCKey eDLCKey = EDLCKey.None;
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey dlc in dLCKeys)
		{
			if (dlc != EDLCKey.None && (m_DLCSpecificCharacters[dlc].Contains(characterModel) || playerState.Items.Any((CItem i) => m_DLCSpecificItemGuids[dlc].Contains(i.ItemGuid))))
			{
				eDLCKey |= dlc;
			}
		}
		return eDLCKey;
	}

	public static EDLCKey GetDLCUsedByEnemyState(EnemyState enemyState)
	{
		CMonsterClass enemyClass = MonsterClassManager.Find(enemyState.ClassID);
		EDLCKey eDLCKey = EDLCKey.None;
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey2 in dLCKeys)
		{
			if (eDLCKey2 != EDLCKey.None && m_DLCSpecificNpcModel != null && m_DLCSpecificNpcModel.ContainsKey(eDLCKey2) && m_DLCSpecificNpcModel[eDLCKey2].Any((CClass.ENPCModel m) => enemyClass?.Models != null && enemyClass.Models.Contains(m.ToString())))
			{
				eDLCKey |= eDLCKey2;
			}
		}
		return eDLCKey;
	}

	public static List<EDLCKey> GetDLCListForFlag(EDLCKey dlcFlag)
	{
		List<EDLCKey> list = new List<EDLCKey>();
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != EDLCKey.None && dlcFlag.HasFlag(eDLCKey))
			{
				list.Add(eDLCKey);
			}
		}
		return list;
	}

	public static List<string> GetDLCNamesForFlag(EDLCKey dlcFlag)
	{
		List<string> list = new List<string>();
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != EDLCKey.None && dlcFlag.HasFlag(eDLCKey))
			{
				list.Add(dlcFlag.ToString());
			}
		}
		return list;
	}

	public static string GetDLCListAsStringFromFlag(EDLCKey dlcFlag)
	{
		List<string> dLCNamesForFlag = GetDLCNamesForFlag(dlcFlag);
		string text = ((dLCNamesForFlag.Count == 0) ? "-" : "");
		for (int i = 0; i < dLCNamesForFlag.Count; i++)
		{
			text += string.Format("{0}{1}", (i > 0) ? ", " : "", dLCNamesForFlag[i]);
		}
		return text;
	}

	public static void RemoveBiomesBasedOnOwnedDLC(EDLCKey ownedDlcFlag, List<ScenarioPossibleRoom.EBiome> biomesToRemoveFrom)
	{
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey dlc in dLCKeys)
		{
			if (dlc != EDLCKey.None && !ownedDlcFlag.HasFlag(dlc))
			{
				biomesToRemoveFrom.RemoveAll((ScenarioPossibleRoom.EBiome b) => m_DLCSpecificBiomes[dlc].Contains(b));
			}
		}
	}

	public static void RemoveSubBiomesBasedOnOwnedDLC(EDLCKey ownedDlcFlag, List<ScenarioPossibleRoom.ESubBiome> subBiomesToRemoveFrom)
	{
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey dlc in dLCKeys)
		{
			if (dlc != EDLCKey.None && !ownedDlcFlag.HasFlag(dlc))
			{
				subBiomesToRemoveFrom.RemoveAll((ScenarioPossibleRoom.ESubBiome b) => m_DLCSpecificSubBiomes[dlc].Contains(b));
			}
		}
	}

	public static void RemoveThemeBasedOnOwnedDLC(EDLCKey ownedDlcFlag, List<ScenarioPossibleRoom.ETheme> themesToRemoveFrom)
	{
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey dlc in dLCKeys)
		{
			if (dlc != EDLCKey.None && !ownedDlcFlag.HasFlag(dlc))
			{
				themesToRemoveFrom.RemoveAll((ScenarioPossibleRoom.ETheme b) => m_DLCSpecificThemes[dlc].Contains(b));
			}
		}
	}

	public static void RemoveSubThemesBasedOnOwnedDLC(EDLCKey ownedDlcFlag, List<ScenarioPossibleRoom.ESubTheme> subThemesToRemoveFrom)
	{
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey dlc in dLCKeys)
		{
			if (dlc != EDLCKey.None && !ownedDlcFlag.HasFlag(dlc))
			{
				subThemesToRemoveFrom.RemoveAll((ScenarioPossibleRoom.ESubTheme b) => m_DLCSpecificSubThemes[dlc].Contains(b));
			}
		}
	}

	public static void RemoveToneBasedOnOwnedDLC(EDLCKey ownedDlcFlag, List<ScenarioPossibleRoom.ETone> tonesToRemoveFrom)
	{
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey dlc in dLCKeys)
		{
			if (dlc != EDLCKey.None && !ownedDlcFlag.HasFlag(dlc))
			{
				tonesToRemoveFrom.RemoveAll((ScenarioPossibleRoom.ETone b) => m_DLCSpecificTones[dlc].Contains(b));
			}
		}
	}
}
