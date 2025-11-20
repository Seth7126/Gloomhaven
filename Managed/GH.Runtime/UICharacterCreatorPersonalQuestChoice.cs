using System;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Party;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterCreatorPersonalQuestChoice : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private UIPersonalQuestState personalQuestState;

	[SerializeField]
	private ControllerInputElement[] controllerElements;

	[SerializeField]
	private ScrollRect m_Scroll;

	private Action<bool, UICharacterCreatorPersonalQuestChoice> onHovered;

	private Action<bool, UICharacterCreatorPersonalQuestChoice> onToggled;

	private bool isSelected;

	private bool isHovered;

	private Color defaultTitleColor;

	public Func<bool> IsQuestSelectedCallback;

	[SerializeField]
	protected TextLocalizedListener goalText;

	[SerializeField]
	protected Color goalColor;

	private bool isNavigationEnabled;

	public CPersonalQuestState PersonalQuest { get; private set; }

	public bool IsSelected => isSelected;

	public bool IsHovered => isHovered;

	public event Action<bool> OnToggle;

	public event Action<bool> OnHover;

	public event Action OnFocus;

	private void Awake()
	{
		button.onMouseEnter.AddListener(delegate
		{
			OnHovered(hovered: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			OnHovered(hovered: false);
		});
		button.onClick.AddListener(ToggleFromButtonClick);
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}

	public void SetPersonalQuest(CPersonalQuestState quest, Action<bool, UICharacterCreatorPersonalQuestChoice> onToggled, Action<bool, UICharacterCreatorPersonalQuestChoice> onHovered)
	{
		isSelected = false;
		isHovered = false;
		PersonalQuest = quest;
		this.onHovered = onHovered;
		this.onToggled = onToggled;
		if (quest.PersonalQuestSteps == 0)
		{
			goalText.SetFormat("<color=#" + goalColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_RESULTS_OBJECTIVE") + ":</color> {0}");
		}
		else
		{
			goalText.SetFormat(string.Format("<color=#{0}>{1} 1/{2}:</color> {{0}}", goalColor.ToHex(), LocalizationManager.GetTranslation("GUI_RESULTS_OBJECTIVE"), quest.PersonalQuestSteps));
		}
		personalQuestState.SetPersonalQuest(quest);
		Focus(focused: true);
		Highlight(highlight: false);
		if (m_Scroll != null)
		{
			m_Scroll.verticalNormalizedPosition = 1f;
		}
		DisableNavigation();
	}

	private void OnHovered(bool hovered)
	{
		isHovered = hovered;
		onHovered?.Invoke(hovered, this);
		this.OnHover?.Invoke(hovered);
		if (hovered)
		{
			EnableNavigation();
		}
		else
		{
			DisableNavigation();
		}
	}

	public void ToggleFromButtonClick()
	{
		if (!InputManager.GamePadInUse || !IsQuestSelectedCallback())
		{
			Toggle();
		}
	}

	public void Toggle()
	{
		if (isSelected)
		{
			Deselect();
		}
		else
		{
			Select();
		}
	}

	public void Select()
	{
		if (!isSelected)
		{
			this.OnToggle?.Invoke(obj: true);
			isSelected = true;
			onToggled?.Invoke(arg1: true, this);
		}
	}

	public void Deselect()
	{
		if (isSelected)
		{
			this.OnToggle?.Invoke(obj: false);
			isSelected = false;
			onToggled?.Invoke(arg1: false, this);
		}
	}

	public void Highlight(bool highlight)
	{
		personalQuestState.SetHighlighted(isHovered || highlight);
	}

	public void Focus(bool focused)
	{
		this.OnFocus?.Invoke();
		personalQuestState.RefreshConceal(!focused && !isSelected);
	}

	public void EnableNavigation()
	{
		isNavigationEnabled = true;
		ControllerInputElement[] array = controllerElements;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
	}

	public void DisableNavigation()
	{
		isNavigationEnabled = false;
		ControllerInputElement[] array = controllerElements;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
	}
}
