using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary.YML;

public class BaseStats
{
	public int Level { get; private set; }

	public int Health { get; set; }

	public int Move { get; set; }

	public int Attack { get; set; }

	public int Range { get; set; }

	public int Target { get; set; }

	public int Shield { get; set; }

	public int Retaliate { get; set; }

	public int RetaliateRange { get; set; }

	public int Pierce { get; set; }

	public bool Flying { get; private set; }

	public bool Advantage { get; private set; }

	public bool AttackersGainDisadvantage { get; private set; }

	public bool Invulnerable { get; private set; }

	public bool PierceInvulnerablility { get; private set; }

	public bool Untargetable { get; private set; }

	public List<CCondition.ENegativeCondition> Conditions { get; private set; }

	public List<CAbility.EAbilityType> Immunities { get; private set; }

	public Dictionary<CAbility.EAbilityType, int> SpecialBaseStats { get; private set; }

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; private set; }

	public List<CAbility> OnDeathAbilities { get; set; }

	public BaseStats(int level, int health, int move, int attack, int range, int target, int shield, int retaliate, int retaliateRange, int pierce, bool flying, bool advantage, bool attackersGainDisadvantage, bool invulnerable, bool pierceInvulnerablility, bool untargetable, List<CCondition.ENegativeCondition> conditions, List<CAbility.EAbilityType> immunities, Dictionary<CAbility.EAbilityType, int> specialBaseStats, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries, List<CAbility> onDeathAbilities)
	{
		Level = level;
		Health = health;
		Move = move;
		Attack = attack;
		Range = range;
		Target = target;
		Shield = shield;
		Retaliate = retaliate;
		RetaliateRange = retaliateRange;
		Pierce = pierce;
		Flying = flying;
		Advantage = advantage;
		AttackersGainDisadvantage = attackersGainDisadvantage;
		Invulnerable = invulnerable;
		PierceInvulnerablility = pierceInvulnerablility;
		Untargetable = untargetable;
		Conditions = conditions;
		Immunities = immunities;
		SpecialBaseStats = specialBaseStats;
		StatIsBasedOnXEntries = statIsBasedOnXEntries;
		OnDeathAbilities = onDeathAbilities;
	}

	public BaseStats Copy()
	{
		int level = Level;
		int health = Health;
		int move = Move;
		int attack = Attack;
		int range = Range;
		int target = Target;
		int shield = Shield;
		int retaliate = Retaliate;
		int retaliateRange = RetaliateRange;
		int pierce = Pierce;
		bool flying = Flying;
		bool advantage = Advantage;
		bool attackersGainDisadvantage = AttackersGainDisadvantage;
		bool invulnerable = Invulnerable;
		bool pierceInvulnerablility = PierceInvulnerablility;
		bool untargetable = (Untargetable = Untargetable);
		return new BaseStats(level, health, move, attack, range, target, shield, retaliate, retaliateRange, pierce, flying, advantage, attackersGainDisadvantage, invulnerable, pierceInvulnerablility, untargetable, Conditions.ToList(), Immunities.ToList(), new Dictionary<CAbility.EAbilityType, int>(SpecialBaseStats), StatIsBasedOnXEntries?.Select((AbilityData.StatIsBasedOnXData x) => x.Copy()).ToList(), OnDeathAbilities?.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false, fullCopy: true)).ToList());
	}
}
