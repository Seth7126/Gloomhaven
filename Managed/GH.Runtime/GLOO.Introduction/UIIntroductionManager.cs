using System;
using System.Collections.Generic;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

namespace GLOO.Introduction;

public class UIIntroductionManager : Singleton<UIIntroductionManager>
{
	[Serializable]
	private class MessageInfo
	{
		public Action OnClosedPressedAction;

		public CLevelMessage Message;

		public string ID;

		public Func<bool> AutocloseCondition;

		public MessageInfo(string id, CLevelMessage message, Action onClosedPressedAction, Func<bool> autocloseCondition)
		{
			Message = message;
			OnClosedPressedAction = onClosedPressedAction;
			ID = id;
			AutocloseCondition = autocloseCondition;
		}
	}

	[SerializeField]
	private LevelMessageUILayoutGroup layoutGroup;

	private List<MessageInfo> m_PendingMessages = new List<MessageInfo>();

	private MessageInfo m_CurrentlyDisplayedMessageInfo;

	public LevelMessageUILayoutGroup LayoutGroup => layoutGroup;

	private void Start()
	{
		layoutGroup.HideWindow();
	}

	public void ShowStep(string id, IntroductionStepUI step, Action onClosed = null, Func<bool> autocloseCondition = null)
	{
		MessageInfo message = new MessageInfo(id, step.ToMessage(), delegate
		{
			onClosed?.Invoke();
			ShowNextMessage();
		}, autocloseCondition);
		AddMessage(message);
	}

	public void ShowSteps(string id, List<IntroductionStepUI> steps, Action onClosed = null, Func<bool> autoMoveToNextStepCondition = null)
	{
		if (steps.Count == 0)
		{
			onClosed?.Invoke();
			return;
		}
		for (int i = 0; i < steps.Count; i++)
		{
			ShowStep(id, steps[i], (i == steps.Count - 1) ? onClosed : null, autoMoveToNextStepCondition);
		}
	}

	public void Show(IntroductionConfigUI config, Action onClosed = null)
	{
		ShowSteps(config.name, config.GetSteps(), onClosed);
	}

	public void Show(EIntroductionConcept concept, Action onClosed = null)
	{
		Show(UIInfoTools.Instance.GetIntroductionConfig(concept), onClosed);
	}

	private void AddMessage(MessageInfo message)
	{
		m_PendingMessages.Add(message);
		if (m_PendingMessages.Count == 1)
		{
			ShowMessageImmediately(message);
		}
	}

	private void ShowNextMessage()
	{
		StopAllCoroutines();
		m_PendingMessages.Remove(m_CurrentlyDisplayedMessageInfo);
		if (m_PendingMessages.Count > 0)
		{
			ShowMessageImmediately(m_PendingMessages[0]);
			return;
		}
		m_CurrentlyDisplayedMessageInfo = null;
		layoutGroup.HideWindow();
	}

	private void ShowMessageImmediately(MessageInfo message)
	{
		m_CurrentlyDisplayedMessageInfo = message;
		layoutGroup.Show(message.Message, message.OnClosedPressedAction, message.AutocloseCondition);
	}

	public void HideById(string id)
	{
		m_PendingMessages.RemoveAll((MessageInfo it) => it.ID == id);
		if (m_CurrentlyDisplayedMessageInfo != null && m_CurrentlyDisplayedMessageInfo.ID == id)
		{
			ShowNextMessage();
		}
	}
}
