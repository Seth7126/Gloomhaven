using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.Adventure;

[Serializable]
public class CAdventureDifficulty : ISerializable
{
	public static EAdventureDifficulty[] AdventureDifficulties = (EAdventureDifficulty[])Enum.GetValues(typeof(EAdventureDifficulty));

	public const float cDefaultThreatModifier = 1f;

	public const float cDefaultHeroHealthModifier = 1f;

	public const float cDefaultXPModifier = 1f;

	public const float cDefaultGoldModifier = 1f;

	public const int cDefaultBlessCards = 0;

	public const int cDefaultCurseCards = 0;

	public const int cDefaultScenarioLevelModifier = 0;

	public string Text;

	public List<EAdventureDifficulty> ActiveOn;

	public float ThreatModifier;

	public float HeroHealthModifier;

	public float XPModifier;

	public float GoldModifier;

	public int BlessCards;

	public int CurseCards;

	public bool PositiveEffect;

	public int ScenarioLevelModifier;

	public bool LoadAsNewDifficulty;

	public bool HasThreatModifier => ThreatModifier != 1f;

	public bool HasHeroHealthModifier => HeroHealthModifier != 1f;

	public bool HasXPModifier => XPModifier != 1f;

	public bool HasGoldModifier => GoldModifier != 1f;

	public bool HasBlessCards => BlessCards != 0;

	public bool HasCurseCards => CurseCards != 0;

	public bool HasScenarioLevelModifier => ScenarioLevelModifier != 0;

	public CAdventureDifficulty(string text, List<EAdventureDifficulty> activeOn, bool positiveEffect, float threat = 1f, float health = 1f, float xp = 1f, float gold = 1f, int bless = 0, int curse = 0, int enemyLevelModifier = 0, bool loadAsNewDifficulty = false)
	{
		Text = text;
		ActiveOn = activeOn;
		ThreatModifier = threat;
		HeroHealthModifier = health;
		XPModifier = xp;
		GoldModifier = gold;
		BlessCards = bless;
		CurseCards = curse;
		PositiveEffect = positiveEffect;
		ScenarioLevelModifier = enemyLevelModifier;
		LoadAsNewDifficulty = loadAsNewDifficulty;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Text", Text);
		info.AddValue("ActiveOn", ActiveOn);
		info.AddValue("ThreatModifier", ThreatModifier);
		info.AddValue("HeroHealthModifier", HeroHealthModifier);
		info.AddValue("XPModifier", XPModifier);
		info.AddValue("GoldModifier", GoldModifier);
		info.AddValue("BlessCards", BlessCards);
		info.AddValue("CurseCards", CurseCards);
		info.AddValue("PositiveEffect", PositiveEffect);
		info.AddValue("EnemyLevelModifier", ScenarioLevelModifier);
	}

	private CAdventureDifficulty(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Text":
					Text = info.GetString("Text");
					break;
				case "ActiveOn":
					ActiveOn = (List<EAdventureDifficulty>)info.GetValue("ActiveOn", typeof(List<EAdventureDifficulty>));
					break;
				case "ThreatModifier":
					ThreatModifier = (float)info.GetDouble("ThreatModifier");
					break;
				case "HeroHealthModifier":
					HeroHealthModifier = (float)info.GetDouble("HeroHealthModifier");
					break;
				case "XPModifier":
					XPModifier = (float)info.GetDouble("XPModifier");
					break;
				case "GoldModifier":
					GoldModifier = (float)info.GetDouble("GoldModifier");
					break;
				case "BlessCards":
					BlessCards = info.GetInt32("BlessCards");
					break;
				case "CurseCards":
					CurseCards = info.GetInt32("CurseCards");
					break;
				case "PositiveEffect":
					PositiveEffect = info.GetBoolean("PositiveEffect");
					break;
				case "EnemyLevelModifier":
					ScenarioLevelModifier = info.GetInt32("EnemyLevelModifier");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CAdventureDifficulty entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAdventureDifficulty()
	{
	}

	public CAdventureDifficulty(CAdventureDifficulty state, ReferenceDictionary references)
	{
		Text = state.Text;
		ActiveOn = references.Get(state.ActiveOn);
		if (ActiveOn == null && state.ActiveOn != null)
		{
			ActiveOn = new List<EAdventureDifficulty>();
			for (int i = 0; i < state.ActiveOn.Count; i++)
			{
				EAdventureDifficulty item = state.ActiveOn[i];
				ActiveOn.Add(item);
			}
			references.Add(state.ActiveOn, ActiveOn);
		}
		ThreatModifier = state.ThreatModifier;
		HeroHealthModifier = state.HeroHealthModifier;
		XPModifier = state.XPModifier;
		GoldModifier = state.GoldModifier;
		BlessCards = state.BlessCards;
		CurseCards = state.CurseCards;
		PositiveEffect = state.PositiveEffect;
		ScenarioLevelModifier = state.ScenarioLevelModifier;
		LoadAsNewDifficulty = state.LoadAsNewDifficulty;
	}
}
