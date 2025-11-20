#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GLOOM;
using SM.Gamepad;
using SM.Utils;
using ScenarioRuleLibrary;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardUI : LocalizedListener, IPooleable
{
	public int CardID;

	public TextMeshProUGUI titleText;

	public GameObject pricePanel;

	public TextMeshProUGUI priceText;

	public GameObject layoutHolder;

	public Image cardBackground;

	public Image itemSymbolImage;

	public Image conditionImage;

	public Sprite[] conditionSprites;

	public Image permanentlyConsumedImage;

	public bool lockUI = true;

	public UIItemModifiersTooltip modifiersTooltip;

	public Image validOwnerIcon;

	[NonSerialized]
	public bool m_IsSelected;

	public string prefabPath;

	public CreateLayout layout;

	public CItem item;

	public GameObject modifiersIcon;

	public ItemCardEffects cardEffects;

	[SerializeField]
	public List<InfuseElement> infuseElements = new List<InfuseElement>();

	[SerializeField]
	private ImageAddressableLoader _imageAddressableLoader;

	private ReferenceToSprite _referenceOnIcon;

	[SerializeField]
	private CardActionHighlight _cardActionHighlight;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private UITextTooltipTarget _allHintsCardTooltip;

	[SerializeField]
	private Vector2 _allHintsTooltipOffset;

	[SerializeField]
	private float _allHintsTooltipDistanceFromModifier = 5f;

	private List<UITextTooltipTarget> _activeTextTooltipTargets;

	private bool _imageLoaded;

	private CItem.EItemSlotState lastState;

	private CancellationTokenSource _spriteLoadCancellationTokenSource;

	private UINavigationSelectable _selectable;

	public string CardName { get; set; }

	public UITextTooltipTarget AllHintsCardTooltip => _allHintsCardTooltip;

	public UINavigationSelectable NavigationSelectable => _selectable ?? (_selectable = _highlight.GetComponent<UINavigationSelectable>());

	public static event EventHandler SelectionChanged;

	protected virtual void OnSelectionChanged()
	{
		ItemCardUI.SelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	private void Start()
	{
		m_IsSelected = false;
		OnLanguageChanged();
		LoadBackground();
		UITextTooltipTarget[] componentsInChildren = GetComponentsInChildren<UITextTooltipTarget>(includeInactive: false);
		_activeTextTooltipTargets = new List<UITextTooltipTarget>();
		List<string> parsedKeys = new List<string>();
		for (int i = 0; i < componentsInChildren?.Length; i++)
		{
			if (!(componentsInChildren[i] != null))
			{
				continue;
			}
			UITextTooltipTarget uITextTooltipTarget = componentsInChildren[i];
			TextMeshProUGUI component = uITextTooltipTarget.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				if (TryParseGlossaryTexts(component, out var parsedText, ref parsedKeys))
				{
					InitializeTooltipTarget(uITextTooltipTarget, parsedText);
					_activeTextTooltipTargets.Add(uITextTooltipTarget);
				}
				else
				{
					uITextTooltipTarget.enabled = false;
				}
				continue;
			}
			Image component2 = uITextTooltipTarget.GetComponent<Image>();
			if (component2 != null)
			{
				string locText = "$Glossary_RedHex$";
				if (!parsedKeys.Contains(locText) && component2.sprite.name.StartsWith("Red", StringComparison.OrdinalIgnoreCase))
				{
					InitializeTooltipTarget(uITextTooltipTarget, CreateLayout.LocaliseText(locText, skipWarnings: true));
					parsedKeys.Add(locText);
					_activeTextTooltipTargets.Add(uITextTooltipTarget);
				}
				else
				{
					uITextTooltipTarget.enabled = false;
				}
			}
			else
			{
				_activeTextTooltipTargets.Add(uITextTooltipTarget);
			}
		}
		SetupAllHintsCardTooltip();
	}

	private void InitializeTooltipTarget(UITextTooltipTarget target, string text)
	{
		target.Initialize(UITooltip.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
		target.SetText(text);
	}

	protected override void OnEnable()
	{
		LoadBackground();
	}

	protected override void OnDisable()
	{
		UnloadBackground();
	}

	private void SetupAllHintsCardTooltip()
	{
		string text = "";
		for (int i = 0; i < _activeTextTooltipTargets.Count; i++)
		{
			string text2 = "\n\n";
			if (i == _activeTextTooltipTargets.Count - 1)
			{
				text2 = "";
			}
			if (_activeTextTooltipTargets[i].ShownTooltipText.IsNOTNullOrEmpty())
			{
				text = text + _activeTextTooltipTargets[i].ShownTooltipText + text2;
				continue;
			}
			_activeTextTooltipTargets[i].SetText(string.Format(LocalizationManager.GetTranslation(_activeTextTooltipTargets[i].tooltipText)));
			text = text + _activeTextTooltipTargets[i].ShownTooltipText + text2;
		}
		if (AllHintsCardTooltip != null)
		{
			AllHintsCardTooltip.SetText(text);
		}
	}

	private bool TryParseGlossaryTexts(TextMeshProUGUI textBox, out string parsedText, ref List<string> parsedKeys)
	{
		parsedText = string.Empty;
		string[] separator = new string[2] { "<sprite name=", ">" };
		string[] array = textBox.text.Split(separator, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		List<string> list = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].All(char.IsLetterOrDigit) && !list.Contains(array[i]))
			{
				string locText = ((array[i] == "Heal" && textBox.transform.parent.name.StartsWith("Summon", StringComparison.Ordinal)) ? "$Glossary_Summon_Health$" : ("$Glossary_" + array[i] + "$"));
				string text = CreateLayout.LocaliseText(locText, skipWarnings: true);
				if (!text.StartsWith("UNDEFINED", StringComparison.Ordinal))
				{
					parsedText += (parsedText.IsNullOrEmpty() ? text : ("\n\n" + text));
					parsedKeys.Add(locText);
					list.Add(array[i]);
				}
				else
				{
					array[i] = string.Empty;
				}
			}
			else
			{
				array[i] = string.Empty;
			}
		}
		if (parsedText.IsNullOrEmpty())
		{
			return false;
		}
		return true;
	}

	public void CreateCard()
	{
		titleText.text = LocalizationManager.GetTranslation(item.Name);
		itemSymbolImage.sprite = UIInfoTools.Instance.GetItemSlotIcon(item.YMLData.Slot.ToString());
		conditionImage.enabled = item.YMLData.Usage != CItem.EUsageType.Unrestricted;
		Debug.Log($"[TextTooltip] conditionImage: {conditionImage.enabled}");
		conditionImage.sprite = ((item.YMLData.Usage == CItem.EUsageType.Spent) ? conditionSprites[0] : conditionSprites[1]);
		permanentlyConsumedImage.enabled = item.YMLData.PermanentlyConsumed == true;
		layout.FullLayout.transform.SetParent(layoutHolder.transform);
		(layout.FullLayout.transform as RectTransform).anchoredPosition = Vector2.zero;
		infuseElements = layout._infuseElements;
		modifiersIcon.SetActive(item.YMLData.Data.AdditionalModifiers.Count > 0);
		if (item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
		{
			validOwnerIcon.gameObject.SetActive(value: false);
			priceText.text = item.YMLData.Cost.ToString();
			pricePanel.gameObject.SetActive(value: true);
		}
		else
		{
			pricePanel.gameObject.SetActive(value: false);
			_referenceOnIcon = UIInfoTools.Instance.GetCharacterAssemblyIcon(item.YMLData.ValidEquipCharacterClassIDs.FirstOrDefault());
			validOwnerIcon.gameObject.SetActive(value: true);
		}
	}

	public void Show(bool highlightElement = true, bool showModifiers = false, bool showModifiersOnLeft = false)
	{
		if (highlightElement)
		{
			UIManager.Instance.HighlightElement(base.gameObject, lockUI);
		}
		base.gameObject.SetActive(value: true);
		UpdateState();
		EnableShowModifiers(showModifiers, showModifiersOnLeft);
	}

	public void HighLight(bool state)
	{
		if (state)
		{
			_cardActionHighlight?.ShowSelected();
			if ((bool)_highlight)
			{
				_highlight.enabled = true;
			}
		}
		else
		{
			_cardActionHighlight?.Hide();
			if ((bool)_highlight)
			{
				_highlight.enabled = false;
			}
		}
	}

	public void EnableShowModifiers(bool enable, bool showLeft = false)
	{
		modifiersTooltip.EnableTooltip = enable && item.YMLData.Data.AdditionalModifiers.Count > 0;
		if (modifiersTooltip.EnableTooltip)
		{
			modifiersTooltip.Initialize(item.YMLData.Data.AdditionalModifiers);
			modifiersTooltip.ShowTooltip(showLeft);
		}
		else
		{
			modifiersTooltip.HideTooltip();
		}
	}

	public void BuildTooltipLayout(bool showNodifiersLeft)
	{
		if (_allHintsCardTooltip == null)
		{
			return;
		}
		RectTransform rectTransform = _allHintsCardTooltip.transform as RectTransform;
		_allHintsCardTooltip.SetCorner((!showNodifiersLeft) ? UITooltip.Corner.TopLeft : UITooltip.Corner.TopRight);
		if (rectTransform == null)
		{
			LogUtils.LogError("AllHintsCardTooltip should have RectTransform component!");
			return;
		}
		rectTransform.anchorMax = new Vector2((!showNodifiersLeft) ? 1 : 0, rectTransform.anchorMax.y);
		rectTransform.anchorMin = new Vector2((!showNodifiersLeft) ? 1 : 0, rectTransform.anchorMin.y);
		rectTransform.pivot = new Vector2(showNodifiersLeft ? 1 : 0, rectTransform.pivot.y);
		Vector2 allHintsTooltipOffset = _allHintsTooltipOffset;
		allHintsTooltipOffset.x = (showNodifiersLeft ? allHintsTooltipOffset.x : (0f - allHintsTooltipOffset.x));
		if (modifiersTooltip.gameObject.activeSelf)
		{
			allHintsTooltipOffset.y -= modifiersTooltip.RectTransform.rect.height + _allHintsTooltipDistanceFromModifier;
		}
		rectTransform.anchoredPosition = allHintsTooltipOffset + new Vector2((!showNodifiersLeft) ? 1 : (-1), 1f);
	}

	public void Hide()
	{
		UIManager.Instance.UnhighlightElement(base.gameObject, lockUI);
		base.transform.localScale = Vector3.one;
		GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		base.gameObject.SetActive(value: false);
		modifiersTooltip.HideTooltip();
	}

	public void ToggleSelect()
	{
		ToggleSelect(m_IsSelected);
	}

	public void ToggleSelect(bool select)
	{
		m_IsSelected = select;
		OnSelectionChanged();
	}

	public void OnReturnedToPool()
	{
		modifiersTooltip.HideTooltip();
		EnableShowModifiers(enable: false);
		lockUI = true;
		CanvasGroup component = GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.enabled = false;
		}
		base.transform.localScale = Vector3.one;
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (infuseElement.IsAnyElement && infuseElement.IsSelected)
			{
				infuseElement.Init(ElementInfusionBoardManager.EElement.Any);
			}
		}
		cardEffects.RestoreCard();
		lastState = CItem.EItemSlotState.None;
	}

	public void OnRemovedFromPool()
	{
	}

	public void UpdateState(bool force = false)
	{
		UpdateState(item.SlotState, force);
	}

	public void UpdateState(CItem.EItemSlotState state, bool force = false)
	{
		if ((force || state != lastState) && cardEffects != null)
		{
			switch (state)
			{
			case CItem.EItemSlotState.Consumed:
				cardEffects.ToggleEffect(active: true, ItemCardEffects.FXTask.Consumed);
				break;
			case CItem.EItemSlotState.Spent:
				cardEffects.ToggleEffect(active: true, ItemCardEffects.FXTask.Spent);
				break;
			default:
				if (force || lastState == CItem.EItemSlotState.Consumed || lastState == CItem.EItemSlotState.Spent)
				{
					cardEffects.RestoreCard();
				}
				break;
			}
		}
		lastState = state;
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

	public void ResetItemElements()
	{
		foreach (InfuseElement infuseElement in infuseElements)
		{
			infuseElement.ResetElementToInitial();
		}
	}

	protected override void OnLanguageChanged()
	{
		if (CardName != null)
		{
			titleText.text = LocalizationManager.GetTranslation(CardName);
		}
	}

	public void ReloadBackground()
	{
		LoadBackground();
	}

	private void LoadBackground()
	{
		if (item.ID != 0)
		{
			ReferenceToSprite itemBackgroundSprite = UIInfoTools.Instance.GetItemBackgroundSprite(item.YMLData.Art);
			_imageAddressableLoader.LoadAsync(this, cardBackground, itemBackgroundSprite.SpriteReference);
			if (_referenceOnIcon != null)
			{
				_imageAddressableLoader.LoadAsync(this, validOwnerIcon, _referenceOnIcon.SpriteReference);
			}
			else if (!item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
			{
				_referenceOnIcon = UIInfoTools.Instance.GetCharacterAssemblyIcon(item.YMLData.ValidEquipCharacterClassIDs.FirstOrDefault());
				_imageAddressableLoader.LoadAsync(this, validOwnerIcon, _referenceOnIcon.SpriteReference);
			}
			_imageLoaded = true;
		}
	}

	private void UnloadBackground()
	{
		if (_imageLoaded)
		{
			_imageAddressableLoader.UnloadAll();
			_imageLoaded = false;
		}
		if (InputManager.GamePadInUse)
		{
			_allHintsCardTooltip.HideTooltip();
		}
		_cardActionHighlight?.Hide();
		if ((bool)_highlight)
		{
			_highlight.enabled = false;
		}
	}
}
