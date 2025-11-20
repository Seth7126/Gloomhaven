using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.YML.Locations;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITempleShopInventory : MonoBehaviour
{
	[Serializable]
	public class TempleSlotEvent : UnityEvent<TempleYML.TempleBlessingDefinition>
	{
	}

	[SerializeField]
	private UITempleShopSlot slotPrefab;

	[SerializeField]
	private ScrollRect scrollContainer;

	[SerializeField]
	private UITempleSlotTooltip tooltip;

	[SerializeField]
	private GameObject emptyInventory;

	[SerializeField]
	private CanvasGroup slotsCanvasGroup;

	[SerializeField]
	private List<UITempleShopSlot> slots = new List<UITempleShopSlot>();

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private UIControllerKeyTip controllerTip;

	[SerializeField]
	private Image _focusBackground;

	private IBlessingShopService service;

	public TempleSlotEvent OnBlessingSelected;

	private void Awake()
	{
		if (_focusBackground != null)
		{
			_focusBackground.enabled = false;
		}
		controllerArea.OnFocused.AddListener(EnableNavigation);
		controllerTip.Hide();
	}

	public void ToggleFocusBackground(bool isActive)
	{
		if (_focusBackground != null)
		{
			_focusBackground.enabled = isActive;
		}
	}

	public void Display(List<TempleYML.TempleBlessingDefinition> blessings, IBlessingShopService service)
	{
		this.service = service;
		HelperTools.NormalizePool(ref slots, slotPrefab.gameObject, scrollContainer.content, blessings.Count);
		scrollContainer.content.gameObject.SetActive(value: false);
		for (int i = 0; i < blessings.Count; i++)
		{
			slots[i].SetBlessing(blessings[i], OnSelected, OnHovered);
		}
		emptyInventory.SetActive(value: true);
	}

	public void SetInteractable(bool isEnabled)
	{
		slotsCanvasGroup.interactable = isEnabled;
	}

	public void Hide()
	{
		tooltip.Hide();
	}

	private void OnSelected(UITempleShopSlot slot)
	{
		OnBlessingSelected.Invoke(slot.Blessing);
	}

	private void OnHovered(bool hovered, UITempleShopSlot slot)
	{
		if (hovered)
		{
			if (InputManager.GamePadInUse)
			{
				scrollContainer.ScrollToFit(slot.transform as RectTransform);
			}
			tooltip.Show(slot.Blessing, slot.transform as RectTransform, slot.IsAvailable);
		}
		else
		{
			tooltip.Hide();
		}
	}

	public bool HasAvailableSlots()
	{
		return slots.Any((UITempleShopSlot x) => x.IsAvailable);
	}

	public void Refresh(ICharacter character)
	{
		bool flag = false;
		if (character != null)
		{
			for (int i = 0; i < slots.Count; i++)
			{
				UITempleShopSlot uITempleShopSlot = slots[i];
				if (!uITempleShopSlot.gameObject.activeSelf)
				{
					break;
				}
				flag = true;
				uITempleShopSlot.UpdateOwner(character);
				uITempleShopSlot.RefreshAffordable(service.CanAfford(character.CharacterID, uITempleShopSlot.Blessing));
				uITempleShopSlot.RefreshAvailable(service.IsAvailable(character.CharacterID, uITempleShopSlot.Blessing));
				uITempleShopSlot.gameObject.SetActive(value: true);
			}
		}
		scrollContainer.content.gameObject.SetActive(character != null);
		emptyInventory.SetActive(!flag && character != null);
	}

	public void UpdateAvailable(TempleYML.TempleBlessingDefinition blessing, bool available)
	{
		UITempleShopSlot uITempleShopSlot = slots.First((UITempleShopSlot it) => it.gameObject.activeSelf && it.Blessing == blessing);
		uITempleShopSlot.RefreshAvailable(available);
		if (uITempleShopSlot.IsHovered)
		{
			tooltip.Show(uITempleShopSlot.Blessing, uITempleShopSlot.transform as RectTransform, uITempleShopSlot.IsAvailable);
		}
	}

	private void EnableNavigation()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.Temple);
	}
}
