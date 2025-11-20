using GLOOM;
using I2.Loc;
using MapRuleLibrary.YML.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAchievementFilter : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI filterText;

	[SerializeField]
	private Color interactionDisabledColor;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private UINewNotificationTip newNotification;

	public UITab tab;

	public EAchievementType filter;

	private int total;

	private int completed;

	private string title;

	private Color? defaultFilterTextColor;

	private void Awake()
	{
		title = GLOOM.LocalizationManager.GetTranslation($"GUI_ACHIEVEMENT_FILTER_{filter}");
		if (!defaultFilterTextColor.HasValue)
		{
			defaultFilterTextColor = filterText.color;
		}
		tab.onValueChanged.AddListener(OnValueChanged);
		void OnValueChanged(bool value)
		{
			if (value)
			{
				tab.OnPointerEnter(new PointerEventData(EventSystem.current));
			}
			else
			{
				tab.OnPointerExit(new PointerEventData(EventSystem.current));
			}
		}
	}

	private void OnEnable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDisable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	public void Init(int total, int completed, bool showNewNotification = false)
	{
		this.total = total;
		this.completed = completed;
		if (!defaultFilterTextColor.HasValue)
		{
			defaultFilterTextColor = filterText.color;
		}
		SetInteractable(interactable: true);
		Refresh();
		ShowNewNotification(showNewNotification);
	}

	public void IncreaseCompleted()
	{
		completed++;
		Refresh();
	}

	private void Refresh()
	{
		filterText.text = $"{completed}/{total}\n<size=-2>{title}";
		if (total <= 0)
		{
			tab.interactable = false;
			icon.color = interactionDisabledColor;
		}
		else
		{
			icon.color = Color.white;
			tab.interactable = true;
		}
		if (completed == total && total > 0)
		{
			filterText.color = UIInfoTools.Instance.achievementCompletedColor;
		}
		else if (defaultFilterTextColor.HasValue)
		{
			filterText.color = defaultFilterTextColor.Value;
		}
	}

	public void SetInteractable(bool interactable)
	{
		icon.CrossFadeColor(interactable ? Color.white : interactionDisabledColor, 0f, ignoreTimeScale: true, useAlpha: false);
	}

	public void ShowNewNotification(bool show)
	{
		if (show)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
	}

	private void OnLanguageChanged()
	{
		title = GLOOM.LocalizationManager.GetTranslation($"GUI_ACHIEVEMENT_FILTER_{filter}");
		filterText.text = $"{completed}/{total}\n<size=-2>{title}";
	}
}
