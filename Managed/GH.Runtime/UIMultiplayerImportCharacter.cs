using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMultiplayerImportCharacter : MonoBehaviour, IMoveHandler, IEventSystemHandler, ISubmitHandler
{
	[Serializable]
	public class ImportChangedEvent : UnityEvent<ICharacterImportData, bool>
	{
	}

	[Serializable]
	public class SlotEvent : UnityEvent<UIMultiplayerImportCharacter>
	{
	}

	[SerializeField]
	private Image characterIcon;

	[SerializeField]
	private TextMeshProUGUI characterName;

	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private TextLocalizedListener itemName;

	[SerializeField]
	private TextMeshProUGUI gainedXP;

	[SerializeField]
	private TextMeshProUGUI gainedGold;

	[SerializeField]
	private UITab skipToggle;

	[SerializeField]
	private UITab importToggle;

	[SerializeField]
	private UITextTooltipTarget importTooltip;

	[SerializeField]
	private List<Image> greyoutMasks;

	[SerializeField]
	private ExtendedButton selectable;

	public ImportChangedEvent OnImportToggled = new ImportChangedEvent();

	public SlotEvent OnSelected = new SlotEvent();

	private ICharacterImportData data;

	private UITab focusedOption;

	public bool EnabledImport => importToggle.isOn;

	public bool IsInteractable => selectable.IsInteractable();

	private void Awake()
	{
		importToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			OnImportToggled?.Invoke(data, isOn);
		});
		selectable.onSelected.AddListener(delegate
		{
			Focus((EnabledImport && skipToggle.interactable) ? skipToggle : (importToggle.interactable ? importToggle : null));
			OnSelected.Invoke(this);
		});
		selectable.onDeselected.AddListener(delegate
		{
			Focus(null);
		});
	}

	protected void OnDestroy()
	{
		importToggle.onValueChanged.RemoveAllListeners();
	}

	public void SetCharacter(ICharacterImportData data, bool interactable)
	{
		this.data = data;
		characterIcon.sprite = data.CharacterIcon;
		characterName.text = data.CharacterName;
		gainedXP.text = string.Format("{0}{1}", data.XP, LocalizationManager.GetTranslation("XP"));
		gainedGold.text = string.Format("{0} {1}", data.Gold, LocalizationManager.GetTranslation("Gold"));
		itemName.SetTextKey(data.ItemLocKey);
		itemIcon.sprite = data.ItemIcon;
		UITab uITab = importToggle;
		bool interactable2 = (skipToggle.interactable = interactable && data.CanImport());
		uITab.interactable = interactable2;
		skipToggle.SetIsOnWithoutNotify(value: true);
		importToggle.SetIsOnWithoutNotify(value: false);
		RefreshImport(data.CanImport());
	}

	private void RefreshImport(bool canImport)
	{
		TextMeshProUGUI textMeshProUGUI = characterName;
		Color color = (itemName.Text.color = (canImport ? UIInfoTools.Instance.mainColor : UIInfoTools.Instance.greyedOutTextColor));
		textMeshProUGUI.color = color;
		for (int i = 0; i < greyoutMasks.Count; i++)
		{
			greyoutMasks[i].material = (canImport ? null : UIInfoTools.Instance.greyedOutMaterial);
		}
		if (canImport)
		{
			importTooltip.enabled = false;
			return;
		}
		importTooltip.SetText(LocalizationManager.GetTranslation("GUI_CANNOT_IMPORT_CHARACTER_UNDERLEVEL"));
		importTooltip.enabled = true;
	}

	public void SetImport(bool enable)
	{
		if (enable)
		{
			importToggle.SetIsOnWithoutNotify(value: true);
		}
		else
		{
			skipToggle.SetIsOnWithoutNotify(value: true);
		}
	}

	public void EnableNavigation(bool select)
	{
		selectable.SetNavigation(Navigation.Mode.Vertical);
		if (select)
		{
			selectable.Select();
		}
	}

	public void DisableNavigation()
	{
		selectable.DisableNavigation();
		Focus(null);
	}

	public void OnMove(AxisEventData eventData)
	{
		switch (eventData.moveDir)
		{
		case MoveDirection.Left:
			if (skipToggle.IsInteractable())
			{
				Focus(skipToggle);
			}
			break;
		case MoveDirection.Right:
			if (importToggle.IsInteractable())
			{
				Focus(importToggle);
			}
			break;
		}
	}

	private void Focus(UITab option)
	{
		if (!(focusedOption == option))
		{
			if (focusedOption != null)
			{
				ExecuteEvents.Execute(focusedOption.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
				focusedOption = null;
			}
			focusedOption = option;
			if (option != null)
			{
				ExecuteEvents.Execute(option.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
			}
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (focusedOption != null && EventSystem.current.currentSelectedGameObject == selectable.gameObject)
		{
			focusedOption.isOn = true;
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
		}
	}
}
