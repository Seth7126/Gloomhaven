using System;
using System.Collections.Generic;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class MonsterYMLData
{
	public static EMonsterBaseStats[] MonsterBaseStatsEnums = (EMonsterBaseStats[])Enum.GetValues(typeof(EMonsterBaseStats));

	public static EMonsterType[] MonsterTypes = (EMonsterType[])Enum.GetValues(typeof(EMonsterType));

	public const int NumberOfBaseStatLevels = 8;

	public string ID { get; set; }

	public string LocKey { get; set; }

	public List<string> Models { get; set; }

	public EMonsterType MonsterType { get; set; }

	public string NonEliteVariant { get; set; }

	public int? StandeeLimit { get; set; }

	public bool? PredominantlyMeleeNullable { get; set; }

	public List<MonsterCardYMLData> AbilityCards { get; set; }

	public List<EMonsterBaseStats> AddNumberOfPlayersTo { get; set; }

	public List<BaseStats> MonsterBaseStats { get; set; }

	public string MonsterClassIDToActImmediatelyBefore { get; set; }

	public string ColourHTML { get; set; }

	public float Fatness { get; set; }

	public float VertexAnimIntensity { get; set; }

	public bool UseNormalSizeAvatarForBoss { get; set; }

	public string CustomConfig { get; set; }

	public bool PredominantlyMelee
	{
		get
		{
			if (!PredominantlyMeleeNullable.HasValue)
			{
				return false;
			}
			return PredominantlyMeleeNullable.Value;
		}
		set
		{
			PredominantlyMeleeNullable = value;
		}
	}

	public string FileName { get; private set; }

	public MonsterYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		LocKey = null;
		Models = null;
		MonsterType = EMonsterType.None;
		NonEliteVariant = null;
		StandeeLimit = null;
		PredominantlyMeleeNullable = null;
		AbilityCards = null;
		AddNumberOfPlayersTo = null;
		MonsterBaseStats = null;
		MonsterClassIDToActImmediatelyBefore = null;
		ColourHTML = "#FFFFFF";
		Fatness = 0f;
		VertexAnimIntensity = 0f;
		UseNormalSizeAvatarForBoss = false;
		CustomConfig = null;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID specified for monster in file " + FileName);
			return false;
		}
		if (LocKey == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No LocKey specified for monster in file " + FileName);
			return false;
		}
		if (Models == null || Models.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Model specified for monster in file " + FileName);
			return false;
		}
		if (MonsterBaseStats == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterBaseStats specified for monster in file " + FileName);
			return false;
		}
		if (MonsterBaseStats.Count != 8)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterBaseStats does not have " + 8 + " levels specified." + FileName);
			return false;
		}
		if (MonsterType == EMonsterType.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterType specified for monster in file " + FileName);
			return false;
		}
		if (!StandeeLimit.HasValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No StandeeLimit specified for monster in file " + FileName);
			return false;
		}
		if (MonsterType != EMonsterType.Object && !PredominantlyMeleeNullable.HasValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PredominantlyMelee value specified for monster in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(MonsterYMLData data)
	{
		if (data.LocKey != null)
		{
			LocKey = data.LocKey;
		}
		if (data.Models != null)
		{
			Models = data.Models;
		}
		if (data.MonsterType != EMonsterType.None)
		{
			MonsterType = data.MonsterType;
		}
		if (data.NonEliteVariant != null)
		{
			NonEliteVariant = data.NonEliteVariant;
		}
		if (data.PredominantlyMeleeNullable.HasValue)
		{
			PredominantlyMeleeNullable = data.PredominantlyMeleeNullable;
		}
		if (data.AbilityCards != null)
		{
			AbilityCards = data.AbilityCards;
		}
		if (data.AddNumberOfPlayersTo != null)
		{
			AddNumberOfPlayersTo = data.AddNumberOfPlayersTo;
		}
		if (data.MonsterBaseStats != null)
		{
			MonsterBaseStats = data.MonsterBaseStats;
		}
		if (data.StandeeLimit.HasValue)
		{
			StandeeLimit = data.StandeeLimit;
		}
		if (data.ColourHTML != "#FFFFFF")
		{
			ColourHTML = data.ColourHTML;
		}
		if (data.Fatness != 0f)
		{
			Fatness = data.Fatness;
		}
		if (data.VertexAnimIntensity != 0f)
		{
			VertexAnimIntensity = data.VertexAnimIntensity;
		}
		if (data.CustomConfig != null)
		{
			CustomConfig = data.CustomConfig;
		}
		UseNormalSizeAvatarForBoss = data.UseNormalSizeAvatarForBoss;
	}
}
