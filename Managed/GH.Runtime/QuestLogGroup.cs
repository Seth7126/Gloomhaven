using System;

public class QuestLogGroup
{
	public Enum Id { get; private set; }

	public bool IsNewNotificationEnabled { get; private set; }

	public bool IsGroupExpanded { get; private set; }

	public Action<bool> OnHovered { get; private set; }

	public Action<bool> OnExpanded { get; private set; }

	public QuestLogGroup(Enum id, bool isExpanded, bool isNewNotificationEnabled, Action<bool> onHovered = null, Action<bool> onExpanded = null)
	{
		Id = id;
		IsGroupExpanded = isExpanded;
		IsNewNotificationEnabled = isNewNotificationEnabled;
		OnHovered = onHovered;
		OnExpanded = onExpanded;
	}
}
