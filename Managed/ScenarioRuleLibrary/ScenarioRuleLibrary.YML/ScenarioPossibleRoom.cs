using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class ScenarioPossibleRoom : ISerializable
{
	[Serializable]
	public enum EBiome
	{
		Inherit,
		Default,
		Crypt,
		Dungeon,
		Cave,
		Forest,
		City
	}

	[Serializable]
	public enum ESubBiome
	{
		Inherit,
		Default,
		Necropolis,
		Ruined,
		[Obsolete]
		Catacombs,
		Sewers,
		[Obsolete]
		Volcanic,
		[Obsolete]
		AncientCaverns,
		[Obsolete]
		DaggerForest,
		CorpseWood,
		Marsh,
		StoneRooms,
		[Obsolete]
		WoodenRooms,
		CityWalls,
		StoneWoodenRooms,
		[Obsolete]
		WoodenStoneRooms,
		Shack,
		Rot,
		SubBiome_01,
		SubBiome_02,
		SubBiome_03,
		SubBiome_04,
		SubBiome_05,
		DLC_SB_Ship,
		DLC_SB_Tunnels,
		DLC_SB_Platform,
		DLC_SB_RuinedSewer,
		DLC_SB_Void,
		DLC_SB_Arena,
		DLC_SB_Custom_01,
		DLC_SB_Custom_02
	}

	[Serializable]
	public enum ETheme
	{
		Inherit,
		Default,
		Torture,
		Library,
		Treasure,
		Kitchen,
		AlchemyLab,
		Chapel,
		GeneralStore,
		KitchenStore,
		TombStore,
		TortureStore,
		SculptorStudio,
		BurialChamber,
		Mausoleum,
		Shrine,
		Temple,
		AncientLibrary,
		Study,
		[Obsolete]
		Well,
		[Obsolete]
		DruidTemple,
		[Obsolete]
		GuardRoom,
		[Obsolete]
		Prison,
		[Obsolete]
		CharnalHouse,
		[Obsolete]
		CessPool,
		[Obsolete]
		Cistern,
		Armoury,
		AncientTemple,
		DeepForestGlade,
		DruidsGrove,
		InoxBurialGround,
		InoxCamp,
		MagicGrotto,
		MushroomLand,
		StillWaters,
		WoodcuttersCottage,
		VermlingVillage,
		[Obsolete]
		FungalWasteland,
		[Obsolete]
		StagnantPits,
		[Obsolete]
		ForestGraveyard,
		[Obsolete]
		PlaguePits,
		[Obsolete]
		EvilDead,
		Volcanic,
		Ice,
		RockTemple,
		OldMine,
		[Obsolete]
		Storeroom,
		[Obsolete]
		BanquetHall,
		[Obsolete]
		Tavern,
		[Obsolete]
		Church,
		[Obsolete]
		DruidStore,
		[Obsolete]
		Bedroom,
		[Obsolete]
		Pantry,
		[Obsolete]
		Apothecary,
		[Obsolete]
		Hallway,
		[Obsolete]
		Gallery,
		[Obsolete]
		Bordello,
		[Obsolete]
		PsychoHut,
		[Obsolete]
		HermitHut,
		[Obsolete]
		HuntersCabin,
		[Obsolete]
		TownStreet,
		[Obsolete]
		TownWalls,
		Miscellaneous,
		Theme_01,
		Theme_02,
		Theme_03,
		Theme_04,
		Theme_05,
		DLC_TH_Vermling,
		DLC_TH_BlackSludge,
		DLC_TH_Abbatoir,
		DLC_TH_Gore,
		DLC_TH_Lab,
		DLC_TH_AbWarehouse,
		DLC_TH_TownGate,
		DLC_TH_Warehouse,
		DLC_TH_Custom_01,
		DLC_TH_Custom_02,
		DLC_TH_Custom_03,
		DLC_TH_Custom_04
	}

	[Serializable]
	public enum ESubTheme
	{
		Inherit,
		Default,
		Bandit,
		Undead,
		Demon,
		Cultist,
		Animal,
		FireDemon,
		IceDemon,
		TownMilitia,
		DLC_SubTH_BloodCult,
		DLC_SubTH_Custom_01,
		DLC_SubTH_Custom_02
	}

	[Serializable]
	public enum ETone
	{
		Inherit,
		Default,
		Dark,
		Natural,
		Spectral,
		Demonic,
		Toxic,
		Evil,
		Crystal,
		ForestDefault,
		ForestEerie,
		ForestMoonlight,
		ForestFairy,
		Bioluminescence,
		LavaLamp,
		Gaslight,
		Candlelight,
		Campaign_01,
		Campaign_02,
		Campaign_03,
		Campaign_04,
		Campaign_05,
		DLC_Tone_Custom_01,
		DLC_Tone_Custom_02,
		DLC_Tone_Custom_03
	}

	public static readonly EBiome[] Biomes = (EBiome[])Enum.GetValues(typeof(EBiome));

	public static readonly ESubBiome[] SubBiomes = (ESubBiome[])Enum.GetValues(typeof(ESubBiome));

	public static readonly ETheme[] Themes = (ETheme[])Enum.GetValues(typeof(ETheme));

	public static readonly ESubTheme[] SubThemes = (ESubTheme[])Enum.GetValues(typeof(ESubTheme));

	public static readonly ETone[] Tones = (ETone[])Enum.GetValues(typeof(ETone));

	public string Name { get; private set; }

	public string MonsterGroup { get; private set; }

	public EBiome Biome { get; private set; }

	public ESubBiome SubBiome { get; private set; }

	public ETheme Theme { get; private set; }

	public ESubTheme SubTheme { get; private set; }

	public ETone Tone { get; private set; }

	public List<int> OneHexObstacles { get; private set; }

	public List<int> TwoHexObstacles { get; private set; }

	public List<int> ThreeHexObstacles { get; private set; }

	public List<int> Traps { get; private set; }

	public List<int> PressurePlates { get; private set; }

	public List<int> TerrainHotCoals { get; private set; }

	public List<int> TerrainWater { get; private set; }

	public List<int> TerrainThorns { get; private set; }

	public List<int> TerrainRubble { get; private set; }

	public List<int> GoldPiles { get; private set; }

	public List<int> TreasureChestChance { get; private set; }

	public List<string> ChestTreasureTables { get; private set; }

	public List<List<string>> GoalChests { get; private set; }

	public List<SpawnerData> SpawnerDatas { get; private set; }

	public List<Tuple<string, List<int>>> AllyMonsters { get; private set; }

	public List<Tuple<string, List<int>>> Enemy2Monsters { get; private set; }

	public List<Tuple<string, List<int>>> Objects { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("MonsterGroup", MonsterGroup);
		info.AddValue("Biome", Biome);
		info.AddValue("SubBiome", SubBiome);
		info.AddValue("Theme", Theme);
		info.AddValue("SubTheme", SubTheme);
		info.AddValue("Tone", Tone);
		info.AddValue("OneHexObstacles", OneHexObstacles);
		info.AddValue("TwoHexObstacles", TwoHexObstacles);
		info.AddValue("ThreeHexObstacles", ThreeHexObstacles);
		info.AddValue("Traps", Traps);
		info.AddValue("TerrainHotCoals", TerrainHotCoals);
		info.AddValue("TerrainWater", TerrainWater);
		info.AddValue("TerrainThorns", TerrainThorns);
		info.AddValue("TerrainRubble", TerrainRubble);
		info.AddValue("GoldPiles", GoldPiles);
		info.AddValue("TreasureChestChance", TreasureChestChance);
		info.AddValue("ChestTreasureTables", ChestTreasureTables);
		info.AddValue("GoalChests", GoalChests);
		info.AddValue("SpawnerDatas", SpawnerDatas);
		info.AddValue("AllyMonsters", AllyMonsters);
		info.AddValue("Enemy2Monsters", Enemy2Monsters);
		info.AddValue("Objects", Objects);
	}

	protected ScenarioPossibleRoom(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Name":
					Name = info.GetString("Name");
					break;
				case "MonsterGroup":
					MonsterGroup = info.GetString("MonsterGroup");
					break;
				case "Biome":
					Biome = (EBiome)info.GetValue("Biome", typeof(EBiome));
					break;
				case "SubBiome":
					SubBiome = (ESubBiome)info.GetValue("SubBiome", typeof(ESubBiome));
					break;
				case "Theme":
					Theme = (ETheme)info.GetValue("Theme", typeof(ETheme));
					break;
				case "SubTheme":
					SubTheme = (ESubTheme)info.GetValue("SubTheme", typeof(ESubTheme));
					break;
				case "Tone":
					Tone = (ETone)info.GetValue("Tone", typeof(ETone));
					break;
				case "OneHexObstacles":
					OneHexObstacles = (List<int>)info.GetValue("OneHexObstacles", typeof(List<int>));
					break;
				case "TwoHexObstacles":
					TwoHexObstacles = (List<int>)info.GetValue("TwoHexObstacles", typeof(List<int>));
					break;
				case "ThreeHexObstacles":
					ThreeHexObstacles = (List<int>)info.GetValue("ThreeHexObstacles", typeof(List<int>));
					break;
				case "Traps":
					Traps = (List<int>)info.GetValue("Traps", typeof(List<int>));
					break;
				case "TerrainHotCoals":
					TerrainHotCoals = (List<int>)info.GetValue("TerrainHotCoals", typeof(List<int>));
					break;
				case "TerrainWater":
					TerrainWater = (List<int>)info.GetValue("TerrainWater", typeof(List<int>));
					break;
				case "TerrainThorns":
					TerrainThorns = (List<int>)info.GetValue("TerrainThorns", typeof(List<int>));
					break;
				case "TerrainRubble":
					TerrainRubble = (List<int>)info.GetValue("TerrainRubble", typeof(List<int>));
					break;
				case "GoldPiles":
					GoldPiles = (List<int>)info.GetValue("GoldPiles", typeof(List<int>));
					break;
				case "TreasureChestChance":
					TreasureChestChance = (List<int>)info.GetValue("TreasureChestChance", typeof(List<int>));
					break;
				case "ChestTreasureTables":
					ChestTreasureTables = (List<string>)info.GetValue("ChestTreasureTables", typeof(List<string>));
					break;
				case "GoalChests":
					GoalChests = (List<List<string>>)info.GetValue("GoalChests", typeof(List<List<string>>));
					break;
				case "SpawnerDatas":
					SpawnerDatas = (List<SpawnerData>)info.GetValue("SpawnerDatas", typeof(List<SpawnerData>));
					break;
				case "AllyMonsters":
					try
					{
						AllyMonsters = (List<Tuple<string, List<int>>>)info.GetValue("AllyMonsters", typeof(List<Tuple<string, List<int>>>));
					}
					catch
					{
						AllyMonsters = new List<Tuple<string, List<int>>>();
					}
					break;
				case "Enemy2Monsters":
					Enemy2Monsters = (List<Tuple<string, List<int>>>)info.GetValue("Enemy2Monsters", typeof(List<Tuple<string, List<int>>>));
					break;
				case "NeutralMonsters":
					Enemy2Monsters = (List<Tuple<string, List<int>>>)info.GetValue("NeutralMonsters", typeof(List<Tuple<string, List<int>>>));
					break;
				case "Objects":
					Objects = (List<Tuple<string, List<int>>>)info.GetValue("Objects", typeof(List<Tuple<string, List<int>>>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize ScenarioPossibleRoom entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public ScenarioPossibleRoom(string name, string monsterGroup, EBiome biome, ESubBiome subBiome, ETheme theme, ESubTheme subTheme, ETone tone, List<int> oneHexObstacles, List<int> twoHexObstacles, List<int> threeHexObstacles, List<int> traps, List<int> pressurePlates, List<int> terrainHotCoals, List<int> terrainWater, List<int> terrainThorns, List<int> terrainRubble, List<int> goldPiles, List<int> treasureChestChance, List<string> chestTreasureTables, List<List<string>> goalChests, List<SpawnerData> spawnerDatas, List<Tuple<string, List<int>>> allyMonsters, List<Tuple<string, List<int>>> enemy2Monsters, List<Tuple<string, List<int>>> objects)
	{
		Name = name;
		MonsterGroup = monsterGroup;
		Biome = biome;
		SubBiome = subBiome;
		Theme = theme;
		SubTheme = subTheme;
		Tone = tone;
		OneHexObstacles = oneHexObstacles;
		TwoHexObstacles = twoHexObstacles;
		ThreeHexObstacles = threeHexObstacles;
		Traps = traps;
		PressurePlates = pressurePlates;
		TerrainHotCoals = terrainHotCoals;
		TerrainWater = terrainWater;
		TerrainThorns = terrainThorns;
		TerrainRubble = terrainRubble;
		GoldPiles = goldPiles;
		TreasureChestChance = treasureChestChance;
		ChestTreasureTables = chestTreasureTables;
		GoalChests = goalChests;
		SpawnerDatas = spawnerDatas;
		AllyMonsters = allyMonsters;
		Enemy2Monsters = enemy2Monsters;
		Objects = objects;
	}

	public bool Validate(string fileName)
	{
		bool result = true;
		if (string.IsNullOrEmpty(Name))
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Name entry in ScenarioPossibleRoom file:\n" + fileName);
			result = false;
		}
		if (AllyMonsters != null && AllyMonsters.Count > 0)
		{
			foreach (Tuple<string, List<int>> allyMonster in AllyMonsters)
			{
				if (allyMonster.Item2.Count != 4)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Too many values in AllyMonster list for " + allyMonster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
				if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == allyMonster.Item1) == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster with matching ID for " + allyMonster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
			}
		}
		if (Enemy2Monsters != null && Enemy2Monsters.Count > 0)
		{
			foreach (Tuple<string, List<int>> enemy2Monster in Enemy2Monsters)
			{
				if (enemy2Monster.Item2.Count != 4)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Too many values in Enemy2Monsters list for " + enemy2Monster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
				if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == enemy2Monster.Item1) == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster with matching ID for " + enemy2Monster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
			}
		}
		if (Objects != null && Objects.Count > 0)
		{
			foreach (Tuple<string, List<int>> objects in Objects)
			{
				if (objects.Item2.Count != 4)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Too many values in Objects list for " + objects.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
				if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == objects.Item1) == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster with matching ID for " + objects.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
			}
		}
		return result;
	}

	public void SetBiome(EBiome biome)
	{
		Biome = biome;
	}

	public void SetSubBiome(ESubBiome subBiome)
	{
		SubBiome = subBiome;
	}

	public void SetTheme(ETheme theme)
	{
		Theme = theme;
	}

	public void SetSubTheme(ESubTheme subTheme)
	{
		SubTheme = subTheme;
	}

	public void SetTone(ETone tone)
	{
		Tone = tone;
	}

	public ScenarioPossibleRoom()
	{
	}

	public ScenarioPossibleRoom(ScenarioPossibleRoom state, ReferenceDictionary references)
	{
		Name = state.Name;
		MonsterGroup = state.MonsterGroup;
		Biome = state.Biome;
		SubBiome = state.SubBiome;
		Theme = state.Theme;
		SubTheme = state.SubTheme;
		Tone = state.Tone;
		OneHexObstacles = references.Get(state.OneHexObstacles);
		if (OneHexObstacles == null && state.OneHexObstacles != null)
		{
			OneHexObstacles = new List<int>();
			for (int i = 0; i < state.OneHexObstacles.Count; i++)
			{
				int item = state.OneHexObstacles[i];
				OneHexObstacles.Add(item);
			}
			references.Add(state.OneHexObstacles, OneHexObstacles);
		}
		TwoHexObstacles = references.Get(state.TwoHexObstacles);
		if (TwoHexObstacles == null && state.TwoHexObstacles != null)
		{
			TwoHexObstacles = new List<int>();
			for (int j = 0; j < state.TwoHexObstacles.Count; j++)
			{
				int item2 = state.TwoHexObstacles[j];
				TwoHexObstacles.Add(item2);
			}
			references.Add(state.TwoHexObstacles, TwoHexObstacles);
		}
		ThreeHexObstacles = references.Get(state.ThreeHexObstacles);
		if (ThreeHexObstacles == null && state.ThreeHexObstacles != null)
		{
			ThreeHexObstacles = new List<int>();
			for (int k = 0; k < state.ThreeHexObstacles.Count; k++)
			{
				int item3 = state.ThreeHexObstacles[k];
				ThreeHexObstacles.Add(item3);
			}
			references.Add(state.ThreeHexObstacles, ThreeHexObstacles);
		}
		Traps = references.Get(state.Traps);
		if (Traps == null && state.Traps != null)
		{
			Traps = new List<int>();
			for (int l = 0; l < state.Traps.Count; l++)
			{
				int item4 = state.Traps[l];
				Traps.Add(item4);
			}
			references.Add(state.Traps, Traps);
		}
		PressurePlates = references.Get(state.PressurePlates);
		if (PressurePlates == null && state.PressurePlates != null)
		{
			PressurePlates = new List<int>();
			for (int m = 0; m < state.PressurePlates.Count; m++)
			{
				int item5 = state.PressurePlates[m];
				PressurePlates.Add(item5);
			}
			references.Add(state.PressurePlates, PressurePlates);
		}
		TerrainHotCoals = references.Get(state.TerrainHotCoals);
		if (TerrainHotCoals == null && state.TerrainHotCoals != null)
		{
			TerrainHotCoals = new List<int>();
			for (int n = 0; n < state.TerrainHotCoals.Count; n++)
			{
				int item6 = state.TerrainHotCoals[n];
				TerrainHotCoals.Add(item6);
			}
			references.Add(state.TerrainHotCoals, TerrainHotCoals);
		}
		TerrainWater = references.Get(state.TerrainWater);
		if (TerrainWater == null && state.TerrainWater != null)
		{
			TerrainWater = new List<int>();
			for (int num = 0; num < state.TerrainWater.Count; num++)
			{
				int item7 = state.TerrainWater[num];
				TerrainWater.Add(item7);
			}
			references.Add(state.TerrainWater, TerrainWater);
		}
		TerrainThorns = references.Get(state.TerrainThorns);
		if (TerrainThorns == null && state.TerrainThorns != null)
		{
			TerrainThorns = new List<int>();
			for (int num2 = 0; num2 < state.TerrainThorns.Count; num2++)
			{
				int item8 = state.TerrainThorns[num2];
				TerrainThorns.Add(item8);
			}
			references.Add(state.TerrainThorns, TerrainThorns);
		}
		TerrainRubble = references.Get(state.TerrainRubble);
		if (TerrainRubble == null && state.TerrainRubble != null)
		{
			TerrainRubble = new List<int>();
			for (int num3 = 0; num3 < state.TerrainRubble.Count; num3++)
			{
				int item9 = state.TerrainRubble[num3];
				TerrainRubble.Add(item9);
			}
			references.Add(state.TerrainRubble, TerrainRubble);
		}
		GoldPiles = references.Get(state.GoldPiles);
		if (GoldPiles == null && state.GoldPiles != null)
		{
			GoldPiles = new List<int>();
			for (int num4 = 0; num4 < state.GoldPiles.Count; num4++)
			{
				int item10 = state.GoldPiles[num4];
				GoldPiles.Add(item10);
			}
			references.Add(state.GoldPiles, GoldPiles);
		}
		TreasureChestChance = references.Get(state.TreasureChestChance);
		if (TreasureChestChance == null && state.TreasureChestChance != null)
		{
			TreasureChestChance = new List<int>();
			for (int num5 = 0; num5 < state.TreasureChestChance.Count; num5++)
			{
				int item11 = state.TreasureChestChance[num5];
				TreasureChestChance.Add(item11);
			}
			references.Add(state.TreasureChestChance, TreasureChestChance);
		}
		ChestTreasureTables = references.Get(state.ChestTreasureTables);
		if (ChestTreasureTables == null && state.ChestTreasureTables != null)
		{
			ChestTreasureTables = new List<string>();
			for (int num6 = 0; num6 < state.ChestTreasureTables.Count; num6++)
			{
				string item12 = state.ChestTreasureTables[num6];
				ChestTreasureTables.Add(item12);
			}
			references.Add(state.ChestTreasureTables, ChestTreasureTables);
		}
		GoalChests = references.Get(state.GoalChests);
		if (GoalChests == null && state.GoalChests != null)
		{
			GoalChests = new List<List<string>>();
			for (int num7 = 0; num7 < state.GoalChests.Count; num7++)
			{
				List<string> list = state.GoalChests[num7];
				List<string> list2 = references.Get(list);
				if (list2 == null && list != null)
				{
					list2 = new List<string>();
					for (int num8 = 0; num8 < list.Count; num8++)
					{
						string item13 = list[num8];
						list2.Add(item13);
					}
					references.Add(list, list2);
				}
				GoalChests.Add(list2);
			}
			references.Add(state.GoalChests, GoalChests);
		}
		SpawnerDatas = references.Get(state.SpawnerDatas);
		if (SpawnerDatas == null && state.SpawnerDatas != null)
		{
			SpawnerDatas = new List<SpawnerData>();
			for (int num9 = 0; num9 < state.SpawnerDatas.Count; num9++)
			{
				SpawnerData spawnerData = state.SpawnerDatas[num9];
				SpawnerData spawnerData2 = references.Get(spawnerData);
				if (spawnerData2 == null && spawnerData != null)
				{
					spawnerData2 = new SpawnerData(spawnerData, references);
					references.Add(spawnerData, spawnerData2);
				}
				SpawnerDatas.Add(spawnerData2);
			}
			references.Add(state.SpawnerDatas, SpawnerDatas);
		}
		AllyMonsters = references.Get(state.AllyMonsters);
		if (AllyMonsters == null && state.AllyMonsters != null)
		{
			AllyMonsters = new List<Tuple<string, List<int>>>();
			for (int num10 = 0; num10 < state.AllyMonsters.Count; num10++)
			{
				Tuple<string, List<int>> tuple = state.AllyMonsters[num10];
				string item14 = tuple.Item1;
				List<int> list3 = references.Get(tuple.Item2);
				if (list3 == null && tuple.Item2 != null)
				{
					list3 = new List<int>();
					for (int num11 = 0; num11 < tuple.Item2.Count; num11++)
					{
						int item15 = tuple.Item2[num11];
						list3.Add(item15);
					}
					references.Add(tuple.Item2, list3);
				}
				Tuple<string, List<int>> item16 = new Tuple<string, List<int>>(item14, list3);
				AllyMonsters.Add(item16);
			}
			references.Add(state.AllyMonsters, AllyMonsters);
		}
		Enemy2Monsters = references.Get(state.Enemy2Monsters);
		if (Enemy2Monsters == null && state.Enemy2Monsters != null)
		{
			Enemy2Monsters = new List<Tuple<string, List<int>>>();
			for (int num12 = 0; num12 < state.Enemy2Monsters.Count; num12++)
			{
				Tuple<string, List<int>> tuple2 = state.Enemy2Monsters[num12];
				string item17 = tuple2.Item1;
				List<int> list4 = references.Get(tuple2.Item2);
				if (list4 == null && tuple2.Item2 != null)
				{
					list4 = new List<int>();
					for (int num13 = 0; num13 < tuple2.Item2.Count; num13++)
					{
						int item18 = tuple2.Item2[num13];
						list4.Add(item18);
					}
					references.Add(tuple2.Item2, list4);
				}
				Tuple<string, List<int>> item19 = new Tuple<string, List<int>>(item17, list4);
				Enemy2Monsters.Add(item19);
			}
			references.Add(state.Enemy2Monsters, Enemy2Monsters);
		}
		Objects = references.Get(state.Objects);
		if (Objects != null || state.Objects == null)
		{
			return;
		}
		Objects = new List<Tuple<string, List<int>>>();
		for (int num14 = 0; num14 < state.Objects.Count; num14++)
		{
			Tuple<string, List<int>> tuple3 = state.Objects[num14];
			string item20 = tuple3.Item1;
			List<int> list5 = references.Get(tuple3.Item2);
			if (list5 == null && tuple3.Item2 != null)
			{
				list5 = new List<int>();
				for (int num15 = 0; num15 < tuple3.Item2.Count; num15++)
				{
					int item21 = tuple3.Item2[num15];
					list5.Add(item21);
				}
				references.Add(tuple3.Item2, list5);
			}
			Tuple<string, List<int>> item22 = new Tuple<string, List<int>>(item20, list5);
			Objects.Add(item22);
		}
		references.Add(state.Objects, Objects);
	}
}
