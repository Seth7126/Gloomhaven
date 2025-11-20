using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using Script.GUI.Ability_Card;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FullAbilityCardAction : MonoBehaviour
{
	public enum CardHalf
	{
		NA,
		Top,
		Bottom
	}

	[SerializeField]
	public Button actionButton;

	[SerializeField]
	private Button defaultActionButton;

	[SerializeField]
	private UIFX_MaterialFX_Control lost;

	[SerializeField]
	private UIFX_MaterialFX_Control permLost;

	[SerializeField]
	private RectTransform cardContentTransform;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	public List<ConsumeButton> consumeButtons = new List<ConsumeButton>();

	[SerializeField]
	private List<InfuseElement> infuseElements = new List<InfuseElement>();

	[SerializeField]
	private CardActionHighlight highlightAction;

	[SerializeField]
	private CardActionHighlight highlightDefaultAction;

	[SerializeField]
	private ButtonSpritesAddressableLoader _buttonSpritesLoader;

	[SerializeField]
	private UITextTooltipTarget _fullActionTooltip;

	[SerializeField]
	private CardEnhancementElements _enhancementElements;

	private bool isInteractable;

	private bool isInteractableDefaultAction;

	private bool isSelected;

	private bool isSelectedDefaultAction;

	private ReferenceToSpriteState _stateReferencesForActionButton;

	private ReferenceToSprite _referenceForImageActionButton;

	private AbilityCardUISkin skin;

	public CardEnhancementElements EnhancementElements => _enhancementElements;

	[UsedImplicitly]
	private void Start()
	{
		UITextTooltipTarget[] componentsInChildren = cardContentTransform.GetComponentsInChildren<UITextTooltipTarget>(includeInactive: false);
		string text = null;
		for (int i = 0; i < componentsInChildren?.Length; i++)
		{
			UITextTooltipTarget uITextTooltipTarget = componentsInChildren[i];
			if (uITextTooltipTarget == _fullActionTooltip || uITextTooltipTarget == null)
			{
				continue;
			}
			TextMeshProUGUI component = uITextTooltipTarget.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				if (TryParseGlossaryTexts(component, out var parsedText, out var newKeywordsTexts))
				{
					uITextTooltipTarget.Initialize(UITooltip.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
					uITextTooltipTarget.SetText(parsedText);
					if (text == null)
					{
						text = parsedText;
						continue;
					}
					foreach (string item in newKeywordsTexts)
					{
						if (text.Contains(item))
						{
							int startIndex = text.IndexOf(item, StringComparison.Ordinal);
							text = text.Remove(startIndex, item.Length);
						}
					}
					text = text + "\n\n" + parsedText;
				}
				else
				{
					uITextTooltipTarget.enabled = false;
				}
			}
			else if (uITextTooltipTarget.GetComponent<RawImage>() != null)
			{
				uITextTooltipTarget.Initialize(UITooltip.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
				uITextTooltipTarget.SetText(CreateLayout.LocaliseText("$Glossary_HitArea$"));
			}
		}
		text = text?.TrimStart('\n');
		_fullActionTooltip?.SetText(text);
		highlightAction?.Hide();
		highlightDefaultAction?.Hide();
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		Show();
	}

	private bool TryParseGlossaryTexts(TextMeshProUGUI textBox, out string parsedText, out List<string> newKeywordsTexts)
	{
		newKeywordsTexts = new List<string>();
		parsedText = string.Empty;
		if (textBox.text.Contains("Summon"))
		{
			parsedText += CreateLayout.LocaliseText("$Glossary_Summon$");
			return true;
		}
		string text = "<sprite name=";
		string text2 = ">";
		List<string> list = new List<string>();
		int num = 0;
		while (num < textBox.text.Length)
		{
			int num2 = textBox.text.IndexOf(text, num);
			if (num2 < 0)
			{
				break;
			}
			int num3 = textBox.text.IndexOf(text2, num2);
			if (num3 < 0)
			{
				break;
			}
			num2 += text.Length;
			list.Add(string.Concat(from c in textBox.text.Substring(num2, num3 - num2)
				where !char.IsWhiteSpace(c)
				select c));
			num = num3 + text2.Length;
		}
		List<string> list2 = new List<string>();
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			if (list[num4].All(char.IsLetterOrDigit) && !list2.Contains(list[num4]))
			{
				string text3 = ((!(list[num4] == "Heal") || !textBox.transform.parent.name.StartsWith("Summon")) ? CreateLayout.LocaliseText("$Glossary_" + list[num4] + "$", skipWarnings: true) : CreateLayout.LocaliseText("$Glossary_Summon_Health$", skipWarnings: true));
				if (!text3.StartsWith("UNDEFINED"))
				{
					parsedText += (parsedText.IsNullOrEmpty() ? text3 : ("\n\n" + text3));
					newKeywordsTexts.Add(text3);
					list2.Add(list[num4]);
				}
				else
				{
					list[num4] = string.Empty;
				}
			}
			else
			{
				list[num4] = string.Empty;
			}
		}
		if (parsedText.IsNullOrEmpty())
		{
			return false;
		}
		return true;
	}

	public bool IsInteractable(CBaseCard.ActionType actionType, bool considerSelection = true)
	{
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.TopAction)
		{
			if (isInteractable)
			{
				if (isSelected)
				{
					return !considerSelection;
				}
				return true;
			}
			return false;
		}
		if (isInteractableDefaultAction)
		{
			if (isSelectedDefaultAction)
			{
				return !considerSelection;
			}
			return true;
		}
		return false;
	}

	public bool IsInteractable(bool considerSelection = true)
	{
		if (!IsInteractable(CBaseCard.ActionType.BottomAction, considerSelection))
		{
			return IsInteractable(CBaseCard.ActionType.DefaultMoveAction, considerSelection);
		}
		return true;
	}

	public void ToggleAnimateBurnIcon(bool active)
	{
		if (lost != null)
		{
			lost.ToggleEnable(active);
		}
		if (permLost != null)
		{
			permLost.ToggleEnable(active);
		}
	}

	public void ToggleHoverInfusion(bool active)
	{
		foreach (InfuseElement infuseElement in infuseElements)
		{
			infuseElement.ToggleHoverEffect(active);
		}
	}

	public void ToggleSelectedHighlightInfusion(bool active)
	{
		foreach (InfuseElement infuseElement in infuseElements)
		{
			infuseElement.TogglePermanentHighlight(active);
		}
	}

	public void AnimateGenerateInfusions()
	{
		foreach (InfuseElement infuseElement in infuseElements)
		{
			infuseElement.AnimateGeneratedElement();
		}
	}

	public void ToggleHighlightHover(bool active, bool isDefaultAbility)
	{
		if (active)
		{
			if (isDefaultAbility)
			{
				highlightDefaultAction?.ShowHover();
			}
			else
			{
				highlightAction?.ShowHover();
			}
		}
		else if (isDefaultAbility)
		{
			highlightDefaultAction?.Hide();
		}
		else
		{
			highlightAction?.Hide();
		}
	}

	public void HighlightAvailableConsumes(List<ElementInfusionBoardManager.EElement> availableElements)
	{
		for (int i = 0; i < consumeButtons.Count; i++)
		{
			ConsumeButton consumeButton = consumeButtons[i];
			consumeButton.SetAvailable(availableElements != null && consumeButton.CanConsumeRequiredElements(availableElements));
		}
	}

	public void ToggleHover(bool active, bool isDefaultAbility)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		if (active)
		{
			if (isDefaultAbility)
			{
				pointerEventData.pointerEnter = defaultActionButton.gameObject;
				defaultActionButton.OnPointerEnter(pointerEventData);
			}
			else
			{
				pointerEventData.pointerEnter = actionButton.gameObject;
				actionButton.OnPointerEnter(pointerEventData);
			}
		}
		else if (isDefaultAbility)
		{
			defaultActionButton.OnPointerExit(pointerEventData);
		}
		else
		{
			actionButton.OnPointerExit(pointerEventData);
		}
	}

	public void SetInteractable(bool active, bool defaultAction)
	{
		if (defaultAction)
		{
			isInteractableDefaultAction = active;
		}
		else
		{
			isInteractable = active;
		}
		if (canvasGroup != null && !defaultAction)
		{
			canvasGroup.alpha = (active ? 1f : 0.5f);
		}
		if (defaultAction)
		{
			defaultActionButton.interactable = active;
		}
		else
		{
			actionButton.interactable = active;
			foreach (InfuseElement infuseElement in infuseElements)
			{
				if (active)
				{
					infuseElement.Enable();
				}
				else
				{
					infuseElement.Disable();
				}
			}
			foreach (EnhancementButton item in _enhancementElements.Buttons.Where((EnhancementButton it) => it.InfuseElement != null))
			{
				if (active)
				{
					item.InfuseElement.Enable();
				}
				else
				{
					item.InfuseElement.Disable();
				}
			}
		}
		if (isSelected && !active)
		{
			ToggleSelect(active: false);
		}
		if (isSelectedDefaultAction && !active)
		{
			ToggleSelect(active: false, isDefaultAction: true);
		}
	}

	public void SetInteractable(bool active)
	{
		SetInteractable(active, defaultAction: true);
		SetInteractable(active, defaultAction: false);
	}

	public void ResetInteractable()
	{
		isInteractableDefaultAction = false;
		isInteractable = false;
		isSelectedDefaultAction = false;
		isSelected = false;
		highlightAction?.Hide();
		highlightDefaultAction?.Hide();
	}

	public void ToggleSelect(bool active, bool isDefaultAction = false)
	{
		if (isDefaultAction)
		{
			isSelectedDefaultAction = active;
			if (!active)
			{
				defaultActionButton.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}
		else
		{
			isSelected = active;
			if (!active)
			{
				actionButton.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		if (isSelected)
		{
			highlightAction?.ShowSelected();
			highlightDefaultAction?.Hide();
		}
		else if (isSelectedDefaultAction)
		{
			highlightDefaultAction?.ShowSelected();
			highlightAction?.Hide();
		}
		else
		{
			highlightAction?.Hide();
			highlightDefaultAction?.Hide();
		}
	}

	private void Update()
	{
		if (isSelectedDefaultAction)
		{
			defaultActionButton.OnSelect(new BaseEventData(EventSystem.current));
		}
		if (isSelected)
		{
			actionButton.OnSelect(new BaseEventData(EventSystem.current));
		}
	}

	public List<InfuseElement> UnselectedInfusions()
	{
		List<InfuseElement> list = new List<InfuseElement>();
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (!infuseElement.IsSelected)
			{
				list.Add(infuseElement);
			}
		}
		return list;
	}

	public List<ElementInfusionBoardManager.EElement> SelectedAnyInfusionElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (infuseElement.IsAnyElement)
			{
				list.Add(infuseElement.SelectedElement);
			}
		}
		return list;
	}

	public CreateLayout MakeAbilityAction(CardLayoutGroup parentGroup, int abilityID, Vector2 contentPadding, bool isLost, bool isPermaLost, bool isLongRest, CardHalf cardHalf)
	{
		cardContentTransform.sizeDelta -= contentPadding;
		if (lost != null)
		{
			lost.gameObject.SetActive(isLost);
		}
		if (permLost != null)
		{
			permLost.gameObject.SetActive(isPermaLost);
		}
		if (parentGroup != null || isLongRest)
		{
			CreateLayout createLayout = new CreateLayout(parentGroup, cardContentTransform.rect, abilityID, isLongRest, _enhancementElements, inConsume: false, cardContentTransform, isItemCard: false, cardHalf);
			consumeButtons = createLayout._consumeButtons;
			infuseElements = createLayout._infuseElements;
			return createLayout;
		}
		return null;
	}

	public void AddEventPusher(FullCardEventPusher.EventType eventType)
	{
		defaultActionButton.gameObject.AddFullCardEventPusher(eventType | FullCardEventPusher.EventType.Default);
		base.gameObject.AddFullCardEventPusher(eventType);
	}

	public void MakeAbilityActionContinued(CreateLayout newLayout)
	{
		if (!newLayout.IsLongRest)
		{
			newLayout.GenerateFullLayout();
			GameObject fullLayout = newLayout.FullLayout;
			fullLayout.transform.SetParent(cardContentTransform);
			RectTransform obj = fullLayout.transform as RectTransform;
			obj.anchoredPosition = Vector2.zero;
			obj.localScale = new Vector3(1f, 1f, 1f);
		}
		newLayout = null;
	}

	public void SetSkin(AbilityCardUISkin skin, bool topAction, bool longRest)
	{
		this.skin = skin;
		if (skin != null)
		{
			if (longRest)
			{
				_stateReferencesForActionButton = skin.GetLongRestActionSpriteState();
				_referenceForImageActionButton = new ReferenceToSprite(skin.longRestActionRegularSprite);
				return;
			}
			_referenceForImageActionButton = (topAction ? skin.TopActionRegularSprite : skin.BottomActionRegularSprite);
			_stateReferencesForActionButton = skin.GetActionSpriteState(topAction);
			defaultActionButton.image.sprite = (topAction ? skin.defaultTopActionRegularSprite : skin.defaultBottomActionRegularSprite);
			defaultActionButton.spriteState = skin.GetDefaultActionSpriteState(topAction);
		}
	}

	public void Show()
	{
		ApplyImage();
	}

	private void ApplyImage()
	{
		if (_referenceForImageActionButton != null)
		{
			_buttonSpritesLoader.AddReferenceToSprites(actionButton, _referenceForImageActionButton, _stateReferencesForActionButton);
		}
	}

	public void UnloadAll()
	{
		_buttonSpritesLoader.UnloadAll();
	}

	public void UpdateView(CBaseCard.ECardPile cardPile)
	{
		if (consumeButtons == null)
		{
			return;
		}
		foreach (ConsumeButton consumeButton in consumeButtons)
		{
			if (isInteractable)
			{
				consumeButton.Enable();
			}
			else
			{
				consumeButton.Disable();
			}
		}
	}

	public void DisplaySelected(bool selected, bool topAction, bool longRest)
	{
		if (skin != null)
		{
			if (longRest)
			{
				_referenceForImageActionButton = (selected ? new ReferenceToSprite(skin.longRestActionHighlightSprite) : new ReferenceToSprite(skin.longRestActionRegularSprite));
			}
			else if (selected)
			{
				_referenceForImageActionButton = (topAction ? skin.TopActionHighlightSprite : skin.BottomActionHighlightSprite);
				defaultActionButton.image.sprite = (topAction ? skin.defaultTopActionHighlightSprite : skin.defaultBottomActionHighlightSprite);
			}
			else
			{
				_referenceForImageActionButton = (topAction ? skin.TopActionRegularSprite : skin.BottomActionRegularSprite);
				defaultActionButton.image.sprite = (topAction ? skin.defaultTopActionRegularSprite : skin.defaultBottomActionRegularSprite);
				ApplyImage();
			}
		}
	}

	public void DisableHoverHighlight(bool disable)
	{
		Button button = defaultActionButton;
		Selectable.Transition transition = (actionButton.transition = ((!disable) ? Selectable.Transition.SpriteSwap : Selectable.Transition.None));
		button.transition = transition;
	}
}
