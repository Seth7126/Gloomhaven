using System.Collections.Generic;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIAdventurePartyAssemblyRosterSlot : MonoBehaviour
{
	public class PartyRosterEvent : UnityEvent<CMapCharacter>
	{
	}

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private Image selected;

	[SerializeField]
	private Image newSelected;

	[SerializeField]
	private Image classIcon;

	[SerializeField]
	private GameObject removePanel;

	[SerializeField]
	private Image lockedIcon;

	[SerializeField]
	private string audioItemSelect;

	[SerializeField]
	private string audioItemUnselect;

	[SerializeField]
	private List<UICharacterInformation> extraInformations;

	private CMapCharacter character;

	[Header("Hover")]
	[SerializeField]
	private RectTransform animationRect;

	[SerializeField]
	private Vector2 hoverFactor = Vector2.one;

	private Vector2 portraitPivot;

	private Vector2 anchorsPortraitMax;

	private Vector2 anchorsPortraitMin;

	private Vector2 rectSize;

	private ScrollRect _scrollRect;

	public PartyRosterEvent OnCharacterSelected = new PartyRosterEvent();

	public PartyRosterEvent OnCharacterDeselected = new PartyRosterEvent();

	public PartyRosterEvent OnCharacterHover = new PartyRosterEvent();

	public PartyRosterEvent OnCharacterUnhover = new PartyRosterEvent();

	private Canvas canvas;

	private int sortingOrder;

	private bool isSelected;

	private IUiNavigationSelectable selectable;

	private bool isHovered;

	private SimpleKeyActionHandlerBlocker _focusHandlerBlocker;

	private UITextTooltipTarget tooltipTarget;

	private HashSet<Component> disableInteractionRequests = new HashSet<Component>();

	public CMapCharacter Character => character;

	public bool IsLocked { get; private set; }

	public bool IsInteractable => button.IsInteractable();

	public IUiNavigationSelectable Selectable
	{
		get
		{
			if (selectable == null)
			{
				selectable = GetComponent<IUiNavigationSelectable>();
			}
			return selectable;
		}
	}

	public bool IsSelected => isSelected;

	private void Awake()
	{
		canvas = animationRect.GetComponent<Canvas>();
		sortingOrder = canvas.sortingOrder;
		anchorsPortraitMax = portrait.rectTransform.anchorMax;
		anchorsPortraitMin = portrait.rectTransform.anchorMin;
		rectSize = animationRect.sizeDelta;
		portraitPivot = portrait.rectTransform.pivot;
		button.onMouseEnter.AddListener(OnPointerEnter);
		button.onMouseExit.AddListener(OnPointerExit);
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(Click);
		}
		_focusHandlerBlocker = new SimpleKeyActionHandlerBlocker();
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Click).AddBlocker(_focusHandlerBlocker).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(button)).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
		tooltipTarget = GetComponent<UITextTooltipTarget>();
	}

	public void Init(CMapCharacter characterOption, bool selected = false, bool locked = false, ScrollRect scrollRect = null)
	{
		_scrollRect = scrollRect;
		character = characterOption;
		classIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData);
		portrait.sprite = UIInfoTools.Instance.GetCharacterAssemblySprite(character.CharacterYMLData.Model, characterOption.CharacterYMLData.CustomCharacterConfig);
		portrait.rectTransform.anchoredPosition = UIInfoTools.Instance.GetCharacterConfigUI(Character.CharacterYMLData.Model).rosterPortraitOffset;
		isHovered = false;
		if (newSelected != null)
		{
			Color characterColor = UIInfoTools.Instance.GetCharacterColor(Character.CharacterYMLData.Model, Character.CharacterYMLData.CustomCharacterConfig);
			newSelected.color = characterColor;
		}
		foreach (UICharacterInformation extraInformation in extraInformations)
		{
			extraInformation.Display(characterOption);
		}
		ClearEvents();
		SetLocked(locked);
		SetSelected(selected);
		if (removePanel != null)
		{
			removePanel.SetActive(value: false);
		}
		button.interactable = true;
		disableInteractionRequests.Clear();
	}

	private void OnDestroy()
	{
		Singleton<KeyActionHandlerController>.Instance?.RemoveHandler(KeyAction.UI_SUBMIT, Click);
		if (!InputManager.GamePadInUse)
		{
			button.onClick.RemoveAllListeners();
		}
	}

	public void SetSelected(bool isSelected)
	{
		this.isSelected = isSelected;
		if (!InputManager.GamePadInUse)
		{
			selected.enabled = isSelected;
		}
		else
		{
			newSelected.enabled = isSelected;
			if (!isHovered)
			{
				selected.gameObject.SetActive(value: false);
			}
		}
		RefreshRemove();
	}

	private void RefreshRemove()
	{
		if (removePanel != null)
		{
			removePanel.SetActive(IsSelected && isHovered && !IsLocked);
			if (IsSelected && newSelected != null)
			{
				newSelected.enabled = !IsSelected || !isHovered || IsLocked;
			}
		}
	}

	public void DisableOption(bool disable, Component request = null)
	{
		if (disable)
		{
			disableInteractionRequests.Add((request == null) ? this : request);
		}
		else
		{
			disableInteractionRequests.Remove((request == null) ? this : request);
		}
		button.interactable = disableInteractionRequests.Count == 0;
		Image image = portrait;
		Color color = (classIcon.color = (button.interactable ? Color.white : Color.gray));
		image.color = color;
	}

	public void DetermineInteractability(CMapCharacter currentlySlottedCharacter)
	{
		NetworkPlayer controller = ControllableRegistry.GetController((int)Character.CharacterYMLData.Model);
		bool flag = !FFSNetwork.IsOnline || currentlySlottedCharacter == Character || ((controller.In(null, PlayerRegistry.MyPlayer) || AdventureState.MapState.MapParty.UnselectedCharacters.Contains(character)) && (currentlySlottedCharacter?.IsUnderMyControl ?? true));
		if (FFSNetwork.IsOnline && (PlayerRegistry.IsSwitchingCharacter || UIAdventurePartyAssemblyWindow.IsChangingCharacter))
		{
			flag = false;
		}
		bool flag2 = !FFSNetwork.IsOnline || (FFSNetwork.IsHost && (ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || PlayerRegistry.JoiningPlayers.Count == 0)) || (FFSNetwork.IsClient && (ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || !PlayerRegistry.OtherClientsAreJoining));
		if (flag2)
		{
			tooltipTarget.TooltipEnabled = false;
			tooltipTarget.HideTooltip(0f);
		}
		else
		{
			tooltipTarget.TooltipEnabled = true;
		}
		flag = flag && flag2;
		DisableOption(!flag);
	}

	public void SetLocked(bool isLocked, bool showLockImage = false)
	{
		IsLocked = isLocked;
		lockedIcon.enabled = isLocked && showLockImage;
		RefreshRemove();
	}

	private void Click()
	{
		if (IsLocked)
		{
			OnCharacterSelected?.Invoke(character);
		}
		else if (!IsSelected)
		{
			AudioControllerUtils.PlaySound(audioItemSelect);
			OnCharacterSelected?.Invoke(character);
		}
		else
		{
			AudioControllerUtils.PlaySound(audioItemUnselect);
			OnCharacterDeselected?.Invoke(character);
		}
	}

	public void OnPointerEnter()
	{
		if (!button.interactable)
		{
			return;
		}
		isHovered = true;
		Vector2 size = portrait.rectTransform.rect.size;
		animationRect.sizeDelta = rectSize + hoverFactor;
		portrait.rectTransform.pivot = new Vector2(portraitPivot.x, (portrait.rectTransform.anchorMax.y + portrait.rectTransform.anchorMin.y) / 2f);
		RectTransform rectTransform = portrait.rectTransform;
		Vector2 anchorMax = (portrait.rectTransform.anchorMin = new Vector2(portrait.rectTransform.anchorMax.x, 0.5f));
		rectTransform.anchorMax = anchorMax;
		portrait.rectTransform.sizeDelta = size;
		canvas.sortingOrder = sortingOrder + 1;
		RefreshRemove();
		if (InputManager.GamePadInUse)
		{
			selected.gameObject.SetActive(value: true);
			if (IsSelected)
			{
				selected.transform.GetChild(0).gameObject.SetActive(value: false);
			}
		}
		if (_scrollRect != null)
		{
			_scrollRect.ScrollToFit(base.gameObject.transform as RectTransform);
		}
		OnCharacterHover?.Invoke(character);
	}

	public void OnPointerExit()
	{
		isHovered = false;
		if (InputManager.GamePadInUse)
		{
			selected.gameObject.SetActive(value: false);
			if (IsSelected)
			{
				selected.transform.GetChild(0).gameObject.SetActive(value: true);
			}
		}
		Unhighlight();
		OnCharacterUnhover?.Invoke(character);
	}

	private void Unhighlight()
	{
		animationRect.sizeDelta = rectSize;
		portrait.rectTransform.pivot = portraitPivot;
		portrait.rectTransform.anchorMax = anchorsPortraitMax;
		portrait.rectTransform.anchorMin = anchorsPortraitMin;
		portrait.rectTransform.sizeDelta = Vector2.zero;
		canvas.sortingOrder = sortingOrder;
		RefreshRemove();
	}

	public void ClearEvents()
	{
		OnCharacterSelected.RemoveAllListeners();
		OnCharacterUnhover.RemoveAllListeners();
		OnCharacterHover.RemoveAllListeners();
		OnCharacterDeselected.RemoveAllListeners();
	}

	public void OnFocus()
	{
		_focusHandlerBlocker.SetBlock(value: false);
	}

	public void OnUnfocus()
	{
		_focusHandlerBlocker.SetBlock(value: true);
	}
}
