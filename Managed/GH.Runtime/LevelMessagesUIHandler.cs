#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

public class LevelMessagesUIHandler : MonoBehaviour
{
	[Serializable]
	public class MessageInfo
	{
		public Action OnClosedPressedAction;

		public CLevelMessage Message;

		public MessageInfo(CLevelMessage message, Action onClosedPressedAction)
		{
			Message = message;
			OnClosedPressedAction = onClosedPressedAction;
		}
	}

	public static LevelMessagesUIHandler s_Instance;

	public LevelMessageUILayoutGroup LevelMessageBoxLayoutGroup;

	public LevelMessageUILayoutGroup LevelMessageHelpTextLayoutGroup;

	public InteractabilityIsolatedUIControl TutorialBoxWindowIsolatedControl;

	private List<MessageInfo> m_PendingBoxMessages = new List<MessageInfo>();

	private List<MessageInfo> m_PendingHelpTextMessages = new List<MessageInfo>();

	private MessageInfo m_CurrentlyDisplayedBoxMessageInfo;

	private MessageInfo m_CurrentlyDisplayedHelpTextMessageInfo;

	public CLevelMessage CurrentlyDisplayedBoxMessage
	{
		get
		{
			if (m_CurrentlyDisplayedBoxMessageInfo != null)
			{
				return m_CurrentlyDisplayedBoxMessageInfo.Message;
			}
			return null;
		}
	}

	public CLevelMessage CurrentlyDisplayedHelpTextMessage
	{
		get
		{
			if (m_CurrentlyDisplayedHelpTextMessageInfo != null)
			{
				return m_CurrentlyDisplayedHelpTextMessageInfo.Message;
			}
			return null;
		}
	}

	public bool DisplayDelayInEffect { get; private set; }

	private void Awake()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		s_Instance = null;
	}

	private void Start()
	{
		LevelMessageBoxLayoutGroup.HideWindow();
		LevelMessageHelpTextLayoutGroup.HideWindow();
	}

	public void ShowBoxMessage(CLevelMessage message, Action onClosed = null)
	{
		MessageInfo messageInfo = new MessageInfo(message, delegate
		{
			HideCurrentlyShownBoxMessage();
			onClosed?.Invoke();
		});
		m_PendingBoxMessages.Add(messageInfo);
		if (m_PendingBoxMessages.Count == 1)
		{
			ShowBoxMessageImmediately(messageInfo);
			return;
		}
		Debug.LogWarning("Received Call to display BOX message [" + message.MessageName + "] while message [" + m_CurrentlyDisplayedBoxMessageInfo.Message.MessageName + "] already displaying");
	}

	public void ShowHelpText(CLevelMessage message, Action onClosed = null)
	{
		MessageInfo messageInfo = new MessageInfo(message, delegate
		{
			HideCurrentlyShownHelpTextMessage();
			onClosed?.Invoke();
		});
		m_PendingHelpTextMessages.Add(messageInfo);
		if (m_PendingHelpTextMessages.Count == 1)
		{
			ShowHelpTextImmediately(messageInfo);
			return;
		}
		Debug.LogWarning("Received Call to display HELPTEXT message [" + message.MessageName + "] while message [" + m_CurrentlyDisplayedHelpTextMessageInfo.Message.MessageName + "] already displaying");
	}

	private void ShowNextBoxMessage()
	{
		m_PendingBoxMessages.Remove(m_CurrentlyDisplayedBoxMessageInfo);
		if (m_PendingBoxMessages.Count > 0)
		{
			ShowBoxMessageImmediately(m_PendingBoxMessages[0]);
		}
		else
		{
			m_CurrentlyDisplayedBoxMessageInfo = null;
		}
	}

	private void ShowNextHelpText()
	{
		m_PendingHelpTextMessages.Remove(m_CurrentlyDisplayedHelpTextMessageInfo);
		if (m_PendingHelpTextMessages.Count > 0)
		{
			ShowHelpTextImmediately(m_PendingHelpTextMessages[0]);
		}
		else
		{
			m_CurrentlyDisplayedHelpTextMessageInfo = null;
		}
	}

	private void ShowBoxMessageImmediately(MessageInfo message)
	{
		m_CurrentlyDisplayedBoxMessageInfo = message;
		StartCoroutine(DisplayMessageInWindowAfterDelay(message, LevelMessageBoxLayoutGroup));
	}

	private void ShowHelpTextImmediately(MessageInfo message)
	{
		m_CurrentlyDisplayedHelpTextMessageInfo = message;
		StartCoroutine(DisplayMessageInWindowAfterDelay(message, LevelMessageHelpTextLayoutGroup));
	}

	public void HideCurrentlyShownBoxMessage()
	{
		if (m_CurrentlyDisplayedBoxMessageInfo != null)
		{
			LevelMessageBoxLayoutGroup.HideWindow();
			LevelEventsController.s_Instance.MessageWasDismissed(m_CurrentlyDisplayedBoxMessageInfo.Message);
		}
		ShowNextBoxMessage();
	}

	public void HideCurrentlyShownHelpTextMessage()
	{
		if (m_CurrentlyDisplayedHelpTextMessageInfo != null)
		{
			LevelMessageHelpTextLayoutGroup.HideWindow();
			LevelEventsController.s_Instance.MessageWasDismissed(m_CurrentlyDisplayedHelpTextMessageInfo.Message);
		}
		ShowNextHelpText();
	}

	public void ConfigureIsolatedUIControlForBoxMessage(CLevelMessage messageDisplayed)
	{
		if (messageDisplayed.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText && messageDisplayed.DismissTrigger.IsTriggeredByDismiss)
		{
			TutorialBoxWindowIsolatedControl.SpecifiedRectTransformToFollow = LevelMessageBoxLayoutGroup.LevelMessageLayouts.First((LevelMessageUILayoutGroup.UILevelMessagePrefabTuple mp) => mp.type == messageDisplayed.LayoutType).uiMessage.closeButton.GetComponent<RectTransform>();
		}
		else
		{
			TutorialBoxWindowIsolatedControl.SpecifiedRectTransformToFollow = null;
		}
	}

	private IEnumerator DisplayMessageInWindowAfterDelay(MessageInfo msg, LevelMessageUILayoutGroup window)
	{
		DisplayDelayInEffect = true;
		if (msg.Message.DisplayDelay > 0f)
		{
			float timeCount = 0f;
			while (timeCount < msg.Message.DisplayDelay)
			{
				if (!Singleton<GamepadDisconnectionBox>.Instance.Window.IsOpen)
				{
					timeCount += Time.unscaledDeltaTime;
				}
				yield return null;
			}
		}
		DisplayDelayInEffect = false;
		LevelEventsController.s_Instance.MessageWasDisplayed(msg.Message);
		window.Show(msg.Message, msg.OnClosedPressedAction);
	}
}
