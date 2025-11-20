#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIActiveBonusBar : Singleton<UIActiveBonusBar>
{
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseActiveBonus activeBonusPrefab;

	[SerializeField]
	private UnityEvent OnChangedActiveBonusAvailable;

	private Dictionary<CActiveBonus, UIUseActiveBonus> activeBonusSlots = new Dictionary<CActiveBonus, UIUseActiveBonus>();

	private Stack<UIUseActiveBonus> pool = new Stack<UIUseActiveBonus>();

	private List<CActor> actors;

	private bool isShown;

	private CAbility.EAbilityType m_LastShownType;

	private CActiveBonus.EActiveBonusBehaviourType m_LastShownBehaviourType;

	private bool m_LastAbilityAlreadyStarted;

	private Action<CActiveBonus, CActor> onClickActiveBonus;

	private Func<CActiveBonus, bool> isMandatoryChecker;

	private bool m_LastShownDamageToTake;

	private Transform lastCreatedSlot;

	public bool IsShowing => isShown;

	public List<CActiveBonus> ShowingActiveBonuses => activeBonusSlots.Keys.ToList();

	public List<CActiveBonus> ShowingPreventDamageBonuses => activeBonusSlots.Keys.Where((CActiveBonus x) => x.Type() == CAbility.EAbilityType.PreventDamage).ToList();

	private void CreateBonus(CActor actor, CActiveBonus bonus)
	{
		FFSNet.Console.LogInfo("Creating active bonus. BaseCardID: " + bonus.BaseCard.ID + ", AbilityName: " + bonus.Ability.Name);
		if (!activeBonusSlots.ContainsKey(bonus))
		{
			UIUseActiveBonus slotFromPool = GetSlotFromPool();
			activeBonusSlots[bonus] = slotFromPool;
			slotFromPool.Show();
			activeBonusSlots[bonus].SetActiveBonus(actor, bonus, onClickActiveBonus, onClickActiveBonus, isMandatoryChecker);
		}
		else
		{
			activeBonusSlots[bonus].gameObject.SetActive(value: true);
		}
	}

	private void RemoveBonus(CActiveBonus bonus)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			activeBonusSlots[bonus].gameObject.SetActive(value: false);
			pool.Push(activeBonusSlots[bonus]);
			activeBonusSlots.Remove(bonus);
			FFSNet.Console.LogInfo("Removed ActiveBonus. BaseCardID: " + bonus.BaseCard.ID + ", AbilityName: " + bonus.Ability.Name);
		}
	}

	public void RefreshBonus(CActiveBonus bonus)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			activeBonusSlots[bonus].Refresh();
		}
	}

	public void SetInteractionAvailableSlots(bool interactable, bool onlyChangeUnselected = false)
	{
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (!onlyChangeUnselected || !activeBonusSlot.Value.IsSelected())
			{
				activeBonusSlot.Value.SetInteractable(interactable);
			}
		}
	}

	private UIUseActiveBonus GetSlotFromPool()
	{
		UIUseActiveBonus uIUseActiveBonus;
		if (pool.Count > 0)
		{
			uIUseActiveBonus = pool.Pop();
			uIUseActiveBonus.gameObject.SetActive(value: true);
			return uIUseActiveBonus;
		}
		uIUseActiveBonus = UnityEngine.Object.Instantiate(activeBonusPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseActiveBonus.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseActiveBonus.transform;
		return uIUseActiveBonus;
	}

	public UIUseActiveBonus GetSlotForActiveBonus(CActiveBonus bonus)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			return activeBonusSlots[bonus];
		}
		return null;
	}

	private void Clear(bool toggle = false)
	{
		Debug.Log("[ACTIVE BONUS BAR]: Clear.");
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (toggle)
			{
				activeBonusSlot.Value.ClearSelection();
			}
			activeBonusSlot.Value.Hide();
			pool.Push(activeBonusSlot.Value);
		}
		if (!activeBonusSlots.IsNullOrEmpty())
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
		activeBonusSlots.Clear();
	}

	public void Remove(CActiveBonus bonus, bool clearSelection = false)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			UIUseActiveBonus uIUseActiveBonus = activeBonusSlots[bonus];
			if (clearSelection)
			{
				uIUseActiveBonus.ClearSelection();
			}
			uIUseActiveBonus.Hide();
			pool.Push(uIUseActiveBonus);
			activeBonusSlots.Remove(bonus);
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void Hide(bool toggle = false)
	{
		Debug.Log("[ACTIVE BONUS BAR]: Hide");
		isShown = false;
		InfusionBoardUI.Instance.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		Clear(toggle);
	}

	public void LockToggledActiveBonuses()
	{
		Debug.Log("[ACTIVE BONUS BAR]: Lock toggled active bonuses.");
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (activeBonusSlot.Key.ToggledBonus && !activeBonusSlot.Key.ToggleLocked)
			{
				ScenarioRuleClient.LockActiveBonus(activeBonusSlot.Key);
			}
		}
	}

	private void OnUnreservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		bool flag = false;
		foreach (Tuple<CActiveBonus, CActor> activeBonuse in GetActiveBonuses(actors, m_LastShownType, m_LastShownBehaviourType, m_LastAbilityAlreadyStarted))
		{
			if (!activeBonusSlots.ContainsKey(activeBonuse.Item1))
			{
				CreateBonus(activeBonuse.Item2, activeBonuse.Item1);
				flag = true;
			}
		}
		if (flag)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	private void OnReservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		bool flag = false;
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item in activeBonusSlots.ToList())
		{
			if (!item.Value.IsSelected() && item.Key.HasConsumeElements() && !CanConsume(item.Key, InfusionBoardUI.Instance.GetAvailableElements()))
			{
				flag = true;
				RemoveBonus(item.Key);
			}
		}
		if (flag)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void UndoSelection()
	{
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item in activeBonusSlots.ToList())
		{
			item.Value.ClearSelection();
		}
		List<Tuple<CActiveBonus, CActor>> list = ((m_LastShownType != CAbility.EAbilityType.PreventDamage) ? GetActiveBonuses(actors, m_LastShownType, m_LastShownBehaviourType, m_LastAbilityAlreadyStarted) : GetPreventDamageActiveBonuses(actors, m_LastShownType, m_LastShownDamageToTake));
		bool flag = false;
		foreach (Tuple<CActiveBonus, CActor> item2 in list)
		{
			if (!activeBonusSlots.ContainsKey(item2.Item1))
			{
				CreateBonus(item2.Item2, item2.Item1);
				flag = true;
			}
		}
		if (flag)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	private void Init(List<CActor> actors, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType, Action<CActiveBonus, CActor> onClickActiveBonus = null, Func<CActiveBonus, bool> isMandatory = null, bool abilityAlreadyStarted = false, bool typeChanged = false)
	{
		this.actors = actors;
		isMandatoryChecker = isMandatory;
		this.onClickActiveBonus = onClickActiveBonus;
		m_LastShownType = abilityType;
		m_LastShownBehaviourType = behaviourType;
		InfusionBoardUI.Instance.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		InfusionBoardUI.Instance.OnReservedElement.RemoveListener(OnReservedElement);
		m_LastAbilityAlreadyStarted = abilityAlreadyStarted;
		InfusionBoardUI.Instance.OnUnreserveElement.AddListener(OnUnreservedElement);
		InfusionBoardUI.Instance.OnReservedElement.AddListener(OnReservedElement);
	}

	public void ShowActiveBonus(CActor actor, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, Action<CActiveBonus, bool> onClickActiveBonus = null, bool abilityAlreadyStarted = false, bool showSingleTargetBonus = false, CAbility ability = null)
	{
		ShowActiveBonus(new List<CActor> { actor }, abilityType, behaviourType, onClickActiveBonus, abilityAlreadyStarted, showSingleTargetBonus, ability);
	}

	public void ShowActiveBonus(List<CActor> actors, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, Action<CActiveBonus, bool> onClickActiveBonus = null, bool abilityAlreadyStarted = false, bool showSingleTargetBonus = false, CAbility ability = null)
	{
		Action<CActiveBonus, CActor> action = delegate(CActiveBonus bonus, CActor actor)
		{
			onClickActiveBonus?.Invoke(bonus, activeBonusSlots[bonus].IsSelected());
		};
		bool flag = false;
		if (isShown)
		{
			if (abilityType != m_LastShownType)
			{
				flag = true;
			}
			else if (!showSingleTargetBonus)
			{
				return;
			}
		}
		Init(actors, abilityType, behaviourType, action, null, abilityAlreadyStarted, flag);
		List<Tuple<CActiveBonus, CActor>> activeBonuses = GetActiveBonuses(actors, abilityType, behaviourType, abilityAlreadyStarted, showSingleTargetBonus, ability);
		foreach (Tuple<CActiveBonus, CActor> item in activeBonuses)
		{
			CreateBonus(item.Item2, item.Item1);
		}
		if (flag)
		{
			CAbility cAbility = null;
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				cAbility = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility?.m_Ability;
			}
			foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots.ToList())
			{
				bool flag2 = true;
				if (activeBonuses.Any((Tuple<CActiveBonus, CActor> x) => x.Item1 == activeBonusSlot.Key))
				{
					flag2 = false;
				}
				if (activeBonusSlot.Key.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.OverrideAbility || activeBonusSlot.Key.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.OverrideMoveAbility)
				{
					flag2 = false;
				}
				if (activeBonusSlot.Key.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility && activeBonusSlot.Key.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && (cAbility == null || cAbility.Name == cAbilityAddActiveBonus.AddAbility.Name))
				{
					flag2 = false;
				}
				if (activeBonusSlot.Key.Ability.ActiveBonusData.CostAbility != null && (cAbility == null || cAbility.Name == activeBonusSlot.Key.Ability.ActiveBonusData.CostAbility.Name))
				{
					flag2 = false;
				}
				if (flag2)
				{
					Remove(activeBonusSlot.Key, clearSelection: true);
				}
			}
		}
		if (activeBonuses.Count > 0)
		{
			isShown = true;
		}
		OnChangedActiveBonusAvailable.Invoke();
	}

	private List<Tuple<CActiveBonus, CActor>> GetActiveBonuses(List<CActor> actors, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, bool abilityAlreadyStarted = false, bool showSingleTargetBonus = false, CAbility ability = null)
	{
		List<Tuple<CActiveBonus, CActor>> list = new List<Tuple<CActiveBonus, CActor>>();
		for (int i = 0; i < actors.Count; i++)
		{
			foreach (CActiveBonus activeBonuse in GetActiveBonuses(actors[i], abilityType, behaviourType, abilityAlreadyStarted, showSingleTargetBonus, ability))
			{
				list.Add(new Tuple<CActiveBonus, CActor>(activeBonuse, actors[i]));
			}
		}
		return list;
	}

	private List<CActiveBonus> GetActiveBonuses(CActor actor, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, bool abilityAlreadyStarted = false, bool showSingleTargetBonus = false, CAbility ability = null)
	{
		Debug.Log("[ACTIVE BONUS BAR]: Get active bonuses. ShowSingleTargetBonus: " + showSingleTargetBonus);
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (abilityType == CAbility.EAbilityType.None && behaviourType != CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility && behaviourType != CActiveBonus.EActiveBonusBehaviourType.EndActionAbility)
		{
			return list;
		}
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance.GetAvailableElements();
		List<CActiveBonus> list2 = (from x in CActiveBonus.FindApplicableActiveBonuses(actor)
			where !x.IsSong
			select x).ToList();
		List<CActiveBonus> list3 = CActiveBonus.FindAllApplicableSongActiveBonuses(actor);
		List<CActiveBonus.EActiveBonusBehaviourType> list4 = new List<CActiveBonus.EActiveBonusBehaviourType> { CActiveBonus.EActiveBonusBehaviourType.EndActionAbility };
		foreach (CActiveBonus item in list2)
		{
			bool isToggleBonus = item.Ability.ActiveBonusData.IsToggleBonus;
			bool flag = item is CForgoActionsForCompanionActiveBonus;
			bool flag2 = item.Ability.ActiveBonusData.ValidAbilityTypes.Contains(abilityType);
			bool flag3 = ability == null || item.BespokeBehaviour == null || item.BespokeBehaviour.ValidTargetTypeFilters(ability);
			bool flag4 = (item.BespokeBehaviour is CDuringTurnAbilityActiveBonus_TriggerAbility && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.ActionSelection) || (item.BespokeBehaviour is CChooseAbilityActiveBonus_EndOfActionAbilityChoice && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.Action);
			bool flag5 = ability == null || (ability != null && ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.None);
			bool flag6 = ability == null || !(item.BespokeBehaviour is COverrideAbilityTypeActiveBonus_OverrideAbility) || !ability.IsMergedAbility;
			bool flag7 = item.Ability.AbilityType != CAbility.EAbilityType.PreventDamage && item.Ability.AbilityType != CAbility.EAbilityType.Shield;
			bool flag8 = (list4.Contains(item.Ability.ActiveBonusData.Behaviour) ? (item.Ability.ActiveBonusData.Behaviour == behaviourType) : (behaviourType == CActiveBonus.EActiveBonusBehaviourType.None || item.Ability.ActiveBonusData.Behaviour == behaviourType));
			bool flag9 = !item.IsRestricted(actor) && item.RequirementsMet(actor);
			bool flag10 = !item.Ability.ActiveBonusData.IsSingleTargetBonus || showSingleTargetBonus;
			bool flag11 = !item.HasConsumeElements() || CanConsume(item, availableElements);
			bool flag12 = CanConsumeResources(item, actor);
			bool flag13 = !item.IsAura || item.ValidActorsInRangeOfAura.Contains(actor);
			bool flag14 = item.Ability.ActiveBonusData.AttackType == CAbility.EAttackType.None || item.Ability.ActiveBonusData.AttackType == CAbility.EAttackType.Attack;
			if (ability != null && ability is CAbilityAttack attackAbility)
			{
				flag14 = item.IsValidAttackType(attackAbility);
			}
			if ((isToggleBonus || flag) && (flag2 || flag4) && flag3 && flag5 && flag6 && flag7 && flag8 && flag14 && flag9 && flag10 && flag11 && flag12 && flag13)
			{
				list.Add(item);
			}
		}
		foreach (CActiveBonus item2 in list3.FindAll((CActiveBonus it) => it.IsSong && it.Ability.ActiveBonusData.IsToggleBonus && it.Ability.ActiveBonusData.IsSingleTargetBonus == showSingleTargetBonus && !it.IsRestricted(actor) && (!it.HasConsumeElements() || CanConsume(it, availableElements)) && CanConsumeResources(it, actor)))
		{
			foreach (CSong.SongEffect songEffect in item2.Ability.Song.SongEffects)
			{
				if (songEffect.IsValidAbilityType(abilityType))
				{
					if (!list.Contains(item2))
					{
						list.Add(item2);
					}
					break;
				}
			}
		}
		if (abilityAlreadyStarted)
		{
			list.RemoveAll((CActiveBonus it) => it.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.OverrideMoveAbility);
		}
		if (abilityAlreadyStarted && abilityType != CAbility.EAbilityType.Move)
		{
			list.RemoveAll((CActiveBonus it) => it.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.OverrideAbility || it.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.CastAbilityFromSummon || it.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.DuringActionAbility);
		}
		return list;
	}

	public void ShowReduceDamageActiveBonuses(CActor actor, CAbility damageAbility, bool isLethalDamage, Action<CActiveBonus, CActor> onClickActiveBonus = null)
	{
		if (isShown)
		{
			return;
		}
		m_LastShownDamageToTake = isLethalDamage;
		CAbility.EAbilityType abilityType = damageAbility?.AbilityType ?? CAbility.EAbilityType.None;
		isShown = true;
		Init(new List<CActor> { actor }, CAbility.EAbilityType.PreventDamage, CActiveBonus.EActiveBonusBehaviourType.None, onClickActiveBonus, (CActiveBonus bonus) => !bonus.Ability.ActiveBonusData.ToggleIsOptional);
		List<CActiveBonus> preventDamageActiveBonuses = GetPreventDamageActiveBonuses(actor, abilityType, isLethalDamage);
		preventDamageActiveBonuses.AddRange(GetShieldIncomingAttackActiveBonuses(actor, abilityType));
		foreach (CActiveBonus item in preventDamageActiveBonuses)
		{
			CreateBonus(actor, item);
		}
		if (preventDamageActiveBonuses.Count > 0)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void ShowReduceDamageActiveBonuses(CActor actor, CAbility.EAbilityType abilityType, bool isLethalDamage, Action<CActiveBonus, CActor> onClickActiveBonus = null)
	{
		if (isShown)
		{
			return;
		}
		m_LastShownDamageToTake = isLethalDamage;
		isShown = true;
		Init(new List<CActor> { actor }, CAbility.EAbilityType.PreventDamage, CActiveBonus.EActiveBonusBehaviourType.None, onClickActiveBonus, (CActiveBonus bonus) => !bonus.Ability.ActiveBonusData.ToggleIsOptional);
		List<CActiveBonus> preventDamageActiveBonuses = GetPreventDamageActiveBonuses(actor, abilityType, isLethalDamage);
		preventDamageActiveBonuses.AddRange(GetShieldIncomingAttackActiveBonuses(actor, abilityType));
		foreach (CActiveBonus item in preventDamageActiveBonuses)
		{
			CreateBonus(actor, item);
		}
		if (preventDamageActiveBonuses.Count > 0)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void RefreshLethalReduceDamageActiveBonuses(CPlayerActor actor, bool isLethal)
	{
		if (!isShown || !actors.Contains(actor))
		{
			return;
		}
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (activeBonusSlot.Key is CPreventDamageActiveBonus { PreventOnlyIfLethal: not false })
			{
				if (isLethal)
				{
					activeBonusSlot.Value.SetInteractable(interactable: true);
				}
				else if (!activeBonusSlot.Value.IsSelected())
				{
					activeBonusSlot.Value.SetInteractable(interactable: false);
				}
			}
		}
	}

	public void RefreshSingleTargetActiveBonusesFromSRLState()
	{
		if (!isShown)
		{
			return;
		}
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (activeBonusSlot.Key.Ability.ActiveBonusData.IsSingleTargetBonus)
			{
				if (activeBonusSlot.Key.ToggledBonus)
				{
					activeBonusSlot.Value.Select();
				}
				else
				{
					activeBonusSlot.Value.ClearSelection();
				}
			}
		}
	}

	private List<Tuple<CActiveBonus, CActor>> GetPreventDamageActiveBonuses(List<CActor> actors, CAbility.EAbilityType abilityType, bool isLetal)
	{
		List<Tuple<CActiveBonus, CActor>> list = new List<Tuple<CActiveBonus, CActor>>();
		for (int i = 0; i < actors.Count; i++)
		{
			foreach (CActiveBonus preventDamageActiveBonuse in GetPreventDamageActiveBonuses(actors[i], abilityType, isLetal))
			{
				list.Add(new Tuple<CActiveBonus, CActor>(preventDamageActiveBonuse, actors[i]));
			}
		}
		return list;
	}

	public List<CActiveBonus> GetPreventDamageActiveBonuses(CActor actor, CAbility.EAbilityType abilityType, bool isLethal)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance.GetAvailableElements();
		List<CActiveBonus> list = CharacterClassManager.FindAllActiveBonuses(CAbility.EAbilityType.PreventDamage, actor);
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		list2.AddRange(list.FindAll((CActiveBonus it) => it is CPreventDamageActiveBonus cPreventDamageActiveBonus && it.Layout != null && it.Ability.ActiveBonusData.IsToggleBonus && !it.IsRestricted(actor) && (!cPreventDamageActiveBonus.PreventOnlyIfLethal || (cPreventDamageActiveBonus.PreventOnlyIfLethal && isLethal)) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(abilityType)) && (!it.HasConsumeElements() || CanConsume(it, availableElements)) && CanConsumeResources(it, actor)));
		if (abilityType == CAbility.EAbilityType.Attack)
		{
			return list2.FindAll((CActiveBonus x) => !(x is CPreventDamageActiveBonus { HasTracker: not false } cPreventDamageActiveBonus) || cPreventDamageActiveBonus.Remaining > 0).ToList();
		}
		return list2.FindAll((CActiveBonus x) => x is CPreventDamageActiveBonus cPreventDamageActiveBonus && (!cPreventDamageActiveBonus.HasTracker || cPreventDamageActiveBonus.Remaining > 0) && !cPreventDamageActiveBonus.PreventDamageAttackSourcesOnly).ToList();
	}

	public List<CActiveBonus> GetShieldIncomingAttackActiveBonuses(CActor actor, CAbility.EAbilityType abilityType)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance.GetAvailableElements();
		List<CActiveBonus> list = CharacterClassManager.FindAllActiveBonuses(actor);
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		list2.AddRange(list.FindAll((CActiveBonus it) => it.Ability.AbilityType == CAbility.EAbilityType.Shield && it.Layout != null && it.Ability.ActiveBonusData.IsToggleBonus && !it.IsRestricted(actor) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(abilityType)) && (!it.HasConsumeElements() || CanConsume(it, availableElements)) && CanConsumeResources(it, actor)));
		return list2.FindAll((CActiveBonus x) => x is CShieldActiveBonus && x.BespokeBehaviour != null && (x.BespokeBehaviour is CShieldActiveBonus_BuffShield || x.BespokeBehaviour is CShieldActiveBonus_ShieldAndDisadvantage || x.BespokeBehaviour is CShieldActiveBonus_ShieldAndRetaliate)).ToList();
	}

	private bool CanConsume(CActiveBonus bonus, List<ElementInfusionBoardManager.EElement> availableElements)
	{
		if (availableElements.Count == 0)
		{
			return false;
		}
		ElementInfusionBoardManager.EElement eElement = bonus.Ability.ActiveBonusData.Consuming[0];
		if (eElement != ElementInfusionBoardManager.EElement.Any)
		{
			return availableElements.Contains(eElement);
		}
		return true;
	}

	private bool CanConsumeResources(CActiveBonus bonus, CActor actor)
	{
		if (bonus.Ability.ActiveBonusData.RequiredResources.Count > 0)
		{
			List<CCharacterResource> source = actor.CharacterResources.ToList();
			foreach (CActiveBonus item in (from it in activeBonusSlots
				where !it.Value.IsSelected()
				select it.Key).ToList())
			{
				List<CCharacterResource> toggledResourcesToConsume = item.ToggledResourcesToConsume;
				if (toggledResourcesToConsume == null || toggledResourcesToConsume.Count <= 0)
				{
					continue;
				}
				foreach (CCharacterResource resource in item.ToggledResourcesToConsume)
				{
					source.SingleOrDefault((CCharacterResource x) => x.ID == resource.ID).Amount -= resource.Amount;
				}
			}
			foreach (KeyValuePair<string, int> requiredResource in bonus.Ability.ActiveBonusData.RequiredResources)
			{
				if (!actor.CharacterHasResource(requiredResource.Key, requiredResource.Value))
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<CActiveBonus> GetNonSelectedActiveBonus()
	{
		return (from it in activeBonusSlots
			where !it.Value.IsSelected()
			select it.Key).ToList();
	}

	public List<CActiveBonus> GetSelectedActiveBonuses()
	{
		return (from it in activeBonusSlots
			where it.Value.IsSelected()
			select it.Key).ToList();
	}

	public void HighlightMandatoryActiveBonus()
	{
		if (isMandatoryChecker == null)
		{
			return;
		}
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (!activeBonusSlot.Value.IsSelected() && isMandatoryChecker(activeBonusSlot.Key))
			{
				activeBonusSlot.Value.Highlight();
			}
		}
	}

	public void HideSingleTargetActiveBonuses()
	{
		Debug.Log("[ACTIVE BONUS BAR]: Hide single target bonuses.");
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item in activeBonusSlots.ToList())
		{
			if (item.Key.Ability.ActiveBonusData.IsSingleTargetBonus)
			{
				Remove(item.Key, clearSelection: true);
			}
		}
	}

	public void ProxyUseActiveBonus(GameAction action)
	{
		if (Singleton<UIScenarioMultiplayerController>.Instance.damageMessage == null)
		{
			ActiveBonusToken activeBonusToken = action.SupplementaryDataToken as ActiveBonusToken;
			KeyValuePair<CActiveBonus, UIUseActiveBonus> tObj = activeBonusSlots.FirstOrDefault((KeyValuePair<CActiveBonus, UIUseActiveBonus> x) => x.Key.BaseCard.ID == activeBonusToken.BaseCardID && x.Key.ID == activeBonusToken.ActiveBonusID && x.Key.Ability.Name == activeBonusToken.AbilityName);
			if (tObj.IsTNull())
			{
				throw new Exception("Error using active bonus. No active bonus found (BaseCardID: " + activeBonusToken.BaseCardID + ", AbilityName: " + activeBonusToken.AbilityName + ", SelectedElementID: " + activeBonusToken.SelectedElementID + ").");
			}
			if (activeBonusToken.Selected)
			{
				tObj.Value.SetSelectedOptions(activeBonusToken.SelectedOptionsID.ToList());
				tObj.Value.SetAnyConsume((ElementInfusionBoardManager.EElement)activeBonusToken.SelectedElementID);
			}
			if (activeBonusToken.Selected != tObj.Value.IsSelected())
			{
				tObj.Value.Toggle();
			}
		}
	}
}
