using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterCreatorClassRosterSlot : MonoBehaviour
{
	public class CharacterClassRosterEvent : UnityEvent<ICharacterCreatorClass>
	{
	}

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private Image selected;

	[SerializeField]
	private Image classIcon;

	[SerializeField]
	private UINewNotificationTip newNotification;

	private ICharacterCreatorClass character;

	[Header("Hover")]
	[SerializeField]
	private RectTransform animationRect;

	[SerializeField]
	private Vector2 hoverFactor = Vector2.one;

	private Vector2 portraitPivot;

	private Vector2 anchorsPortraitMax;

	private Vector2 anchorsPortraitMin;

	private Vector2 rectSize;

	public CharacterClassRosterEvent OnCharacterSelected = new CharacterClassRosterEvent();

	public CharacterClassRosterEvent OnCharacterHover = new CharacterClassRosterEvent();

	public CharacterClassRosterEvent OnCharacterUnhover = new CharacterClassRosterEvent();

	private Canvas canvas;

	private int sortingOrder;

	private bool isHovered;

	private SimpleKeyActionHandlerBlocker _focusHandlerBlocker;

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
			button.onClick.AddListener(Select);
		}
		_focusHandlerBlocker = new SimpleKeyActionHandlerBlocker();
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Select).AddBlocker(_focusHandlerBlocker).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(button)).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
		Singleton<KeyActionHandlerController>.Instance?.RemoveHandler(KeyAction.UI_SUBMIT, Select);
	}

	public void Init(ICharacterCreatorClass characterOption, bool selected = false, bool isNew = false, bool interactable = true)
	{
		character = characterOption;
		Decorate(characterOption);
		isHovered = false;
		ClearEvents();
		SetSelected(selected);
		SetInteractable(interactable);
		if (isNew)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
	}

	private void Decorate(ICharacterCreatorClass character)
	{
		classIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(character.Data);
		portrait.sprite = UIInfoTools.Instance.GetCharacterAssemblySprite(character.Data.Model);
		portrait.rectTransform.anchoredPosition = UIInfoTools.Instance.GetCharacterConfigUI(character.Data.Model).rosterPortraitOffset;
	}

	public void SetSelected(bool isSelected)
	{
		selected.enabled = isSelected;
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	private void Select()
	{
		OnCharacterSelected?.Invoke(character);
	}

	public void OnPointerEnter()
	{
		if (button.interactable)
		{
			isHovered = true;
			Vector2 size = portrait.rectTransform.rect.size;
			animationRect.sizeDelta = rectSize + hoverFactor;
			portrait.rectTransform.pivot = new Vector2(portraitPivot.x, (portrait.rectTransform.anchorMax.y + portrait.rectTransform.anchorMin.y) / 2f);
			RectTransform rectTransform = portrait.rectTransform;
			Vector2 anchorMax = (portrait.rectTransform.anchorMin = new Vector2(portrait.rectTransform.anchorMax.x, 0.5f));
			rectTransform.anchorMax = anchorMax;
			portrait.rectTransform.sizeDelta = size;
			canvas.sortingOrder = sortingOrder + 1;
			newNotification.Hide();
			OnCharacterHover?.Invoke(character);
		}
	}

	public void OnPointerExit()
	{
		isHovered = false;
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
	}

	public void ClearEvents()
	{
		OnCharacterSelected.RemoveAllListeners();
		OnCharacterUnhover.RemoveAllListeners();
		OnCharacterHover.RemoveAllListeners();
	}

	public void OnParentFocused()
	{
		_focusHandlerBlocker.SetBlock(value: false);
	}

	public void OnParentUnfocused()
	{
		_focusHandlerBlocker.SetBlock(value: true);
	}
}
