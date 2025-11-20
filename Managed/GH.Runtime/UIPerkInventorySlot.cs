using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using SM.Gamepad;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPerkInventorySlot : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Transform holderPoints;

	[SerializeField]
	private UIPerkCheckbox pointPrefab;

	[SerializeField]
	private UIPerkModifiersTooltip tooltip;

	[SerializeField]
	private float alphaInactive = 0.4f;

	[SerializeField]
	private Color colorTitleActive;

	[Header("Hover")]
	[SerializeField]
	private float hoverFadeDuration = 0.3f;

	[SerializeField]
	protected Image background;

	[SerializeField]
	private Image _hoverOffset;

	private List<UIPerkCheckbox> pointsPool = new List<UIPerkCheckbox>();

	private List<CharacterPerk> perks;

	private Action<bool, UIPerkInventorySlot> onHoverCallback;

	private Action<UIPerkInventorySlot> onSelectedCallback;

	private LTDescr hoverAnimation;

	private UIPerkCheckbox previewedCheck;

	private Color defaultColor;

	protected ICharacterService character;

	public CharacterPerk Perk => perks[0];

	public int TotalCounters => perks.Count;

	public int ActiveCounters => perks.Count((CharacterPerk it) => it.IsActive);

	public UINavigationSelectable Selectable { get; private set; }

	public UIPerkModifiersTooltip Tooltip => tooltip;

	protected virtual void Awake()
	{
		Selectable = GetComponent<UINavigationSelectable>();
		defaultColor = title.color;
		if (InputManager.GamePadInUse)
		{
			InitGamepadInput();
		}
		else
		{
			button.onClick.AddListener(Select);
		}
	}

	protected void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			DisableGamepadInput();
		}
	}

	private void InitGamepadInput()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Select).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(button)).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Select);
		}
	}

	public virtual void Init(ICharacterService character, List<CharacterPerk> perks, Action<bool, UIPerkInventorySlot> onHoverCallback, Action<UIPerkInventorySlot> onSelectedCallback, bool isInteractable = true)
	{
		this.character = character;
		this.perks = perks;
		this.onHoverCallback = onHoverCallback;
		this.onSelectedCallback = onSelectedCallback;
		title.text = LocalizationManager.GetTranslation(perks[0].Perk.Name);
		description.text = LocalizationManager.GetTranslation(perks[0].Perk.Description);
		Tooltip.Initialize(perks[0]);
		background.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(character.CharacterModel, highlighted: true, character.Class.CustomCharacterConfig);
		HelperTools.NormalizePool(ref pointsPool, pointPrefab.gameObject, holderPoints, perks.Count);
		RefreshCounters();
		PreviewSelect(preview: false);
		if (FFSNetwork.IsOnline)
		{
			button.interactable = character.IsUnderMyControl && isInteractable;
		}
		else
		{
			button.interactable = isInteractable;
		}
		if (!InputManager.GamePadInUse)
		{
			ActiveBackground(active: true);
		}
	}

	public virtual void OnHover(bool hovered)
	{
		onHoverCallback(hovered, this);
		PreviewSelect(hovered);
		if (InputManager.GamePadInUse)
		{
			_hoverOffset.gameObject.SetActive(hovered);
		}
		if (hoverAnimation != null)
		{
			LeanTween.cancel(hoverAnimation.id);
		}
	}

	protected virtual void PreviewSelect(bool preview)
	{
		if (preview)
		{
			previewedCheck = pointsPool.Take(perks.Count).FirstOrDefault((UIPerkCheckbox it) => !it.isOn);
			if (previewedCheck != null)
			{
				previewedCheck.Highlight(highlight: true);
			}
		}
		else if (previewedCheck != null)
		{
			previewedCheck.Highlight(highlight: false);
			previewedCheck = null;
		}
	}

	private void Select()
	{
		if (!LevelMessageUILayoutGroup.IsShown)
		{
			if (perks.FirstOrDefault((CharacterPerk it) => !it.IsActive) != null)
			{
				onSelectedCallback(this);
			}
			else
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			}
		}
	}

	public virtual void RefreshCounters()
	{
		bool flag = character.PerkPoints > 0;
		for (int i = 0; i < perks.Count; i++)
		{
			pointsPool[i].isOn = perks[i].IsActive;
			flag |= perks[i].IsActive;
		}
		canvasGroup.alpha = (flag ? 1f : alphaInactive);
		title.color = (flag ? colorTitleActive : defaultColor);
		if (previewedCheck != null)
		{
			PreviewSelect(preview: true);
		}
	}

	protected virtual void OnDisable()
	{
		DisableNavigation();
		ActiveBackground(active: false);
		if (hoverAnimation != null)
		{
			LeanTween.cancel(hoverAnimation.id);
			hoverAnimation = null;
		}
	}

	public void EnableNavigation()
	{
		if (!InputManager.GamePadInUse && button != null)
		{
			button.SetNavigation(Navigation.Mode.Vertical);
		}
	}

	public void DisableNavigation()
	{
		if (!InputManager.GamePadInUse && button != null)
		{
			button.DisableNavigation();
		}
	}

	public void ActiveBackground(bool active)
	{
		if (InputManager.GamePadInUse)
		{
			background.enabled = active;
		}
		else
		{
			background.enabled = true;
		}
	}
}
