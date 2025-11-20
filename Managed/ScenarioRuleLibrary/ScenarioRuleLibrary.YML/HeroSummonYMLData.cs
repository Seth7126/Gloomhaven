using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class HeroSummonYMLData
{
	public string ID { get; set; }

	public string LocKey { get; set; }

	public string Model { get; set; }

	public int[] HealthTable { get; set; }

	public int? HealthEnhancementsNullable { get; set; }

	public int? MoveNullable { get; set; }

	public int? MoveEnhancementsNullable { get; set; }

	public int? AttackNullable { get; set; }

	public int? AttackEnhancementsNullable { get; set; }

	public int? RangeNullable { get; set; }

	public int? RangeEnhancementsNullable { get; set; }

	public int? ShieldNullable { get; set; }

	public int? RetaliateNullable { get; set; }

	public int? RetaliateRangeNullable { get; set; }

	public int? PierceNullable { get; set; }

	public bool? FlyingNullable { get; set; }

	public bool? InvulnerableNullable { get; private set; }

	public bool? PierceInvulnerabilityNullable { get; private set; }

	public bool? UntargetableNullable { get; private set; }

	public bool? DoesNotBlockNullable { get; private set; }

	public bool? IgnoreActorCollisionNullable { get; private set; }

	public bool? TreatAsTrapNullable { get; private set; }

	public bool? PlayerControlledNullable { get; private set; }

	public ElementInfusionBoardManager.EElement? AttackInfuse { get; set; }

	public string AttackAnimOverload { get; set; }

	public string AttackOnDeathAnimOverload { get; set; }

	public int? AdjacentAttackOnDeathNullable { get; set; }

	public List<CCondition.ENegativeCondition> OnAttackConditions { get; set; }

	public List<CAbility> OnSummonAbilities { get; set; }

	public List<CAbility> OnDeathAbilities { get; set; }

	public List<CAbility> SpecialAbilities { get; set; }

	public CAbilityOverride MoveOverride { get; set; }

	public CAbilityOverride AttackOverride { get; set; }

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; set; }

	public bool NullAttackAnimOverload { get; set; }

	public bool NullAttackOnDeathAnimOverload { get; set; }

	public bool NullAdjacentAttackOnDeathNullable { get; set; }

	public bool NullOnAttackConditions { get; set; }

	public bool NullOnSummonAbilities { get; set; }

	public bool NullOnDeathAbilities { get; set; }

	public bool NullSpecialAbilities { get; set; }

	public bool NullMoveOverride { get; set; }

	public bool NullAttackOverride { get; set; }

	public bool NullStatIsBasedOnXEntries { get; set; }

	public string ColourHTML { get; set; }

	public float Fatness { get; set; }

	public float VertexAnimIntensity { get; set; }

	public string CustomConfig { get; set; }

	public List<AbilityData.StatIsBasedOnXData> AttackStatIsBasedOnXEntries => StatIsBasedOnXEntries?.Where((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Attack || x.BaseStatType == EMonsterBaseStats.Range || x.BaseStatType == EMonsterBaseStats.Target).ToList();

	public List<AbilityData.StatIsBasedOnXData> MoveStatIsBasedOnXEntries => StatIsBasedOnXEntries?.Where((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Move).ToList();

	public int HealthEnhancements
	{
		get
		{
			return (HealthEnhancementsNullable.HasValue ? HealthEnhancementsNullable.Value : 0) + HPEnhancementBonus;
		}
		set
		{
			HealthEnhancementsNullable = value;
		}
	}

	public int Move
	{
		get
		{
			return (MoveNullable.HasValue ? MoveNullable.Value : 0) + MoveEnhancementBonus;
		}
		set
		{
			MoveNullable = value;
		}
	}

	public int MoveEnhancements
	{
		get
		{
			if (!MoveEnhancementsNullable.HasValue)
			{
				return 0;
			}
			return MoveEnhancementsNullable.Value;
		}
		set
		{
			MoveEnhancementsNullable = value;
		}
	}

	public int Attack
	{
		get
		{
			return (AttackNullable.HasValue ? AttackNullable.Value : 0) + AttackEnhancementBonus;
		}
		set
		{
			AttackNullable = value;
		}
	}

	public int AttackEnhancements
	{
		get
		{
			if (!AttackEnhancementsNullable.HasValue)
			{
				return 0;
			}
			return AttackEnhancementsNullable.Value;
		}
		set
		{
			AttackEnhancementsNullable = value;
		}
	}

	public int Range
	{
		get
		{
			return (RangeNullable.HasValue ? RangeNullable.Value : 0) + RangeEnhancementBonus;
		}
		set
		{
			RangeNullable = value;
		}
	}

	public int RangeEnhancements
	{
		get
		{
			if (!RangeEnhancementsNullable.HasValue)
			{
				return 0;
			}
			return RangeEnhancementsNullable.Value;
		}
		set
		{
			RangeEnhancementsNullable = value;
		}
	}

	public int Shield
	{
		get
		{
			if (!ShieldNullable.HasValue)
			{
				return 0;
			}
			return ShieldNullable.Value;
		}
		set
		{
			ShieldNullable = value;
		}
	}

	public int Retaliate
	{
		get
		{
			if (!RetaliateNullable.HasValue)
			{
				return 0;
			}
			return RetaliateNullable.Value;
		}
		set
		{
			RetaliateNullable = value;
		}
	}

	public int RetaliateRange
	{
		get
		{
			if (!RetaliateRangeNullable.HasValue)
			{
				return 1;
			}
			return RetaliateRangeNullable.Value;
		}
		set
		{
			RetaliateRangeNullable = value;
		}
	}

	public int Pierce
	{
		get
		{
			if (!PierceNullable.HasValue)
			{
				return 0;
			}
			return PierceNullable.Value;
		}
		set
		{
			PierceNullable = value;
		}
	}

	public bool Flying
	{
		get
		{
			if (!FlyingNullable.HasValue)
			{
				return false;
			}
			return FlyingNullable.Value;
		}
		set
		{
			FlyingNullable = value;
		}
	}

	public bool Invulnerable
	{
		get
		{
			if (!InvulnerableNullable.HasValue)
			{
				return false;
			}
			return InvulnerableNullable.Value;
		}
		set
		{
			InvulnerableNullable = value;
		}
	}

	public bool PierceInvulnerability
	{
		get
		{
			if (!PierceInvulnerabilityNullable.HasValue)
			{
				return false;
			}
			return PierceInvulnerabilityNullable.Value;
		}
		set
		{
			PierceInvulnerabilityNullable = value;
		}
	}

	public bool Untargetable
	{
		get
		{
			if (!UntargetableNullable.HasValue)
			{
				return false;
			}
			return UntargetableNullable.Value;
		}
		set
		{
			UntargetableNullable = value;
		}
	}

	public bool DoesNotBlock
	{
		get
		{
			if (!DoesNotBlockNullable.HasValue)
			{
				return false;
			}
			return DoesNotBlockNullable.Value;
		}
		set
		{
			DoesNotBlockNullable = value;
		}
	}

	public bool PlayerControlled
	{
		get
		{
			if (!PlayerControlledNullable.HasValue)
			{
				return false;
			}
			return PlayerControlledNullable.Value;
		}
		set
		{
			PlayerControlledNullable = value;
		}
	}

	public bool IgnoreActorCollision
	{
		get
		{
			if (!IgnoreActorCollisionNullable.HasValue)
			{
				return false;
			}
			return IgnoreActorCollisionNullable.Value;
		}
		set
		{
			IgnoreActorCollisionNullable = value;
		}
	}

	public bool TreatAsTrap
	{
		get
		{
			if (!TreatAsTrapNullable.HasValue)
			{
				return false;
			}
			return TreatAsTrapNullable.Value;
		}
		set
		{
			TreatAsTrapNullable = value;
		}
	}

	public int AdjacentAttackOnDeath
	{
		get
		{
			if (!AdjacentAttackOnDeathNullable.HasValue)
			{
				return 0;
			}
			return AdjacentAttackOnDeathNullable.Value;
		}
		set
		{
			AdjacentAttackOnDeathNullable = value;
		}
	}

	public int AttackEnhancementBonus { get; set; }

	public int HPEnhancementBonus { get; set; }

	public int MoveEnhancementBonus { get; set; }

	public int RangeEnhancementBonus { get; set; }

	public string FileName { get; private set; }

	public HeroSummonYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		LocKey = null;
		Model = null;
		HealthTable = null;
		HealthEnhancementsNullable = null;
		MoveNullable = null;
		MoveEnhancementsNullable = null;
		AttackNullable = null;
		AttackEnhancementsNullable = null;
		RangeNullable = null;
		RangeEnhancementsNullable = null;
		ShieldNullable = null;
		RetaliateNullable = null;
		RetaliateRangeNullable = null;
		PierceNullable = null;
		FlyingNullable = null;
		AttackInfuse = null;
		AttackAnimOverload = string.Empty;
		AttackOnDeathAnimOverload = string.Empty;
		AdjacentAttackOnDeathNullable = null;
		StatIsBasedOnXEntries = null;
		OnAttackConditions = null;
		OnSummonAbilities = null;
		OnDeathAbilities = null;
		SpecialAbilities = null;
		MoveOverride = null;
		AttackOverride = null;
		InvulnerableNullable = null;
		PierceInvulnerabilityNullable = null;
		UntargetableNullable = null;
		DoesNotBlockNullable = null;
		PlayerControlledNullable = null;
		IgnoreActorCollisionNullable = null;
		TreatAsTrapNullable = null;
		AttackEnhancementBonus = 0;
		HPEnhancementBonus = 0;
		MoveEnhancementBonus = 0;
		RangeEnhancementBonus = 0;
		NullAttackAnimOverload = false;
		NullAttackOnDeathAnimOverload = false;
		NullAdjacentAttackOnDeathNullable = false;
		NullOnAttackConditions = false;
		NullOnSummonAbilities = false;
		NullOnDeathAbilities = false;
		NullSpecialAbilities = false;
		NullMoveOverride = false;
		NullAttackOverride = false;
		NullStatIsBasedOnXEntries = false;
		ColourHTML = "#FFFFFF";
		Fatness = 0f;
		VertexAnimIntensity = 0f;
		CustomConfig = null;
	}

	public HeroSummonYMLData Copy()
	{
		return new HeroSummonYMLData(FileName)
		{
			ID = ID,
			LocKey = LocKey,
			Model = Model,
			HealthTable = HealthTable,
			HealthEnhancements = HealthEnhancements,
			Move = Move,
			MoveEnhancements = MoveEnhancements,
			Attack = Attack,
			AttackEnhancements = AttackEnhancements,
			Range = Range,
			RangeEnhancements = RangeEnhancements,
			Shield = Shield,
			Retaliate = Retaliate,
			RetaliateRange = RetaliateRange,
			Pierce = Pierce,
			Flying = Flying,
			AttackInfuse = AttackInfuse,
			AttackAnimOverload = (AttackAnimOverload ?? string.Empty),
			AttackOnDeathAnimOverload = (AttackOnDeathAnimOverload ?? string.Empty),
			AdjacentAttackOnDeath = AdjacentAttackOnDeath,
			StatIsBasedOnXEntries = StatIsBasedOnXEntries?.Select((AbilityData.StatIsBasedOnXData x) => x.Copy()).ToList(),
			OnAttackConditions = OnAttackConditions,
			OnSummonAbilities = ((OnSummonAbilities != null) ? OnSummonAbilities.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList() : null),
			OnDeathAbilities = ((OnDeathAbilities != null) ? OnDeathAbilities.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList() : null),
			SpecialAbilities = ((SpecialAbilities != null) ? SpecialAbilities.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList() : null),
			MoveOverride = MoveOverride,
			AttackOverride = AttackOverride,
			AttackEnhancementBonus = AttackEnhancementBonus,
			HPEnhancementBonus = HPEnhancementBonus,
			MoveEnhancementBonus = MoveEnhancementBonus,
			RangeEnhancementBonus = RangeEnhancementBonus,
			Fatness = Fatness,
			ColourHTML = ColourHTML,
			VertexAnimIntensity = VertexAnimIntensity,
			CustomConfig = CustomConfig,
			Invulnerable = Invulnerable,
			PierceInvulnerability = PierceInvulnerability,
			Untargetable = Untargetable,
			DoesNotBlock = DoesNotBlock,
			PlayerControlled = PlayerControlled,
			IgnoreActorCollision = IgnoreActorCollision
		};
	}

	public bool Validate()
	{
		if (LocKey == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "HeroSummon is missing LocKey value. " + FileName);
			return false;
		}
		if (Model == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "HeroSummon is missing Model value. " + FileName);
			return false;
		}
		if (HealthTable == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "HeroSummon is missing HealthTable value. " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(HeroSummonYMLData summonData)
	{
		if (summonData.LocKey != null)
		{
			LocKey = summonData.LocKey;
		}
		if (summonData.Model != null)
		{
			Model = summonData.Model;
		}
		if (summonData.HealthTable != null)
		{
			HealthTable = summonData.HealthTable;
		}
		if (summonData.HealthEnhancementsNullable.HasValue)
		{
			HealthEnhancementsNullable = summonData.HealthEnhancementsNullable;
		}
		if (summonData.MoveNullable.HasValue)
		{
			MoveNullable = summonData.MoveNullable;
		}
		if (summonData.MoveEnhancementsNullable.HasValue)
		{
			MoveEnhancementsNullable = summonData.MoveEnhancementsNullable;
		}
		if (summonData.AttackNullable.HasValue)
		{
			AttackNullable = summonData.AttackNullable;
		}
		if (summonData.AttackEnhancementsNullable.HasValue)
		{
			AttackEnhancementsNullable = summonData.AttackEnhancementsNullable;
		}
		if (summonData.RangeNullable.HasValue)
		{
			RangeNullable = summonData.RangeNullable;
		}
		if (summonData.RangeEnhancementsNullable.HasValue)
		{
			RangeEnhancementsNullable = summonData.RangeEnhancementsNullable;
		}
		if (summonData.ShieldNullable.HasValue)
		{
			ShieldNullable = summonData.ShieldNullable;
		}
		if (summonData.RetaliateNullable.HasValue)
		{
			RetaliateNullable = summonData.RetaliateNullable;
		}
		if (summonData.RetaliateRangeNullable.HasValue)
		{
			RetaliateRangeNullable = summonData.RetaliateRangeNullable;
		}
		if (summonData.PierceNullable.HasValue)
		{
			PierceNullable = summonData.PierceNullable;
		}
		if (summonData.FlyingNullable.HasValue)
		{
			FlyingNullable = summonData.FlyingNullable;
		}
		if (summonData.AttackInfuse.HasValue)
		{
			AttackInfuse = summonData.AttackInfuse;
		}
		if (summonData.AttackAnimOverload != null)
		{
			AttackAnimOverload = summonData.AttackAnimOverload;
		}
		if (summonData.AttackOnDeathAnimOverload != null)
		{
			AttackOnDeathAnimOverload = summonData.AttackOnDeathAnimOverload;
		}
		if (summonData.AdjacentAttackOnDeathNullable.HasValue)
		{
			AdjacentAttackOnDeathNullable = summonData.AdjacentAttackOnDeath;
		}
		if (summonData.StatIsBasedOnXEntries != null)
		{
			StatIsBasedOnXEntries = summonData.StatIsBasedOnXEntries;
		}
		if (summonData.OnAttackConditions != null)
		{
			OnAttackConditions = summonData.OnAttackConditions;
		}
		if (summonData.OnSummonAbilities != null)
		{
			OnSummonAbilities = summonData.OnSummonAbilities;
		}
		if (summonData.OnDeathAbilities != null)
		{
			OnDeathAbilities = summonData.OnDeathAbilities;
		}
		if (summonData.SpecialAbilities != null)
		{
			SpecialAbilities = summonData.SpecialAbilities;
		}
		if (summonData.MoveOverride != null)
		{
			MoveOverride = summonData.MoveOverride;
		}
		if (summonData.AttackOverride != null)
		{
			AttackOverride = summonData.AttackOverride;
		}
		if (summonData.InvulnerableNullable.HasValue)
		{
			InvulnerableNullable = summonData.InvulnerableNullable;
		}
		if (summonData.DoesNotBlockNullable.HasValue)
		{
			DoesNotBlockNullable = summonData.DoesNotBlockNullable;
		}
		if (summonData.PierceInvulnerabilityNullable.HasValue)
		{
			PierceInvulnerabilityNullable = summonData.PierceInvulnerabilityNullable;
		}
		if (summonData.UntargetableNullable.HasValue)
		{
			UntargetableNullable = summonData.UntargetableNullable;
		}
		if (summonData.PlayerControlledNullable.HasValue)
		{
			PlayerControlledNullable = summonData.PlayerControlledNullable;
		}
		if (summonData.IgnoreActorCollisionNullable.HasValue)
		{
			IgnoreActorCollisionNullable = summonData.IgnoreActorCollisionNullable;
		}
		if (summonData.TreatAsTrapNullable.HasValue)
		{
			TreatAsTrapNullable = summonData.TreatAsTrapNullable;
		}
		if (NullAttackAnimOverload)
		{
			AttackAnimOverload = string.Empty;
		}
		if (NullAttackOnDeathAnimOverload)
		{
			AttackOnDeathAnimOverload = string.Empty;
		}
		if (NullAdjacentAttackOnDeathNullable)
		{
			AdjacentAttackOnDeathNullable = null;
		}
		if (NullOnAttackConditions)
		{
			OnAttackConditions = null;
		}
		if (NullOnSummonAbilities)
		{
			OnSummonAbilities = null;
		}
		if (NullOnDeathAbilities)
		{
			OnDeathAbilities = null;
		}
		if (NullSpecialAbilities)
		{
			SpecialAbilities = null;
		}
		if (NullMoveOverride)
		{
			MoveOverride = null;
		}
		if (NullAttackOverride)
		{
			AttackOverride = null;
		}
		if (NullStatIsBasedOnXEntries)
		{
			StatIsBasedOnXEntries = null;
		}
		if (summonData.ColourHTML != "#FFFFFF")
		{
			ColourHTML = summonData.ColourHTML;
		}
		if (summonData.Fatness != 0f)
		{
			Fatness = summonData.Fatness;
		}
		if (summonData.VertexAnimIntensity != 0f)
		{
			VertexAnimIntensity = summonData.VertexAnimIntensity;
		}
		if (summonData.CustomConfig != null)
		{
			CustomConfig = summonData.CustomConfig;
		}
	}

	public void ResetEnhancementBonuses()
	{
		AttackEnhancementBonus = 0;
		HPEnhancementBonus = 0;
		MoveEnhancementBonus = 0;
		RangeEnhancementBonus = 0;
	}

	public int GetHealth(int level)
	{
		return HealthTable[level] + HPEnhancementBonus;
	}
}
