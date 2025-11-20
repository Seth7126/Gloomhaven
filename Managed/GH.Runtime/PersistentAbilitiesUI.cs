using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class PersistentAbilitiesUI : MonoBehaviour
{
	[SerializeField]
	private GameObject persistentAbilityPrefab;

	[SerializeField]
	private Transform abilitiesHolder;

	[SerializeField]
	private Transform abilitiesHolderExtended;

	[SerializeField]
	private Image actorBackgroundImage;

	[SerializeField]
	private bool showBackroundImage;

	[SerializeField]
	private Transform tooltipHolder;

	[SerializeField]
	private GameObject immunityPrefab;

	[SerializeField]
	private GameObject monsterStatPrefab;

	[SerializeField]
	private GameObject passivePerkPrefab;

	[SerializeField]
	private PassivePerkPreviewUI companionPreview;

	private CActor actor;

	private List<PersistentAbilityPreviewUI> persistentAbilitiesPreviewsList = new List<PersistentAbilityPreviewUI>();

	private bool isInteractable;

	private List<ImmunityPreviewUI> immunitiesPreviewsList = new List<ImmunityPreviewUI>();

	private List<StatusEffectPreviewUI> monsterStatsPreviewsList = new List<StatusEffectPreviewUI>();

	private List<PassivePerkPreviewUI> passivesPreviewsList = new List<PassivePerkPreviewUI>();

	private const int MAX_ABILITIES_HOLDER = 4;

	private Tuple<Selectable, Selectable> navigation;

	private List<Selectable> bonuses = new List<Selectable>();

	public bool IsInteractable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			bool flag = value && !(actor is CEnemyActor);
			if (isInteractable != flag)
			{
				isInteractable = flag;
				RefreshInteractable(IsInteractable);
			}
		}
	}

	private void Start()
	{
		if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			SubscribeToControllableChanges();
		}
		else
		{
			SubscribeToMyPlayerInitialized();
		}
	}

	private void OnDestroy()
	{
		if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			UnsubscribeFromControllableChanges();
		}
		else
		{
			UnsubscribeFromMyPlayerInitialized();
		}
	}

	public void Init(CActor actor, bool interactable)
	{
		this.actor = actor;
		isInteractable = interactable && !(actor is CEnemyActor);
		if (actor is CPlayerActor && actorBackgroundImage != null)
		{
			actorBackgroundImage.sprite = UIInfoTools.Instance.GetActiveAbilityIconBackground(actor);
		}
		RefreshAbilities();
	}

	private List<CActiveBonus> FilterActiveBonus(CActor actor)
	{
		return (from it in GetActorBonuses(actor)
			where it.Layout != null || actor.IsMonsterType || it.IsAugment || it.IsSong || it.IsDoom
			select it).ToList();
	}

	private List<CActiveBonus> GetActorBonuses(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (actor is CPlayerActor cPlayerActor)
		{
			list.AddRange(cPlayerActor.CharacterClass.FindCasterActiveBonuses(cPlayerActor));
		}
		else if (actor is CEnemyActor cEnemyActor)
		{
			list.AddRange(MonsterClassManager.FindAllActiveBonuses(cEnemyActor));
		}
		return list;
	}

	private CHeroSummonActor GetCompanion(CActor actor)
	{
		return ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor it) => it.Summoner.ActorGuid == actor.ActorGuid && it.IsCompanionSummon);
	}

	public void RefreshAbilities()
	{
		if (actor == null)
		{
			return;
		}
		List<CActiveBonus> list = (from x in GetActorBonuses(actor)
			where x.Layout == null
			select x).ToList();
		for (int num = 0; num < list.Count(); num++)
		{
			UIActiveAbility.CheckForLayoutToGenerate(list[num]);
		}
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		list2.AddRange(FilterActiveBonus(actor));
		List<List<CActiveBonus>> list3 = new List<List<CActiveBonus>>();
		foreach (IGrouping<string, CActiveBonus> item in from x in list2
			where !x.Ability.ActiveBonusData.Hidden
			group x by x.BaseCard.Name)
		{
			if (item.Any((CActiveBonus x) => x.IsDoom))
			{
				list3.Add(new List<CActiveBonus> { item.First((CActiveBonus x) => x.IsDoom) });
				continue;
			}
			foreach (IGrouping<Guid, CActiveBonus> item2 in from x in item
				group x by x.Ability.ID)
			{
				list3.Add(item2.ToList());
			}
		}
		int num2 = list3.Count;
		bonuses.Clear();
		HelperTools.NormalizePool(ref persistentAbilitiesPreviewsList, persistentAbilityPrefab, abilitiesHolder, num2);
		int num3 = 0;
		foreach (List<CActiveBonus> activeBonusGroupEntry in list3)
		{
			int instances = ((activeBonusGroupEntry[0].Type() == CAbility.EAbilityType.Summon || activeBonusGroupEntry[0].IsDoom) ? 1 : activeBonusGroupEntry.Count);
			PersistentAbilityPreviewUI slot = persistentAbilitiesPreviewsList[num3];
			slot.SetAbilityBonus(activeBonusGroupEntry.FirstOrDefault(), actor, tooltipHolder, delegate
			{
				OnCancelActiveAbility(activeBonusGroupEntry, actor, delegate
				{
					OnFinishedCancel(slot);
				});
			}, instances);
			slot.descriptionPopup.activeAbility.RefreshTracker();
			slot.SetInteractable(isInteractable);
			slot.transform.SetParent((num3 >= 4) ? abilitiesHolderExtended : abilitiesHolder);
			bonuses.Add(slot.Selectable);
			num3++;
		}
		List<KeyValuePair<string, string>> list4 = GetMonsterOrSummonBaseStats(actor).ToList();
		HelperTools.NormalizePool(ref monsterStatsPreviewsList, monsterStatPrefab, abilitiesHolder, list4.Count);
		for (int num4 = 0; num4 < list4.Count; num4++)
		{
			monsterStatsPreviewsList[num4].SetEffect("$Innate$", CreateLayout.LocaliseText(list4[num4].Key), UIInfoTools.Instance.GetActiveAbilityIcon(list4[num4].Value), tooltipHolder);
			monsterStatsPreviewsList[num4].transform.SetParent((num3 >= 4) ? abilitiesHolderExtended : abilitiesHolder);
			bonuses.Add(monsterStatsPreviewsList[num4].GetComponentInChildren<Selectable>());
			num3++;
		}
		if (actor is CEnemyActor)
		{
			List<CAbility.EAbilityType> list5 = ((CEnemyActor)actor).MonsterClass.Immunities;
			bool flag = CActor.ImmuneToAllConditionsCheck(list5);
			if (flag)
			{
				list5 = list5.Except(CActor.ImmunityConditionAbilityTypes).ToList();
			}
			if (list5.Contains(CAbility.EAbilityType.ControlActor))
			{
				list5.Remove(CAbility.EAbilityType.ControlActor);
			}
			HelperTools.NormalizePool(ref immunitiesPreviewsList, immunityPrefab, abilitiesHolder, list5.Count + (flag ? 1 : 0));
			if (list5.Count > 0)
			{
				for (int num5 = 0; num5 < list5.Count; num5++)
				{
					immunitiesPreviewsList[num5].SetImmunity(list5[num5].ToString(), tooltipHolder);
					immunitiesPreviewsList[num5].transform.SetParent((num3 >= 4) ? abilitiesHolderExtended : abilitiesHolder);
					num3++;
					bonuses.Add(immunitiesPreviewsList[num5].GetComponentInChildren<Selectable>());
				}
				if (flag)
				{
					immunitiesPreviewsList[immunitiesPreviewsList.Count - 1].SetImmunity("AllConditions", tooltipHolder);
					immunitiesPreviewsList[immunitiesPreviewsList.Count - 1].transform.SetParent((num3 >= 4) ? abilitiesHolderExtended : abilitiesHolder);
					num2++;
					bonuses.Add(immunitiesPreviewsList[immunitiesPreviewsList.Count - 1].GetComponentInChildren<Selectable>());
				}
			}
			num2 += list5.Count + list4.Count;
			if (actorBackgroundImage != null && showBackroundImage)
			{
				actorBackgroundImage.enabled = num2 > list4.Count;
			}
			goto IL_08ab;
		}
		List<Tuple<ReferenceToSprite, string, string, Action, bool>> customEffects = GetCustomEffects(actor);
		List<CharacterPerk> list6 = new List<CharacterPerk>();
		if (AdventureState.MapState != null)
		{
			CMapScenarioState currentMapScenarioState = AdventureState.MapState.CurrentMapScenarioState;
			if (currentMapScenarioState != null && currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				CMapScenarioState currentMapScenarioState2 = AdventureState.MapState.CurrentMapScenarioState;
				if (currentMapScenarioState2 == null || currentMapScenarioState2.CustomLevelData.PartySpawnType != ELevelPartyChoiceType.LoadAdventureParty)
				{
					goto IL_0682;
				}
			}
			CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter it) => it.CharacterID == actor.Class.ID);
			if (cMapCharacter != null)
			{
				list6 = cMapCharacter.GetPassivePerks();
			}
		}
		goto IL_0682;
		IL_08ab:
		base.gameObject.SetActive(num2 > 0);
		abilitiesHolderExtended.gameObject.SetActive(num2 > 4);
		RefreshNavigation();
		return;
		IL_0682:
		HelperTools.NormalizePool(ref passivesPreviewsList, passivePerkPrefab, abilitiesHolder, list6.Count + customEffects.Count);
		for (int num6 = 0; num6 < list6.Count; num6++)
		{
			passivesPreviewsList[num6].SetPassive(list6[num6].Perk.IgnoreNegativeItemEffects ? new ReferenceToSprite(UIInfoTools.Instance.IgnoreNegativeItemEffectsIcon) : new ReferenceToSprite(UIInfoTools.Instance.IgnoreNegativeScenarioEffectsIcon), list6[num6].Perk.Name, list6[num6].Perk.Description, tooltipHolder);
			passivesPreviewsList[num6].transform.SetParent((num3 >= 4) ? abilitiesHolderExtended : abilitiesHolder);
			passivesPreviewsList[num6].SetInteractable(interactable: false);
			bonuses.Add(passivesPreviewsList[num6].Selectable);
			num3++;
			num2++;
		}
		for (int num7 = 0; num7 < customEffects.Count; num7++)
		{
			PassivePerkPreviewUI passivePerkPreviewUI = passivesPreviewsList[num7 + list6.Count];
			passivePerkPreviewUI.SetPassive(customEffects[num7].Item1, customEffects[num7].Item2, customEffects[num7].Item3, tooltipHolder, customEffects[num7].Item4, localizeKeys: true, customEffects[num7].Item5);
			passivePerkPreviewUI.SetInteractable(isInteractable && (!FFSNetwork.IsOnline || actor.IsUnderMyControl));
			bonuses.Add(passivePerkPreviewUI.Selectable);
			passivePerkPreviewUI.transform.SetParent((num3 >= 4) ? abilitiesHolderExtended : abilitiesHolder);
			num3++;
			num2++;
		}
		if (actorBackgroundImage != null && showBackroundImage)
		{
			actorBackgroundImage.enabled = true;
		}
		RefreshCompanion();
		goto IL_08ab;
	}

	private void OnFinishedCancel(PersistentAbilityPreviewUI slot)
	{
		if (InputManager.GamePadInUse && ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.InitiativeTrack))
		{
			if (bonuses.Contains(slot.Selectable))
			{
				EventSystem.current.SetSelectedGameObject(slot.Selectable.gameObject);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(slot.Selectable.navigation.selectOnLeft?.gameObject);
			}
		}
	}

	public void RefreshCompanion()
	{
		CHeroSummonActor companion = GetCompanion(actor);
		if (companion == null)
		{
			companionPreview.gameObject.SetActive(value: false);
			return;
		}
		HeroSummonYMLData summonData = companion.SummonData;
		PreviewEffectInfo previewEffectConfig = UIInfoTools.Instance.GetPreviewEffectConfig(summonData.ID);
		ReferenceToSprite referenceToSprite = null;
		referenceToSprite = ((!(previewEffectConfig?.previewEffectIcon != null)) ? UIInfoTools.Instance.GetCharacterSpriteRef(((CPlayerActor)actor).CharacterClass.CharacterModel, highlight: false, ((CPlayerActor)actor).CharacterClass.CharacterYML.CustomCharacterConfig) : new ReferenceToSprite(previewEffectConfig?.previewEffectIcon));
		string descriptionKey = string.Format((previewEffectConfig != null && previewEffectConfig.previewEffectText != null) ? LocalizationManager.GetTranslation(previewEffectConfig.previewEffectText) : LocalizationManager.GetTranslation(summonData.Model + "_TOOLTIP"), companion.MaxHealth, summonData.Move, summonData.Attack, (summonData.Range < 2) ? "-" : summonData.Range.ToString());
		companionPreview.gameObject.SetActive(value: true);
		companionPreview.SetPassive(referenceToSprite, LocalizationManager.GetTranslation(summonData.LocKey), descriptionKey, null, null, localizeKeys: false);
	}

	private List<Tuple<ReferenceToSprite, string, string, Action, bool>> GetCustomEffects(CActor actor)
	{
		List<Tuple<ReferenceToSprite, string, string, Action, bool>> list = new List<Tuple<ReferenceToSprite, string, string, Action, bool>>();
		CPlayerActor playerActor = actor as CPlayerActor;
		if (playerActor != null)
		{
			if (playerActor.CharacterClass.ImprovedShortRest && PhaseManager.CurrentPhase.Type != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				list.Add(new Tuple<ReferenceToSprite, string, string, Action, bool>(UIInfoTools.Instance.GetCharacterSpriteRef(playerActor.CharacterClass.CharacterModel, highlight: false, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig), "GUI_IMPROVED_SHORT_REST_TITLE_TOOLTIP", "GUI_IMPROVED_SHORT_REST_ACTIVE_BONUS_TOOLTIP", delegate
				{
					Singleton<UIConfirmationBoxManager>.Instance.ShowCancelActiveAbility(LocalizationManager.GetTranslation("GUI_IMPROVED_SHORT_REST_TITLE_TOOLTIP"), lost: false, delegate
					{
						CardsHandManager.Instance.CancelImprovedShortRest((CPlayerActor)actor);
						if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
						{
							int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? playerActor.CharacterName.GetHashCode() : playerActor.CharacterClass.ModelInstanceID);
							Synchronizer.ReplicateControllableStateChange(GameActionType.CancelImprovedShortRest, ActionProcessor.CurrentPhase, controllableID);
						}
					});
				}, item5: true));
			}
			for (int num = 0; num < playerActor.CarriedQuestItems.Count; num++)
			{
				CObjectProp cObjectProp = playerActor.CarriedQuestItems[num];
				if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
				{
					list.Add(new Tuple<ReferenceToSprite, string, string, Action, bool>(new ReferenceToSprite(UIInfoTools.Instance.CarryableQuestItemIconSprite), cObjectProp.PrefabName + "_TOOLTIP", cObjectProp.PrefabName + "_DESCR_TOOLTIP", null, item5: false));
				}
			}
		}
		return list;
	}

	private void OnCancelActiveAbility(List<CActiveBonus> bonuses, CActor actor, UnityAction onFinished)
	{
		CActiveBonus cActiveBonus = bonuses.FirstOrDefault((CActiveBonus it) => it.Ability.ActiveBonusData.CannotCancel);
		if (cActiveBonus != null)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation(cActiveBonus.Ability.AbilityBaseCard.Name), LocalizationManager.GetTranslation("GUI_CANNOT_CANCEL_ACTIVE_BONUS_BEFORE_END_ROUND"), "GUI_CONTINUE", onFinished);
			return;
		}
		for (int num = 0; num < bonuses.Count; num++)
		{
			OnCancelActiveAbility(bonuses[num], actor, (num == bonuses.Count - 1) ? onFinished : null);
		}
	}

	private void OnCancelActiveAbility(CActiveBonus bonus, CActor actor, UnityAction onFinished)
	{
		if (bonus.Ability.ActiveBonusData.CannotCancel)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation(bonus.Ability.AbilityBaseCard.Name), LocalizationManager.GetTranslation("GUI_CANNOT_CANCEL_ACTIVE_BONUS_BEFORE_END_ROUND"), "GUI_CONTINUE", onFinished);
		}
		else if (bonus.BaseCard is CItem cItem)
		{
			if (cItem.YMLData.Trigger == CItem.EItemTrigger.PassiveEffect)
			{
				return;
			}
			Singleton<UIConfirmationBoxManager>.Instance.ShowCancelActiveAbility(LocalizationManager.GetTranslation(bonus.BaseCard.Name), cItem.YMLData.Usage == CItem.EUsageType.Consumed, delegate
			{
				CClass.CancelActiveBonus(bonus);
				RefreshAbilities();
				if (actor is CPlayerActor playerActor)
				{
					CardsHandManager.Instance.NetworkCancelingActiveBonuses(playerActor, bonus.BaseCard);
				}
				else
				{
					FFSNet.Console.Log("Trying to cancel an item based active ability on a non-playerActor.");
				}
				onFinished?.Invoke();
			}, null, onFinished);
		}
		else if (bonus.BaseCard != null && bonus.BaseCard is CAbilityCard card)
		{
			CardsHandManager.Instance.DiscardActiveAbilityCard((CPlayerActor)actor, card, onFinished, onFinished);
		}
	}

	public void OnActiveBonusTriggered(CActiveBonus activeBonus)
	{
		persistentAbilitiesPreviewsList.SingleOrDefault((PersistentAbilityPreviewUI s) => s.gameObject.activeSelf && s.ActiveBonus == activeBonus)?.OnActiveBonusTriggered();
	}

	public void RefreshInteractable(bool isInteractable)
	{
		foreach (PersistentAbilityPreviewUI persistentAbilitiesPreviews in persistentAbilitiesPreviewsList)
		{
			persistentAbilitiesPreviews.SetInteractable(isInteractable);
		}
		passivesPreviewsList.ForEach(delegate(PassivePerkPreviewUI x)
		{
			x.SetInteractable(isInteractable && (!FFSNetwork.IsOnline || actor.IsUnderMyControl));
		});
	}

	public static Dictionary<string, string> GetMonsterOrSummonBaseStats(CActor actor, List<CCondition.ENegativeCondition> additionalConditions = null)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		if (actor is CEnemyActor cEnemyActor)
		{
			result = BuildMonsterBaseStatsDict(cEnemyActor.MonsterClass, additionalConditions);
		}
		else if (actor is CHeroSummonActor summonActor)
		{
			result = BuildSummonBaseStatsDict(summonActor);
		}
		return result;
	}

	public static Dictionary<string, string> BuildMonsterBaseStatsDict(CMonsterClass monsterClass, List<CCondition.ENegativeCondition> additionalConditions = null)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (monsterClass.Invulnerable)
		{
			dictionary.Add("$ScenarioAbility_Invulnerable$", "Prevent");
		}
		if (monsterClass.Untargetable)
		{
			dictionary.Add("$ScenarioAbility_Untargetable$", "Immune");
		}
		if (monsterClass.PierceInvulnerability)
		{
			dictionary.Add("$ScenarioAbility_PierceInvulnerable$", "Pierce");
		}
		if (monsterClass.Target > 1)
		{
			dictionary.Add("$Target$ <sprite name=Target> " + monsterClass.Target, "Target");
		}
		if (monsterClass.Pierce > 0)
		{
			dictionary.Add("$Pierce$ <sprite name=Pierce> " + monsterClass.Pierce, "Pierce");
		}
		if (monsterClass.Flying)
		{
			dictionary.Add("$Flying$", "Flying");
		}
		if (monsterClass.Advantage)
		{
			dictionary.Add("$Advantage$", "Advantage");
		}
		if (monsterClass.AttackersGainDisadv)
		{
			dictionary.Add("$AttackersGainDisadvantage$", "Disadvantage");
		}
		if (monsterClass.Shield > 0 && (monsterClass.ShieldStatIsBasedOnXEntries == null || !monsterClass.ShieldStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Shield)))
		{
			string text = monsterClass.Shield.ToString();
			string key = "$Shield$ <sprite name=Shield> " + text;
			dictionary.Add(key, "Shield");
		}
		if (monsterClass.Retaliate > 0)
		{
			string text2 = "$Retaliate$ <sprite name=Retaliate> " + monsterClass.Retaliate;
			text2 += ((monsterClass.RetaliateRange > 1) ? (", $Retaliate$ $Range$ <sprite name=Range> " + monsterClass.RetaliateRange) : "");
			dictionary.Add(text2, "Retaliate");
		}
		if (monsterClass.Conditions.Count > 0 || (additionalConditions != null && additionalConditions.Count > 0))
		{
			List<CCondition.ENegativeCondition> list = monsterClass.Conditions.ToList();
			if (additionalConditions != null)
			{
				list.AddRange(additionalConditions);
				list = list.Distinct().ToList();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("$AttacksApply$ ");
			foreach (CCondition.ENegativeCondition item in list)
			{
				stringBuilder.Append("<nobr>$" + item.ToString() + "$ <sprite name=" + item.ToString() + "></nobr>");
				if (list.IndexOf(item) != list.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			dictionary.Add(stringBuilder.ToString(), "Attack");
		}
		if (monsterClass.AttackStatIsBasedOnXEntries != null || monsterClass.ShieldStatIsBasedOnXEntries != null)
		{
			List<AbilityData.StatIsBasedOnXData> list2 = new List<AbilityData.StatIsBasedOnXData>();
			if (monsterClass.AttackStatIsBasedOnXEntries != null)
			{
				list2.AddRange(monsterClass.AttackStatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength));
			}
			if (monsterClass.ShieldStatIsBasedOnXEntries != null)
			{
				list2.AddRange(monsterClass.ShieldStatIsBasedOnXEntries);
			}
			foreach (AbilityData.StatIsBasedOnXData item2 in list2)
			{
				switch (item2.BasedOn)
				{
				case CAbility.EStatIsBasedOnXType.HexesMovedThisTurn:
					dictionary.Add("$Boss_AddHexesMovedToAttackBuffText$", "Attack");
					break;
				case CAbility.EStatIsBasedOnXType.ActorCount:
				case CAbility.EStatIsBasedOnXType.DeadActorCount:
				{
					string text3 = null;
					string className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterEnemyClasses?.FirstOrDefault();
					if (!className.IsNullOrEmpty())
					{
						MonsterYMLData monsterData = ScenarioRuleClient.SRLYML.GetMonsterData(className);
						if (monsterData != null)
						{
							text3 = monsterData.LocKey;
						}
						else
						{
							Debug.LogError("Unable to find monster class " + className);
						}
					}
					else
					{
						className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterPlayerClasses?.FirstOrDefault();
						if (!className.IsNullOrEmpty())
						{
							CharacterYMLData characterYMLData = ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == className);
							if (characterYMLData != null)
							{
								text3 = characterYMLData.LocKey;
							}
							else
							{
								Debug.LogError("Unable to find character class " + className);
							}
						}
						else
						{
							className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterHeroSummonClasses?.FirstOrDefault();
							if (!className.IsNullOrEmpty())
							{
								HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == className);
								if (heroSummonYMLData != null)
								{
									text3 = heroSummonYMLData.LocKey;
								}
								else
								{
									Debug.LogError("Unable to find Hero Summon class " + className);
								}
							}
							else
							{
								className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterObjectClasses?.FirstOrDefault();
								if (className.IsNOTNullOrEmpty())
								{
									MonsterYMLData monsterData2 = ScenarioRuleClient.SRLYML.GetMonsterData(className);
									if (monsterData2 != null)
									{
										text3 = monsterData2.LocKey;
									}
								}
							}
						}
					}
					if (!text3.IsNOTNullOrEmpty())
					{
						break;
					}
					if (item2.BaseStatType == EMonsterBaseStats.Attack)
					{
						string text4 = "$Boss_BaseAttackStrengthIsEqualTo$ ";
						text4 += string.Format(LocalizationManager.GetTranslation("Boss_NumberOfXsPresent"), LocalizationManager.GetTranslation(text3));
						if (item2.Multiplier != 1f)
						{
							text4 = text4 + " $times$ " + item2.Multiplier;
						}
						dictionary.Add(text4, "Attack");
					}
					else if (item2.BaseStatType == EMonsterBaseStats.Shield)
					{
						string key2 = string.Format(LocalizationManager.GetTranslation((item2.BasedOn == CAbility.EStatIsBasedOnXType.DeadActorCount) ? "INNATE_ShieldBasedOnNumberOfDeadXsPresent" : "INNATE_ShieldBasedOnNumberOfXsPresent"), (monsterClass.OriginalBaseStats.Shield > 0) ? monsterClass.OriginalBaseStats.Shield : 0, (item2.Multiplier >= 0f) ? " $plus$ " : " $minus$ ", LocalizationManager.GetTranslation(text3));
						dictionary.Add(key2, "Shield");
					}
					break;
				}
				}
			}
		}
		if (monsterClass.CurrentMonsterStat.OnDeathAbilities != null && monsterClass.CurrentMonsterStat.OnDeathAbilities.Count > 0)
		{
			foreach (CAbility item3 in monsterClass.CurrentMonsterStat.OnDeathAbilities.Where((CAbility x) => x.AbilityText.IsNOTNullOrEmpty()))
			{
				string locText = "$" + item3.AbilityText + "$";
				locText = CreateLayout.LocaliseText(locText);
				locText = string.Format(locText, item3.Strength);
				dictionary.Add(locText, "Kill");
			}
		}
		return dictionary;
	}

	public static Dictionary<string, string> BuildSummonBaseStatsDict(CHeroSummonActor summonActor)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (summonActor.SummonData.Flying)
		{
			dictionary.Add("$Flying$", "Flying");
		}
		if (summonActor.SummonData.Shield > 0)
		{
			dictionary.Add("$Shield$ " + summonActor.SummonData.Shield, "Shield");
		}
		if (summonActor.SummonData.Retaliate > 0)
		{
			dictionary.Add("$Retaliate$ " + summonActor.SummonData.Retaliate + ((summonActor.SummonData.RetaliateRange > 1) ? (", $Retaliate$ $Range$ <sprite name=Range> " + summonActor.SummonData.RetaliateRange) : ""), "Retaliate");
		}
		if (summonActor.SummonData.Pierce > 0)
		{
			dictionary.Add("$Pierce$ " + summonActor.SummonData.Pierce, "Pierce");
		}
		if (summonActor.SummonData.OnSummonAbilities != null)
		{
			foreach (CAbility item in summonActor.SummonData.OnSummonAbilities.Where((CAbility x) => x.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA && x.AbilityText.IsNOTNullOrEmpty()))
			{
				dictionary.Add(item.AbilityText, (item.AbilityType == CAbility.EAbilityType.AddActiveBonus) ? ("AA_" + summonActor.Class.DefaultModel) : item.AbilityType.ToString());
			}
		}
		if (summonActor.SummonData.AttackStatIsBasedOnXEntries != null)
		{
			foreach (AbilityData.StatIsBasedOnXData attackStatIsBasedOnXEntry in summonActor.SummonData.AttackStatIsBasedOnXEntries)
			{
				if (attackStatIsBasedOnXEntry.BasedOn == CAbility.EStatIsBasedOnXType.PercentageOfCurrentHP && attackStatIsBasedOnXEntry.AbilityStatType == CAbility.EAbilityStatType.Strength)
				{
					dictionary.Add(string.Format(LocalizationManager.GetTranslation("INNATE_StrengthIsPercentageOfCurrentHP"), ((int)(attackStatIsBasedOnXEntry.Multiplier * 100f)).ToString()), "Attack");
				}
			}
		}
		return dictionary;
	}

	private void SubscribeToMyPlayerInitialized()
	{
		FFSNet.Console.LogInfo("Active Ability: Subscribing to MyPlayerInitialized.");
		PlayerRegistry.MyPlayerInitialized.AddListener(SubscribeToControllableChanges);
	}

	private void UnsubscribeFromMyPlayerInitialized()
	{
		FFSNet.Console.LogInfo("Active Ability: Unsubscribing from MyPlayerInitialized.");
		PlayerRegistry.MyPlayerInitialized.RemoveListener(SubscribeToControllableChanges);
	}

	private void SubscribeToControllableChanges()
	{
		FFSNet.Console.LogInfo("Active Ability: Subscribing to controllable changes.");
		PlayerRegistry.MyPlayer.MyControllables.CollectionChanged -= OnControllableOwnershipChanged;
		PlayerRegistry.MyPlayer.MyControllables.CollectionChanged += OnControllableOwnershipChanged;
	}

	private void UnsubscribeFromControllableChanges()
	{
		FFSNet.Console.LogInfo("Active Ability: Unsubscribing from controllable changes.");
		PlayerRegistry.MyPlayer.MyControllables.CollectionChanged -= OnControllableOwnershipChanged;
	}

	private void OnControllableOwnershipChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		persistentAbilitiesPreviewsList.ForEach(delegate(PersistentAbilityPreviewUI x)
		{
			x.SetInteractable(isInteractable);
		});
		passivesPreviewsList.ForEach(delegate(PassivePerkPreviewUI x)
		{
			x.SetInteractable(isInteractable && (!FFSNetwork.IsOnline || actor.IsUnderMyControl));
		});
	}

	public void EnableNavigation(Selectable left, Selectable right)
	{
		navigation = new Tuple<Selectable, Selectable>(left, right);
		RefreshNavigation();
	}

	private void RefreshNavigation()
	{
		if (navigation == null)
		{
			for (int i = 0; i < bonuses.Count; i++)
			{
				bonuses[i].DisableNavigation();
			}
		}
		else
		{
			for (int j = 0; j < bonuses.Count; j++)
			{
				bonuses[j].SetNavigation(down: (j == 0) ? bonuses.Last() : bonuses[j - 1], up: bonuses[(j + 1) % bonuses.Count], left: navigation.Item1, right: navigation.Item2);
			}
		}
	}

	public void DisableNavigation()
	{
		if (navigation != null)
		{
			navigation = null;
			RefreshNavigation();
		}
	}

	public Selectable GetFirstBonus()
	{
		return bonuses.LastOrDefault();
	}
}
