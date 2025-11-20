#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseItemScenario : UIUseConsumeInfuseSlot<CItem>
{
	[SerializeField]
	private Image imageItem;

	[SerializeField]
	private UIItemTooltip tooltip;

	[SerializeField]
	private UIUseOption optionTick;

	[SerializeField]
	private UIUsePreview previewEffect;

	private Action<CItem, bool> onMouseHover;

	private CItem.EItemSlotState lastState;

	protected override void Awake()
	{
		base.Awake();
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnPointerDown, base.ShowHotkey, base.HideHotkey, isPersistent: false, KeyActionHandler.RegisterType.Click).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
	}

	public void SetItem(CItem item, Action<CItem> onSelectItem, Action<CItem> onUnselectItem, CActor iactor, Action<CItem, bool> onMouseHover = null, List<InfuseElement> infusions = null, Action<CItem> onPickedAll = null, Action onPickerCancel = null)
	{
		this.onMouseHover = onMouseHover;
		List<ElementInfusionBoardManager.EElement> list = item.YMLData.Consumes.ToList();
		if (item.YMLData.Data.Abilities != null)
		{
			foreach (ElementInfusionBoardManager.EElement item2 in item.YMLData.Data.Abilities.Where((CAbility it) => it is CAbilityConsumeElement).SelectMany((CAbility it) => (it as CAbilityConsumeElement).ElementsToConsume))
			{
				int num = list.LastIndexOf(item2);
				if (num != -1)
				{
					list.RemoveAt(num);
				}
			}
		}
		Init(iactor, item, onSelectItem, onUnselectItem, null, item.SlotState == CItem.EItemSlotState.Selected, infusions?.Cast<IInfuseElement>().ToList(), list, onPickedAll, onPickerCancel);
		if (item.SlotState == CItem.EItemSlotState.Useable && !item.ChosenElement.IsNullOrEmpty())
		{
			InfusionBoardUI.Instance.ReserveElements(item.ChosenElement, active: false);
			item.ChosenElement.Clear();
			selected = false;
		}
		Decorate(item);
		Refresh();
	}

	public bool WasSelected()
	{
		if (lastState != CItem.EItemSlotState.Selected)
		{
			return lastState == CItem.EItemSlotState.Locked;
		}
		return true;
	}

	protected override void OnSelectedElements(List<ElementInfusionBoardManager.EElement> elements)
	{
		if (consumes.Count > 0)
		{
			if (consumePickerController.IsSelecting())
			{
				consumePickerController.ClosePicker();
				return;
			}
			if (!consumePickerController.Pick())
			{
				return;
			}
		}
		if (infusions.Count > 0)
		{
			if (infusePickerController.IsSelecting())
			{
				infusePickerController.ClosePicker();
				return;
			}
			if (!infusePickerController.Pick())
			{
				return;
			}
		}
		Consume(elements, active: true);
		selected = true;
		Refresh();
		onSelect?.Invoke(element);
	}

	protected override void Infuse(List<ElementInfusionBoardManager.EElement> infusions, bool active)
	{
		if (active)
		{
			ElementInfusionBoardManager.Infuse(infusions, actor);
		}
		else
		{
			ElementInfusionBoardManager.Infuse(infusions, actor);
		}
		InfusionBoardUI.Instance.UpdateBoard(infusions);
	}

	protected override void Consume(List<ElementInfusionBoardManager.EElement> elements, bool active)
	{
		if (consumes.Exists((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any))
		{
			if (active)
			{
				element.ChosenElement = (from it in consumes
					where it.RequiredElement == ElementInfusionBoardManager.EElement.Any
					select it.SelectedElement.Value).ToList();
			}
			else if (!element.ChosenElement.IsNullOrEmpty())
			{
				element.ChosenElement.Clear();
			}
		}
		base.Consume(elements, active);
	}

	protected override void CreateConsumes(List<ElementInfusionBoardManager.EElement> elements)
	{
		base.CreateConsumes(elements);
		if (!element.ChosenElement.IsNullOrEmpty())
		{
			for (int i = 0; i < element.ChosenElement.Count; i++)
			{
				if (element.ChosenElement[i] != ElementInfusionBoardManager.EElement.Any)
				{
					consumes[i].SetSelectedElement(element.ChosenElement[i]);
				}
			}
		}
		else if (element.SlotState == CItem.EItemSlotState.Selected)
		{
			for (int j = 0; j < consumes.Count; j++)
			{
				consumes[j].SetSelectedElement(elements[j]);
			}
		}
	}

	private void Decorate(CItem item)
	{
		ItemConfigUI itemConfig = UIInfoTools.Instance.GetItemConfig(item.YMLData.Art);
		imageItem.sprite = itemConfig.miniIcon;
		selectAudioItem = itemConfig.toggleAudioItem;
		previewEffect.SetDescription(item);
		if (item.YMLData.Data.Abilities != null && item.YMLData.Data.Abilities.Exists((CAbility it) => it.AbilityType == CAbility.EAbilityType.Choose))
		{
			optionTick.Clear();
			optionTick.Show();
		}
		else
		{
			optionTick.Hide();
		}
		tooltip.Init(item);
	}

	public override void Refresh()
	{
		if (element != null)
		{
			lastState = element.SlotState;
			bool flag = selected;
			selected = element.SlotState != CItem.EItemSlotState.Useable && element.SlotState != CItem.EItemSlotState.Equipped && element.SlotState != CItem.EItemSlotState.None;
			base.Refresh();
			selected = flag;
		}
	}

	protected override void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.IsInitialized)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnPointerDown);
		}
		base.OnDestroy();
		tooltip.Clear();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		tooltip.Hide();
	}

	public override void OnPointerEnter()
	{
		base.OnPointerEnter();
		onMouseHover?.Invoke(element, arg2: true);
	}

	public override void OnPointerExit()
	{
		base.OnPointerExit();
		onMouseHover?.Invoke(element, arg2: false);
	}

	protected override void ShowTooltip(bool show)
	{
		if (show)
		{
			tooltip.Show();
		}
		else
		{
			tooltip.Hide();
		}
	}

	public override void OnPointerDown()
	{
		if (interactable && element.SlotState != CItem.EItemSlotState.Locked && element.SlotState != CItem.EItemSlotState.Spent && element.SlotState != CItem.EItemSlotState.Consumed)
		{
			base.OnPointerDown();
		}
	}

	public override bool Escape()
	{
		if (element.SlotState == CItem.EItemSlotState.Locked || element.SlotState == CItem.EItemSlotState.Spent || element.SlotState == CItem.EItemSlotState.Consumed)
		{
			UIWindowManager.UnregisterEscapable(this);
			return false;
		}
		return base.Escape();
	}

	public void SetInteractable(bool interactable, bool updateLook = false)
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			if (FFSNetwork.IsOnline)
			{
				if (element.YMLData.Slot == CItem.EItemSlot.QuestItem)
				{
					interactable = interactable && actor.IsUnderMyControl;
				}
				else
				{
					CMapCharacter cMapCharacter = SaveData.Instance.Global.CampaignData.AdventureMapState.MapParty.IsItemEquippedByParty(element);
					interactable = interactable && (!FFSNetwork.IsOnline || (cMapCharacter != null && PlayerRegistry.MyPlayer.HasControlOver(cMapCharacter.CharacterName.GetHashCode())));
				}
			}
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
		{
			if (SaveData.Instance.Global?.AdventureData?.AdventureMapState?.MapParty == null)
			{
				Debug.LogError("Adventure data is null");
			}
			if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer == null)
			{
				Debug.LogError("My player is null");
			}
			CMapCharacter cMapCharacter2 = SaveData.Instance.Global.AdventureData.AdventureMapState.MapParty.IsItemEquippedByParty(element);
			Debug.LogFormat("Item owner is {0}", cMapCharacter2?.CharacterID);
			interactable = interactable && (!FFSNetwork.IsOnline || (cMapCharacter2 != null && PlayerRegistry.MyPlayer.HasControlOver(CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter2.CharacterID))));
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial || SaveData.Instance.Global.GameMode == EGameMode.Autotest)
		{
			PlayerState playerWithItem = SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.FirstOrDefault((PlayerState p) => p.Items.Contains(element));
			if (playerWithItem != null && CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.CharacterID == playerWithItem.ClassID) != null)
			{
				CPlayerActor cPlayerActor = (CPlayerActor)playerWithItem.Actor;
				int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? cPlayerActor.CharacterName.GetHashCode() : cPlayerActor.CharacterClass.ModelInstanceID);
				interactable = interactable && (!FFSNetwork.IsOnline || PlayerRegistry.MyPlayer.HasControlOver(controllableID));
			}
		}
		if (updateLook)
		{
			base.SetInteractable(interactable);
		}
	}
}
