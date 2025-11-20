using System;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using I2.Loc;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestLogSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private GameObject questMarker;

	[SerializeField]
	private Image questIcon;

	[SerializeField]
	private Image questHighlight;

	[SerializeField]
	private UINewNotificationTip newNotification;

	[SerializeField]
	private Image highlight;

	[SerializeField]
	private CanvasGroup highlightCanvasGroup;

	[SerializeField]
	private Image selectedMask;

	[SerializeField]
	private Color normalHiglightColor;

	[SerializeField]
	private Color lockedHighlightColor;

	[Header("Information")]
	[SerializeField]
	private TextMeshProUGUI questText;

	[SerializeField]
	private float indent = 10f;

	[SerializeField]
	private float indentStory = 40f;

	[SerializeField]
	private float _consoleIndent = 15f;

	[SerializeField]
	private float _consoleIndentStory = 45f;

	[SerializeField]
	private Color informationColor;

	[SerializeField]
	private Color lockedInformationColor;

	[SerializeField]
	private string _cityQuestTitleKey = "CONSOLES/GUI_CITY_QUEST_TITLE";

	[SerializeField]
	private string _cityQuestDescrKey = "CONSOLES/GUI_CITY_QUEST_DESCR";

	[Header("Title")]
	[SerializeField]
	private Color mediumDifficultyColor;

	[SerializeField]
	private Color lockedTitleColor;

	private CQuestState questState;

	private Action<CQuestState, bool> onHovered;

	private Action<CQuestState> onClicked;

	private Action onSelected;

	private bool isSelected;

	private bool isHighlighted;

	private string title;

	private string localisedNameKey;

	private string localisedListDescriptionKey;

	private Color titleNormalColor;

	private string _currentColor;

	public CQuestState Quest => questState;

	public bool IsHighlighted => isHighlighted;

	public bool IsFocused
	{
		get
		{
			if (!isSelected)
			{
				return isHighlighted;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		button.onSelected.AddListener(OnSelected);
		button.onMouseEnter.AddListener(OnHovered);
		button.onMouseExit.AddListener(OnUnhovered);
		button.onClick.AddListener(Select);
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDisable()
	{
		button.onSelected.RemoveListener(OnSelected);
		button.onMouseEnter.RemoveListener(OnHovered);
		button.onMouseExit.RemoveListener(OnUnhovered);
		button.onClick.RemoveListener(Select);
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	public void SetFakeQuest(Action onClicked)
	{
		onHovered = null;
		this.onClicked = delegate
		{
			onClicked();
		};
		onSelected = delegate
		{
			newNotification.Hide();
		};
		titleNormalColor = mediumDifficultyColor;
		SetFakeTitle();
		questIcon.sprite = null;
		questHighlight.sprite = UIInfoTools.Instance.GetQuestMarkerHighlightSprite();
		questMarker.SetActive(value: false);
		SetNormalState();
		newNotification.Show();
		SetHighlighted(highlighted: false);
		SetSelected(selected: false);
	}

	private void SetFakeTitle()
	{
		title = $"<indent={_consoleIndent}><color=#{{0}}>{GLOOM.LocalizationManager.GetTranslation(_cityQuestTitleKey)}: </color>{GLOOM.LocalizationManager.GetTranslation(_cityQuestDescrKey)}";
	}

	public void SetQuest(CQuestState questState, Action<CQuestState, bool> onHovered, Action<CQuestState> onClicked)
	{
		this.questState = questState;
		this.onHovered = onHovered;
		this.onClicked = onClicked;
		onSelected = null;
		SetQuest(questState);
		RefreshLockState();
		RefreshNotification();
		SetHighlighted(highlighted: false);
		SetSelected(selected: false);
	}

	private void SetQuest(CQuestState questState)
	{
		this.questState = questState;
		titleNormalColor = GetTitleColor();
		localisedNameKey = questState.Quest.LocalisedNameKey;
		localisedListDescriptionKey = questState.Quest.LocalisedListDescriptionKey;
		if ((questState.Quest.Type == EQuestType.Story || questState.Quest.Type == EQuestType.City || questState.Quest.Type == EQuestType.CityAdjacent) && !AdventureState.MapState.IsCampaign && questState.QuestState < CQuestState.EQuestState.Completed)
		{
			title = $"<indent={(InputManager.GamePadInUse ? _consoleIndentStory : indentStory)}><color=#{{0}}>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedNameKey)}: </color>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedListDescriptionKey)}";
			questIcon.sprite = UIInfoTools.Instance.GetQuestMarkerSprite(questState);
			questHighlight.sprite = UIInfoTools.Instance.GetQuestMarkerHighlightSprite(questState);
			questMarker.SetActive(value: true);
		}
		else
		{
			title = $"<indent={(InputManager.GamePadInUse ? _consoleIndent : (AdventureState.MapState.IsCampaign ? 0f : indent))}><color=#{{0}}>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedNameKey)}: </color>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedListDescriptionKey)}";
			questMarker.SetActive(value: false);
		}
	}

	public void RefreshLockState()
	{
		RequirementCheckResult requirementCheckResult = questState.CheckRequirements();
		if (!requirementCheckResult.IsUnlocked() && !requirementCheckResult.IsOnlyMissingCharacters() && !Singleton<MapChoreographer>.Instance.ShowAllScenariosMode)
		{
			SetLockState();
		}
		else
		{
			SetNormalState();
		}
	}

	private void SetLockState()
	{
		highlight.color = lockedHighlightColor;
		_currentColor = lockedTitleColor.ToHex();
		UpdateQuestText();
		questText.color = lockedInformationColor;
		questIcon.material = UIInfoTools.Instance.greyedOutMaterial;
	}

	private void SetNormalState()
	{
		questIcon.material = null;
		highlight.color = normalHiglightColor;
		_currentColor = normalHiglightColor.ToHex();
		UpdateQuestText();
		questText.color = informationColor;
	}

	private void UpdateQuestText()
	{
		questText.text = string.Format(title, _currentColor);
	}

	private Color GetTitleColor()
	{
		if (questState.QuestState == CQuestState.EQuestState.Locked)
		{
			return lockedTitleColor;
		}
		return mediumDifficultyColor;
	}

	public void RefreshNotification()
	{
		if (questState.IsNew)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
	}

	public void Highlight()
	{
		SetHighlighted(highlighted: true);
	}

	public void UnHighlight()
	{
		SetHighlighted(highlighted: false);
	}

	private void SetHighlighted(bool highlighted)
	{
		isHighlighted = highlighted;
		selectedMask.SetAlpha(isSelected ? 1 : 0);
		highlightCanvasGroup.alpha = ((highlighted || isSelected) ? 1 : 0);
	}

	private void Select()
	{
		if (!isSelected)
		{
			onClicked?.Invoke(questState);
		}
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		highlightCanvasGroup.alpha = ((isHighlighted || isSelected) ? 1 : 0);
	}

	private void OnHovered()
	{
		Highlight();
		onHovered?.Invoke(questState, arg2: true);
	}

	private void OnUnhovered()
	{
		UnHighlight();
		onHovered?.Invoke(questState, arg2: false);
	}

	private void OnSelected()
	{
		onSelected?.Invoke();
	}

	private void OnLanguageChanged()
	{
		if (questState == null || questState.Quest == null)
		{
			SetFakeTitle();
		}
		else if ((questState.Quest.Type == EQuestType.Story || questState.Quest.Type == EQuestType.City || questState.Quest.Type == EQuestType.CityAdjacent) && !AdventureState.MapState.IsCampaign && questState.QuestState < CQuestState.EQuestState.Completed)
		{
			title = $"<indent={(InputManager.GamePadInUse ? _consoleIndentStory : indentStory)}><color=#{{0}}>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedNameKey)}: </color>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedListDescriptionKey)}";
		}
		else
		{
			title = $"<indent={(InputManager.GamePadInUse ? _consoleIndent : (AdventureState.MapState.IsCampaign ? 0f : indent))}><color=#{{0}}>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedNameKey)}: </color>{GLOOM.LocalizationManager.GetTranslation(questState.Quest.LocalisedListDescriptionKey)}";
		}
		UpdateQuestText();
	}
}
