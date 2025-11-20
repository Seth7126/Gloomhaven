using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using GLOOM;
using JetBrains.Annotations;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.BattleGoals;
using SM.Gamepad;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleGoalPickerSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private TextLocalizedListener title;

	[Header("Goal")]
	[SerializeField]
	private TextMeshProUGUI goal;

	[SerializeField]
	[TextArea]
	private string goalFormat = "{0}";

	[Header("Reward")]
	[SerializeField]
	protected List<UIQuestReward> rewardPool;

	[Header("Assigned")]
	[SerializeField]
	private Image assignedCharacterShield;

	[SerializeField]
	private GUIAnimator assignAnimator;

	[SerializeField]
	private GameObject assignedMask;

	[Header("Focus")]
	[SerializeField]
	protected List<Image> focusMasks;

	[SerializeField]
	private List<TextMeshProUGUI> focusTexts;

	[SerializeField]
	private Color unfocusedColorText;

	[SerializeField]
	private GameObject _dimmerElement;

	[Header("Higlight")]
	[SerializeField]
	private List<Graphic> highlightMasks;

	[SerializeField]
	private float defaultHighlightAlpha = 0.2f;

	[SerializeField]
	private float unhighlightAlpha = 0.5f;

	private CBattleGoalState battleGoal;

	private Action<bool, CBattleGoalState> onToggled;

	private Action<bool> onHovered;

	private Action onFinishedAssignAnimation;

	private bool isSelected;

	private List<Color> defaultTextColors;

	private bool _isHovered;

	private bool _isFocused;

	[field: SerializeField]
	public UINavigationSelectable UINavigationSelectable { get; private set; }

	public string Id
	{
		get
		{
			if (battleGoal == null)
			{
				return string.Empty;
			}
			return battleGoal.ID;
		}
	}

	public bool IsHovered => _isHovered;

	public bool IsSelected => isSelected;

	public static event Action SlotPicked;

	public event Action<bool> OnSelect;

	public event Action<bool> OnHover;

	public event Action OnFocus;

	[UsedImplicitly]
	private void Awake()
	{
		defaultTextColors = focusTexts.Select((TextMeshProUGUI it) => it.color).ToList();
		assignAnimator.OnAnimationFinished.AddListener(delegate
		{
			onFinishedAssignAnimation?.Invoke();
			UIBattleGoalPickerSlot.SlotPicked?.Invoke();
		});
		button.onClick.AddListener(ToggleFromButtonClick);
		button.onMouseEnter.AddListener(delegate
		{
			onHovered?.Invoke(obj: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			onHovered?.Invoke(obj: false);
		});
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.RemoveAllListeners();
			assignAnimator.OnAnimationFinished.RemoveAllListeners();
		}
		button.onMouseExit.RemoveAllListeners();
		button.onMouseExit.RemoveAllListeners();
	}

	public void SetBattleGoal(CBattleGoalState battleGoal, ICharacterService characterToAssign, Action<bool, CBattleGoalState> onToggled, Action<bool> onHovered)
	{
		this.battleGoal = battleGoal;
		this.onToggled = onToggled;
		this.onHovered = onHovered;
		assignAnimator.Stop(goToEnd: true);
		Decorate(battleGoal);
		assignedCharacterShield.sprite = UIInfoTools.Instance.GetCharacterMarker(characterToAssign.CharacterModel, characterToAssign.Class.CustomCharacterConfig);
		SetSelected(select: false);
		SetFocused(focused: true);
		ClearHighlight();
		button.interactable = !FFSNetwork.IsOnline || characterToAssign.IsUnderMyControl;
	}

	public void SetDimmer(bool isDimmer)
	{
		_dimmerElement?.SetActive(isDimmer);
	}

	private void Decorate(CBattleGoalState battleGoal)
	{
		Decorate(battleGoal.BattleGoal);
		List<Reward> list = battleGoal.Rewards.SelectMany((RewardGroup it) => it.Rewards).ToList();
		HelperTools.NormalizePool(ref rewardPool, rewardPool[0].gameObject, rewardPool[0].transform.parent, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			rewardPool[num].ShowReward(list[num]);
		}
	}

	private void Decorate(BattleGoalYMLData battleGoal)
	{
		title.SetTextKey(battleGoal.LocalisedName);
		goal.text = LocalizationManager.GetTranslation("GUI_RESULTS_OBJECTIVE") + ": " + string.Format(goalFormat, LocalizationManager.GetTranslation(battleGoal.LocalisedDescription));
	}

	private void SetSelected(bool select)
	{
		assignedMask.SetActive(select);
		isSelected = select;
	}

	public void Select()
	{
		if (!isSelected)
		{
			assignAnimator.Stop(goToEnd: true);
			SetSelected(select: true);
			this.OnSelect?.Invoke(obj: true);
			onToggled?.Invoke(arg1: true, battleGoal);
		}
	}

	public void HighlightAssign(Action onFinishedHighlight)
	{
		onFinishedAssignAnimation = onFinishedHighlight;
		assignAnimator.Play();
		if (InputManager.GamePadInUse)
		{
			assignAnimator.OnAnimationFinished?.Invoke();
		}
	}

	public void Deselect()
	{
		if (isSelected)
		{
			assignAnimator.Stop(goToEnd: true);
			SetSelected(select: false);
			this.OnSelect?.Invoke(obj: false);
			onToggled?.Invoke(arg1: false, battleGoal);
		}
	}

	public void SetFocused(bool focused)
	{
		_isFocused = focused;
		for (int i = 0; i < rewardPool.Count && rewardPool[i].gameObject.activeSelf; i++)
		{
			rewardPool[i].ShowUnlocked(focused);
		}
		for (int j = 0; j < focusTexts.Count; j++)
		{
			focusTexts[j].color = (focused ? defaultTextColors[j] : unfocusedColorText);
		}
		foreach (Image focusMask in focusMasks)
		{
			focusMask.material = (focused ? null : UIInfoTools.Instance.disabledGrayscaleMaterial);
		}
	}

	public void Highlight(bool on)
	{
		this.OnHover?.Invoke(on);
		_isHovered = on;
		foreach (Graphic highlightMask in highlightMasks)
		{
			highlightMask.SetAlpha(on ? 0f : unhighlightAlpha);
		}
	}

	public void ClearHighlight()
	{
		_isHovered = false;
		foreach (Graphic highlightMask in highlightMasks)
		{
			highlightMask.SetAlpha(defaultHighlightAlpha);
		}
	}

	private void ToggleFromButtonClick()
	{
		if (_isFocused && (!InputManager.GamePadInUse || !FFSNetwork.IsOnline))
		{
			Toggle();
		}
	}

	public void Toggle()
	{
		if (NewPartyDisplayUI.PartyDisplay.BattleGoalWindow.SelectionAllowed)
		{
			if (!isSelected)
			{
				Select();
			}
			else
			{
				Deselect();
			}
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			assignAnimator.Stop(goToEnd: true);
		}
	}

	public void OnFocused()
	{
		this.OnFocus?.Invoke();
	}
}
