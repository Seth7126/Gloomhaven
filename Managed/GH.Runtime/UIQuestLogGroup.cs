using System;
using System.Collections.Generic;
using AsmodeeNet.Utils.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestLogGroup : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener title;

	[SerializeField]
	private Color highlightedTitleColor;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	private ExtendedButton hoverButton;

	public RectTransform questContainer;

	[SerializeField]
	private UINewNotificationTip newNotification;

	public List<UIQuestLogSlot> Slots;

	private bool isExpanded;

	private bool enableNewNotification;

	private Color defaultTilteColor;

	private QuestLogGroup group;

	private Action<bool> onExpanded;

	private Action<bool> onHovered;

	public bool IsExpanded => isExpanded;

	private void Awake()
	{
		defaultTilteColor = title.Text.color;
		hoverButton.onMouseEnter.AddListener(OnSelected);
		hoverButton.onMouseExit.AddListener(OnDeselected);
	}

	private void OnSelected()
	{
		InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, Toggle);
		OnHovered(hovered: true);
	}

	private void OnDeselected()
	{
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, Toggle);
		OnHovered(hovered: false);
	}

	public void SetQuestGroup(QuestLogGroup group, Action<bool> onHovered = null, Action<bool> onExpanded = null)
	{
		this.group = group;
		enableNewNotification = group.IsNewNotificationEnabled;
		this.onHovered = ((onHovered == null) ? group.OnHovered : ((Action<bool>)delegate(bool hovered)
		{
			onHovered(hovered);
			group.OnHovered?.Invoke(hovered);
		}));
		this.onExpanded = delegate(bool expanded)
		{
			onExpanded?.Invoke(expanded);
			group.OnExpanded?.Invoke(expanded);
		};
		title.SetTextKey($"GUI_QUEST_GROUP_{group.Id}");
		icon.sprite = UIInfoTools.Instance.GetQuestGroupIcon(group.Id);
		if (group.IsGroupExpanded)
		{
			Expand();
		}
		else
		{
			Srink();
		}
		Refresh();
	}

	public void Expand()
	{
		isExpanded = true;
		newNotification.Hide();
		amountText.enabled = false;
		if (Slots != null)
		{
			for (int i = 0; i < Slots.Count; i++)
			{
				Slots[i].gameObject.SetActive(value: true);
			}
		}
		onExpanded?.Invoke(obj: true);
	}

	public void Srink()
	{
		isExpanded = false;
		amountText.enabled = true;
		if (Slots != null)
		{
			for (int i = 0; i < Slots.Count; i++)
			{
				if (!Slots[i].IsFocused)
				{
					Slots[i].gameObject.SetActive(value: false);
				}
			}
		}
		ShowNewNotification(enableNewNotification && Slots != null && Slots.Exists((UIQuestLogSlot it) => it.Quest.IsNew));
		onExpanded?.Invoke(obj: false);
	}

	public void Toggle()
	{
		if (isExpanded)
		{
			Srink();
		}
		else
		{
			Expand();
		}
	}

	private void ShowNewNotification(bool show)
	{
		if (show && enableNewNotification)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
	}

	public void AddFakeSlot(UIQuestLogSlot slot)
	{
		if (Slots == null)
		{
			Slots = new List<UIQuestLogSlot>();
		}
		if (!Slots.Contains(slot))
		{
			if (slot.transform.parent != questContainer)
			{
				slot.transform.SetParent(questContainer);
			}
			slot.transform.SetAsFirstSibling();
			Expand();
			Slots.Add(slot);
		}
		Refresh();
		slot.gameObject.SetActive(isExpanded || slot.IsFocused);
	}

	public void AddSlot(UIQuestLogSlot slot)
	{
		if (Slots == null)
		{
			Slots = new List<UIQuestLogSlot>();
		}
		if (slot.transform.parent != questContainer)
		{
			slot.transform.SetParent(questContainer);
		}
		if (slot.Quest.IsNew)
		{
			slot.transform.SetAsFirstSibling();
		}
		Slots.Add(slot);
		Refresh();
		slot.gameObject.SetActive(isExpanded || slot.IsFocused);
	}

	public void RemoveSlot(UIQuestLogSlot slot)
	{
		if (Slots != null && Slots.Remove(slot))
		{
			Refresh();
		}
	}

	public void RemoveFirstSlot()
	{
		if (Slots != null && Slots.Count > 0)
		{
			Slots.RemoveFirst();
			Refresh();
		}
	}

	public bool HasSlot(UIQuestLogSlot slot)
	{
		if (Slots != null)
		{
			return Slots.Contains(slot);
		}
		return false;
	}

	public void Refresh()
	{
		ShowNewNotification(!isExpanded && enableNewNotification && Slots != null && Slots.Exists((UIQuestLogSlot it) => it.Quest.IsNew));
		base.gameObject.SetActive(Slots != null && Slots.Count > 0);
		amountText.text = Slots?.Count.ToString() ?? string.Empty;
	}

	private void OnHovered(bool hovered)
	{
		title.Text.color = (hovered ? highlightedTitleColor : defaultTilteColor);
		onHovered?.Invoke(hovered);
	}

	public void SetHighlighted(bool highlight)
	{
		Action<bool> action = onHovered;
		onHovered = null;
		if (highlight)
		{
			hoverButton.OnPointerEnter(null);
		}
		else
		{
			hoverButton.OnPointerExit(null);
		}
		onHovered = action;
	}

	private void OnDestroy()
	{
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, Toggle);
	}
}
