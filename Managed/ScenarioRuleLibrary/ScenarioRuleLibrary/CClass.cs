using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CClass
{
	public enum ENPCModel
	{
		None,
		BanditArcher,
		BanditArcherElite,
		LivingBones,
		LivingBonesElite,
		BanditGuard,
		BanditGuardElite,
		Hound,
		HoundElite,
		RendingDrake,
		RendingDrakeElite,
		SpittingDrake,
		SpittingDrakeElite,
		LivingCorpse,
		LivingCorpseElite,
		LivingSpirit,
		LivingSpiritElite,
		InoxGuard,
		InoxGuardElite,
		InoxArcher,
		InoxArcherElite,
		InoxShaman,
		InoxShamanElite,
		VermlingScout,
		VermlingScoutElite,
		CaveBear,
		CaveBearElite,
		Cultist,
		CultistElite,
		Lurker,
		LurkerElite,
		FrostDemon,
		FrostDemonElite,
		FlameDemon,
		FlameDemonElite,
		EarthDemon,
		EarthDemonElite,
		WindDemon,
		WindDemonElite,
		NightDemon,
		NightDemonElite,
		SunDemon,
		SunDemonElite,
		ForestImp,
		ForestImpElite,
		BlackImp,
		BlackImpElite,
		StoneGolem,
		StoneGolemElite,
		Ooze,
		OozeElite,
		AncientArtillery,
		AncientArtilleryElite,
		GiantViper,
		GiantViperElite,
		CityGuard,
		CityGuardElite,
		CityArcher,
		CityArcherElite,
		VermlingShaman,
		VermlingShamanElite,
		SavvasIcestorm,
		SavvasIcestormElite,
		SavvasLavaflow,
		SavvasLavaflowElite,
		HarrowerInfester,
		HarrowerInfesterElite,
		DeepTerror,
		DeepTerrorElite,
		WingedHorrorEgg,
		BanditCommander,
		InoxBodyguard,
		MercilessOverseer,
		ElderDrake,
		TheBetrayer,
		TheColorless,
		SightlessEye,
		DarkRider,
		WingedHorror,
		Jekserah,
		CaptainOfTheGuard,
		PrimeDemon,
		TheGloom,
		UndeadCommander,
		HighCultist,
		BoneRanger,
		BoneRangerElite,
		Zephyr,
		Hail,
		HailCenser,
		Redthorn,
		Captive01,
		Captive02,
		Captive03,
		Captive04,
		Captive05,
		Captive06,
		HungrySoul,
		Fish,
		GiantOoze,
		Villager01,
		Villager02,
		Villager03,
		Villager04,
		Villager05,
		Villager06,
		Villager07,
		Villager08,
		Villager09,
		Villager10,
		Villager11,
		Villager12,
		ArcaneGolem,
		CultistandVictim,
		SiegeCannon,
		Harvester,
		Infiltrator,
		BloatedRegent,
		RitualCorpse,
		RitualCorpseElite,
		SnakeCultist,
		SnakeCultistElite,
		Battlebot,
		BurningAvatar,
		Decoy,
		GiantRat,
		Killbot,
		ManaSphere,
		MysticAlly,
		PlagueRat,
		RatKing,
		RatSwarm,
		Skeleton,
		WarriorSpirit,
		Doppelganger,
		BeastTyrantBear,
		JadeFalcon,
		GreenAdder,
		TatteredWolf,
		RedFalcon,
		SwampLizard,
		Monolith,
		WindTotem,
		SpiritBanner,
		LivingBomb,
		ShadowWolf,
		IronBeast,
		NailSphere,
		VoidEater,
		BlackUnicorn,
		GiantBat,
		LavaGolem,
		RockColossus,
		ThornShooter,
		SlimeSpirit,
		HealingSprite,
		WarHawk,
		BattleBoar,
		ViciousJackal,
		GiantToad,
		SpittingCobra,
		SteelConstruct,
		SummonsAltar,
		CrystalAltar,
		Elementalist3DemonSummonAltar,
		UndeadSummoningAltar,
		PropDummyObject,
		PrimeDemonAltar,
		LurkerKing,
		Captive01Weapon,
		Captive02Weapon,
		Captive03Weapon,
		Captive04Weapon,
		Captive05Weapon,
		Captive06Weapon,
		BlackSludge,
		BlackSludgeElite,
		FilthySludge,
		BloodImp,
		BloodImpElite,
		BloodMonstrosity,
		BloodMonstrosityElite,
		ChaosDemon,
		ChaosDemonElite,
		RatMonstrosity,
		RatMonstrosityElite,
		VermlingRaider,
		VermlingRaiderElite,
		Zealot,
		ZealotElite,
		EntropyDemon,
		Crowd01,
		Crowd02,
		Crowd03,
		Crowd04,
		Crowd05,
		Crowd06,
		Crowd07,
		Crowd08,
		BloodHorror,
		BloodTumor,
		FirstOfTheOrder,
		SandDevil,
		Ward,
		Rikharn,
		EarthLord,
		InoxNecromancer,
		SpiritBear,
		SpiritOfXorn,
		SongOfTheDeep,
		GhostWolf,
		DeepEarth,
		HighFlame,
		WoundedGuard,
		CityGuardLieutenant,
		VermlingDruid
	}

	public static ENPCModel[] NPCModels = (ENPCModel[])Enum.GetValues(typeof(ENPCModel));

	public const int CURSE_LIMIT = 10;

	public const int BLESS_LIMIT = 10;

	private List<CAbilityCard> m_TemporaryCards = new List<CAbilityCard>();

	public string ID { get; private set; }

	public List<string> Models { get; private set; }

	public string DefaultModel => Models[0];

	public string LocKey { get; private set; }

	public List<CAbilityCard> TemporaryCards => m_TemporaryCards;

	public virtual int Health()
	{
		return 0;
	}

	public virtual string Avatar()
	{
		return null;
	}

	public CClass(string id, string model, string locKey)
	{
		ID = id;
		Models = new List<string> { model };
		LocKey = locKey;
	}

	public CClass(string id, List<string> models, string locKey)
	{
		ID = id;
		Models = models;
		LocKey = locKey;
	}

	public static void MarkRoundActiveBonusesFinished(CActor actor)
	{
		List<CActiveBonus> list;
		if (actor.IsOriginalMonsterType)
		{
			list = MonsterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Round, actor);
			list.AddRange(CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Round, actor));
		}
		else
		{
			if (actor.IsOriginalMonsterType)
			{
				return;
			}
			list = CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Round, actor);
		}
		foreach (CActiveBonus activeBonus in list)
		{
			if (activeBonus.BaseCard.Name.Equals("ABILITY_CARD_FrozenMind"))
			{
				activeBonus.Ability.Augment.AttackType = CAbility.EAttackType.Melee;
				activeBonus.Ability.ActiveBonusData.AttackType = CAbility.EAttackType.Melee;
				activeBonus.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
				continue;
			}
			activeBonus.Finish();
			if (activeBonus.Caster.OriginalType == CActor.EType.Player)
			{
				continue;
			}
			CAbility ability = activeBonus.Ability;
			if (ability != null && ability.MiscAbilityData?.UseOriginalActor == true)
			{
				continue;
			}
			activeBonus.BaseCard.ActiveBonuses.Remove(activeBonus);
			if (activeBonus.BaseCard.ActiveBonuses.Count <= 0 && activeBonus.Caster.Class is CMonsterClass cMonsterClass)
			{
				cMonsterClass.ActivatedCards.RemoveAll((CBaseCard x) => x.ID == activeBonus.BaseCard.ID);
			}
		}
	}

	public static void MarkTurnActiveBonusesFinished(CActor actor)
	{
		List<CActiveBonus> list;
		if (actor.IsOriginalMonsterType)
		{
			list = MonsterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Turn, actor);
			list.AddRange(CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Turn, actor));
		}
		else
		{
			if (actor.IsOriginalMonsterType)
			{
				return;
			}
			list = CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Turn, actor);
		}
		foreach (CActiveBonus item in list)
		{
			item.Finish();
			if (actor.OriginalType != CActor.EType.Player)
			{
				item.BaseCard.ActiveBonuses.Remove(item);
			}
		}
	}

	public static void MarkAbilityActiveBonusesFinished(CActor actor)
	{
		List<CActiveBonus> list;
		if (actor.IsOriginalMonsterType)
		{
			list = MonsterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Ability, actor);
		}
		else
		{
			if (actor.IsOriginalMonsterType)
			{
				return;
			}
			list = CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Ability, actor);
		}
		foreach (CActiveBonus item in list)
		{
			CItem cItem = null;
			if (item.BaseCard.CardType == CBaseCard.ECardType.Item)
			{
				cItem = actor.Inventory.GetItem(((CItem)item.BaseCard).YMLData.Slot, ((CItem)item.BaseCard).SlotIndex);
				if (item.HasTracker && item.TrackerIndex == 0)
				{
					if (item.Ability.ActiveBonusData.Consuming.Count > 0)
					{
						foreach (ElementInfusionBoardManager.EElement item2 in item.Ability.ActiveBonusData.Consuming)
						{
							ElementInfusionBoardManager.Consume((item2 == ElementInfusionBoardManager.EElement.Any) ? (item.ToggledElement.HasValue ? item.ToggledElement.Value : item2) : item2, actor);
						}
					}
				}
				else
				{
					cItem = null;
				}
			}
			item.Finish();
			if (actor.OriginalType != CActor.EType.Player)
			{
				item.BaseCard.ActiveBonuses.Remove(item);
			}
			if (cItem != null)
			{
				actor.Inventory.ReactivateItem(cItem, actor);
			}
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public static void CancelAllActiveBonusesOnDeath(CActor actor)
	{
		List<CActiveBonus> list;
		if (actor.IsOriginalMonsterType)
		{
			list = MonsterClassManager.FindAllActiveBonuses(actor);
			list.AddRange(CharacterClassManager.FindAllActiveBonuses(actor));
		}
		else
		{
			if (actor.IsOriginalMonsterType)
			{
				return;
			}
			list = CharacterClassManager.FindAllActiveBonuses(actor);
		}
		foreach (CActiveBonus item in list)
		{
			if (item.IsAura && item.Actor != actor)
			{
				continue;
			}
			if (item.IsDoom)
			{
				CDoom cDoom = null;
				foreach (CDoom doom in item.Caster.Dooms)
				{
					if (doom.DoomAbilities.Contains(item.Ability))
					{
						cDoom = doom;
					}
				}
				if (cDoom != null)
				{
					if (!item.Caster.CachedRemovedOnDeathDooms.Contains(cDoom))
					{
						item.Caster.CachedRemovedOnDeathDooms.Add(cDoom);
					}
					continue;
				}
			}
			item.Finish();
			if (actor is CPlayerActor cPlayerActor)
			{
				cPlayerActor.CharacterClass.CheckForFinishedActiveBonuses(actor);
			}
			else if (actor is CHeroSummonActor cHeroSummonActor)
			{
				cHeroSummonActor.Summoner.CharacterClass.CheckForFinishedActiveBonuses(cHeroSummonActor.Summoner);
			}
			if (item.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
			{
				GameState.KillActor(actor, item.Actor, CActor.ECauseOfDeath.SummonActiveBonusCancelled, out var _);
			}
			item.Actor.RefreshCharacterSpecialMechanicSlots();
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
		if (actor.Dooms != null && actor.Dooms.Count > 0)
		{
			actor.RemoveAllDooms();
		}
	}

	public static void CancelActiveBonus(CActiveBonus activeBonus)
	{
		if (activeBonus == null)
		{
			return;
		}
		if (activeBonus.Ability.Augment != null)
		{
			activeBonus.Actor.RemoveAugment(activeBonus.Ability.Augment);
			return;
		}
		if (activeBonus.Ability.Song != null)
		{
			activeBonus.Actor.RemoveSong(activeBonus.Ability.Song);
			return;
		}
		if (activeBonus.IsDoom)
		{
			activeBonus.Actor = activeBonus.Caster;
			{
				foreach (CDoom doom in activeBonus.Caster.Dooms)
				{
					if (doom.DoomAbilities.Contains(activeBonus.Ability))
					{
						activeBonus.Caster.RemoveDoom(doom);
						break;
					}
				}
				return;
			}
		}
		activeBonus.Finish();
		if (activeBonus.Actor.Type == CActor.EType.Player)
		{
			(activeBonus.Actor.Class as CCharacterClass).CheckForFinishedActiveBonuses(activeBonus.Actor);
		}
		else if (activeBonus.Actor.Type == CActor.EType.HeroSummon)
		{
			(activeBonus.Actor as CHeroSummonActor).Summoner.CharacterClass.CheckForFinishedActiveBonuses((activeBonus.Actor as CHeroSummonActor).Summoner);
		}
		if (activeBonus.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
		{
			GameState.KillActor(activeBonus.Caster, activeBonus.Actor, CActor.ECauseOfDeath.SummonActiveBonusCancelled, out var _);
		}
		activeBonus.Actor.RefreshCharacterSpecialMechanicSlots();
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public void AddAugmentBaseCardToTemporaryCards(CAbilityCard augmentBaseCard)
	{
		m_TemporaryCards.Add(augmentBaseCard);
	}

	public void ClearTemporaryCards()
	{
		m_TemporaryCards.Clear();
	}

	public virtual CBaseCard FindCardWithAbility(CAbility ability, CActor actor)
	{
		return null;
	}

	public virtual CBaseCard FindCard(int id, string name)
	{
		return null;
	}

	public virtual void AddCurseCard(CActor actor, bool canGoOverLimit = false)
	{
	}

	public virtual void RemoveCurses(CActor actor)
	{
	}

	public virtual void AddBlessCard(CActor actor, bool canGoOverLimit = false)
	{
	}

	public virtual void RemoveBlesses(CActor actor)
	{
	}

	public virtual List<AttackModifierYMLData> GetCurseCards()
	{
		return new List<AttackModifierYMLData>();
	}

	public virtual List<AttackModifierYMLData> GetBlessCard()
	{
		return new List<AttackModifierYMLData>();
	}

	public static void AddModifierCards(List<string> cardNames, ref List<AttackModifierYMLData> list)
	{
		foreach (string attackModName in cardNames)
		{
			if (attackModName == CCondition.EPositiveCondition.Bless.ToString())
			{
				list.Add(AttackModifiersYML.CreateBless());
				continue;
			}
			if (attackModName == CCondition.ENegativeCondition.Curse.ToString())
			{
				list.Add(AttackModifiersYML.CreateCurse());
				continue;
			}
			list.Add(ScenarioRuleClient.SRLYML.AttackModifiers.Single((AttackModifierYMLData c) => c.Name == attackModName));
		}
	}
}
