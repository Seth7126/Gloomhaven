#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using Photon.Bolt;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIUseAbilitiesBar : Singleton<UIUseAbilitiesBar>
{
	public class Ability : IAbility
	{
		private CAbility ability;

		private CBaseAbilityCard card;

		private string tooltipTitle;

		public List<IInfuseElement> Infusions { get; private set; }

		public List<CAbility> Abilities => new List<CAbility> { ability };

		public Sprite Icon => UIInfoTools.Instance.GetElementUseSprite(ElementInfusionBoardManager.EElement.Any);

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionTitle
		{
			get
			{
				if (tooltipTitle != null || card == null)
				{
					return tooltipTitle;
				}
				return LocalizationManager.GetTranslation(card.Name);
			}
		}

		public string DescriptionText { get; private set; }

		public Ability(CAbility ability, CBaseAbilityCard card, List<IInfuseElement> infusions = null, string tooltipTitle = null, string tooltipText = null)
		{
			Infusions = infusions;
			this.ability = ability;
			this.card = card;
			this.tooltipTitle = tooltipTitle;
			DescriptionText = tooltipText;
			ID = $"{ability.Name}{((card == null) ? string.Empty : card.ID.ToString())}";
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	public class DummyAbilityInfusion : IAbility
	{
		public List<IInfuseElement> Infusions { get; private set; }

		public List<CAbility> Abilities => new List<CAbility>();

		public Sprite Icon => UIInfoTools.Instance.GetElementUseSprite(ElementInfusionBoardManager.EElement.Any);

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionTitle { get; private set; }

		public string DescriptionText { get; private set; }

		public DummyAbilityInfusion(string id, List<IInfuseElement> infusions, string tooltipTitle = null, string tooltipText = null)
		{
			DescriptionText = tooltipText;
			DescriptionTitle = tooltipTitle;
			Infusions = infusions;
			ID = id;
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	public class InfuseAction : IAbility
	{
		private CAction action;

		private FullAbilityCard card;

		public List<IInfuseElement> Infusions { get; private set; }

		public List<CAbility> Abilities => action.Abilities;

		public Sprite Icon => UIInfoTools.Instance.GetElementUseSprite(ElementInfusionBoardManager.EElement.Any);

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionTitle => LocalizationManager.GetTranslation(card.AbilityCard.Name);

		public string DescriptionText => LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENT_INFUSE");

		public InfuseAction(CAction action, List<IInfuseElement> infusions, FullAbilityCard card)
		{
			Infusions = infusions;
			this.action = action;
			this.card = card;
			ID = $"{card.AbilityCard.ID} {((card.AbilityCard.BottomAction == action) ? CBaseCard.ActionType.BottomAction : CBaseCard.ActionType.TopAction)}";
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	private class DummyAnyInfusion : IInfuseElement
	{
		public ElementInfusionBoardManager.EElement SelectedElement { get; private set; } = ElementInfusionBoardManager.EElement.Any;

		public bool IsAnyElement => true;

		public void PickElement(ElementInfusionBoardManager.EElement element)
		{
			SelectedElement = element;
		}

		public void ResetElementToInitial()
		{
			SelectedElement = ElementInfusionBoardManager.EElement.Any;
		}
	}

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseAbility slotPrefab;

	[SerializeField]
	private UnityEvent OnChangedAbilitiesAvailable;

	private Dictionary<string, UIUseAbility> slots = new Dictionary<string, UIUseAbility>();

	private Stack<UIUseAbility> pool = new Stack<UIUseAbility>();

	private CPlayerActor actor;

	private Transform lastCreatedSlot;

	private Action<CAbility> onSelectChooseAbility;

	private Action onDeselectChooseAbility;

	private ChooseAbility currentChooseAbility;

	public bool IsShown => slots.Count > 0;

	private void Create(IAbility ability, Action<IAbility> onSelect, Action<IAbility> onDeselect = null, bool preSelect = false)
	{
		if (!slots.ContainsKey(ability.ID))
		{
			UIUseAbility slotFromPool = GetSlotFromPool();
			slots[ability.ID] = slotFromPool;
			slotFromPool.Show(preSelect);
		}
		else
		{
			slots[ability.ID].Show(instant: true);
		}
		slots[ability.ID].Init(actor, ability, onSelect, onDeselect);
		if (preSelect)
		{
			slots[ability.ID].Select();
		}
	}

	public void Remove(CAbility ability)
	{
		string key = $"{ability.Name}{((ability.AbilityBaseCard != null) ? ability.AbilityBaseCard.ID.ToString() : string.Empty)}";
		if (slots.ContainsKey(key))
		{
			UIUseAbility uIUseAbility = slots[key];
			uIUseAbility.Hide();
			pool.Push(uIUseAbility);
			slots.Remove(key);
			OnChangedAbilitiesAvailable.Invoke();
		}
	}

	private UIUseAbility GetSlotFromPool()
	{
		UIUseAbility uIUseAbility;
		if (pool.Count > 0)
		{
			uIUseAbility = pool.Pop();
			uIUseAbility.gameObject.SetActive(value: true);
			return uIUseAbility;
		}
		uIUseAbility = UnityEngine.Object.Instantiate(slotPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseAbility.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseAbility.transform;
		return uIUseAbility;
	}

	public void Hide(bool clearSelection = false)
	{
		foreach (KeyValuePair<string, UIUseAbility> slot in slots)
		{
			if (clearSelection)
			{
				slot.Value.ClearSelection();
			}
			slot.Value.Hide();
			pool.Push(slot.Value);
		}
		if (!slots.IsNullOrEmpty())
		{
			OnChangedAbilitiesAvailable.Invoke();
		}
		slots.Clear();
	}

	public void UndoSelection()
	{
		foreach (KeyValuePair<string, UIUseAbility> slot in slots)
		{
			slot.Value.ClearSelection();
		}
	}

	private void NetworkInfuseAbility(IAbility ability, bool infuse)
	{
		if (FFSNetwork.IsOnline && actor != null && actor.IsUnderMyControl)
		{
			int iD = actor.ID;
			IProtocolToken supplementaryDataToken = new InfuseToken(ability.ID, infuse ? ability.Infusions.ConvertAll((IInfuseElement it) => it.SelectedElement) : null);
			Synchronizer.SendGameAction(GameActionType.PickElement, ActionPhaseType.ElementPicking, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
	}

	public void ShowInfuseAbilities(CPlayerActor actor, List<CAbilityInfuse> abilities, Action<CAbility> onSelect, Action<CAbility> onDeselect)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		foreach (CAbilityInfuse it in abilities)
		{
			List<IInfuseElement> infusions = GetInfusions(actor, it);
			Create(new Ability(it, it.AbilityBaseCard as CAbilityCard, infusions, (infusions.Count == 1) ? LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENT_INFUSE") : string.Format(LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENTS_INFUSE"), infusions.Count)), delegate(IAbility ability)
			{
				NetworkInfuseAbility(ability, infuse: true);
				onSelect?.Invoke(it);
			}, delegate(IAbility ability)
			{
				NetworkInfuseAbility(ability, infuse: false);
				onDeselect?.Invoke(it);
			});
		}
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowInfusionsAction(CPlayerActor actor, FullAbilityCard card, CAction action, List<InfuseElement> infusions, Action onSelect)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		Create(new InfuseAction(action, infusions.Cast<IInfuseElement>().ToList(), card), delegate(IAbility ability)
		{
			NetworkInfuseAbility(ability, infuse: true);
			onSelect?.Invoke();
		});
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowGenericInfusion(CPlayerActor actor, Action<ElementInfusionBoardManager.EElement> onSelect, CAbility ability = null, CBaseAbilityCard card = null, string tooltipTitle = null, string tooltipText = null)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		List<IInfuseElement> infusions = new List<IInfuseElement>
		{
			new DummyAnyInfusion()
		};
		IAbility ability2 = ((ability != null) ? ((IAbility)new Ability(ability, card, infusions, tooltipTitle, tooltipText)) : ((IAbility)new DummyAbilityInfusion("DummyInfusion" + slots.Count, infusions, tooltipTitle, tooltipText)));
		Create(ability2, delegate(IAbility selectedAbility)
		{
			NetworkInfuseAbility(selectedAbility, infuse: true);
			onSelect?.Invoke(infusions[0].SelectedElement);
		});
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowChooseAbility(CPlayerActor actor, CAbilityChooseAbility ability, Action<CAbility> onSelect, Action onDeselect)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		onSelectChooseAbility = onSelect;
		onDeselectChooseAbility = onDeselect;
		currentChooseAbility = new ChooseAbility(actor, ability);
		Create(currentChooseAbility, delegate
		{
			if (FFSNetwork.IsOnline && actor != null && actor.IsUnderMyControl)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				int iD = actor.ID;
				IProtocolToken supplementaryDataToken = new ChooseAbilityToken(currentChooseAbility, selected: true, currentChooseAbility.SelectedOption);
				Synchronizer.SendGameAction(GameActionType.ChooseAbility, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			onSelectChooseAbility?.Invoke(currentChooseAbility.SelectedAbility);
		}, delegate
		{
			if (FFSNetwork.IsOnline && actor != null && actor.IsUnderMyControl)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				int iD = actor.ID;
				IProtocolToken supplementaryDataToken = new ChooseAbilityToken(currentChooseAbility, selected: false);
				Synchronizer.SendGameAction(GameActionType.ChooseAbility, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			onDeselectChooseAbility?.Invoke();
		}, preSelect: true);
		OnChangedAbilitiesAvailable.Invoke();
	}

	private List<IInfuseElement> GetInfusions(CPlayerActor actor, CAbilityInfuse ability)
	{
		List<IInfuseElement> list = new List<IInfuseElement>();
		if (ability.ParentBaseCard == null)
		{
			for (int i = 0; i < ability.ElementsToInfuse.Count((ElementInfusionBoardManager.EElement w) => w == ElementInfusionBoardManager.EElement.Any); i++)
			{
				list.Add(new DummyAnyInfusion());
			}
		}
		else
		{
			AbilityCardUI card = CardsHandManager.Instance.GetHand(actor).GetCard(ability.ParentBaseCard.ID);
			CBaseCard.ActionType? action = CardsActionControlller.s_Instance.GetAction(card.AbilityCard);
			foreach (EnhancementButton item in ((!action.HasValue) ? card.EnhancementElements : ((action == CBaseCard.ActionType.TopAction) ? card.fullAbilityCard.topActionButton.EnhancementElements : card.fullAbilityCard.bottomActionButton.EnhancementElements)).Buttons.Where((EnhancementButton w) => w.EnhancementType == EEnhancement.AnyElement).ToList())
			{
				list.Add(item.InfuseElement);
			}
		}
		return list;
	}

	public void SetAbilitiesInteractable(bool enable)
	{
		foreach (KeyValuePair<string, UIUseAbility> slot in slots)
		{
			slot.Value.SetInteractable(enable);
		}
	}

	public void ProxyInfuseAbility(GameAction gameAction)
	{
		InfuseToken infuseToken = gameAction.SupplementaryDataToken as InfuseToken;
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>(infuseToken.Elements.Length);
		for (int i = 0; i < infuseToken.Elements.Length; i++)
		{
			list.Add((ElementInfusionBoardManager.EElement)infuseToken.Elements[i]);
		}
		InfuseAbility(infuseToken.AbilityID, list);
	}

	public void InfuseAbility(string abilityId, List<ElementInfusionBoardManager.EElement> elements)
	{
		Debug.Log("InfuseAbility " + elements.Count + " " + abilityId);
		if (!slots.ContainsKey(abilityId))
		{
			throw new Exception("Ability id not found: " + abilityId);
		}
		if (elements.Count > 0)
		{
			slots[abilityId].SetInfusions(elements);
			slots[abilityId].Select();
			slots[abilityId].SelectAll(elements);
		}
		else
		{
			slots[abilityId].Unselect();
		}
	}

	public void ProxyChooseAbility(GameAction gameAction)
	{
		ChooseAbilityToken chooseAbilityToken = gameAction.SupplementaryDataToken as ChooseAbilityToken;
		ChooseAbility(chooseAbilityToken.AbilityID, chooseAbilityToken.Selected, chooseAbilityToken.SelectedOptionID);
		if (chooseAbilityToken.Selected)
		{
			onSelectChooseAbility?.Invoke(currentChooseAbility.SelectedAbility);
		}
		else
		{
			onDeselectChooseAbility?.Invoke();
		}
	}

	public void ChooseAbility(string abilityId, bool select, string optionId = null)
	{
		Debug.Log("ChooseAbility " + select + " " + abilityId);
		if (!slots.ContainsKey(abilityId))
		{
			throw new Exception("Ability id not found: " + abilityId);
		}
		if (select)
		{
			if (optionId != null)
			{
				slots[abilityId].SetSelectedOptions(new List<string> { optionId });
			}
			slots[abilityId].Select();
		}
		else
		{
			slots[abilityId].Unselect();
		}
	}
}
