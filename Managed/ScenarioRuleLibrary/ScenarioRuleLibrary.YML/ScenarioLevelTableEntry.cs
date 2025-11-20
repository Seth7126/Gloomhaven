namespace ScenarioRuleLibrary.YML;

public class ScenarioLevelTableEntry
{
	public int GoldConversion;

	public int TrapDamage;

	public int BonusXP;

	public int HazardousTerrainDamage;

	public ScenarioLevelTableEntry(int goldConversion, int trapDamage, int bonusXP, int hazardousTerrainDamage)
	{
		GoldConversion = goldConversion;
		TrapDamage = trapDamage;
		BonusXP = bonusXP;
		HazardousTerrainDamage = hazardousTerrainDamage;
	}
}
