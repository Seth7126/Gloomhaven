using System;
using System.Collections.Generic;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Message;
using ScenarioRuleLibrary.CustomLevels;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

public class MapStoryController : Singleton<MapStoryController>
{
	public delegate void MapStoryControllerDelegate(EMapMessageTrigger mapMessageTrigger);

	public class MapDialogInfo : StoryController.DialogInfo
	{
		public MapDialogInfo(List<DialogLineDTO> dialogs, Action onClosed, bool hideOtherUI = true)
			: base(dialogs, onClosed, hideOtherUI)
		{
		}

		public MapDialogInfo(DialogLineDTO dialog, Action onClosed, bool hideOtherUI = true)
			: this(new List<DialogLineDTO> { dialog }, onClosed, hideOtherUI)
		{
		}

		public MapDialogInfo(CLevelMessage levelMessage, Action onClosed)
			: base(levelMessage, onClosed)
		{
		}

		public MapDialogInfo(CMapMessageState mapMessageState, Action onClosed)
			: base(mapMessageState, onClosed)
		{
		}
	}

	[SerializeField]
	private UICharacterStoryBox dialogBox;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private List<StoryHide> elementToHide;

	public static bool DisplayDelayInEffect;

	private EMapMessageTrigger messageTrigger;

	private Queue<MapDialogInfo> m_PendingMessages = new Queue<MapDialogInfo>();

	private StoryController.DialogInfo m_CurrentMessage;

	private MapStoryControllerDelegate m_AllMessagesShownDelegate;

	private bool isVisibleOtherUI = true;

	protected override void Awake()
	{
		base.Awake();
		UIWindow uIWindow = window;
		uIWindow.OnShow = (Action)Delegate.Combine(uIWindow.OnShow, new Action(WindowOnOnShow));
		UIWindow uIWindow2 = window;
		uIWindow2.OnHide = (Action)Delegate.Combine(uIWindow2.OnHide, new Action(WindowOnOnHide));
	}

	protected override void OnDestroy()
	{
		UIWindow uIWindow = window;
		uIWindow.OnShow = (Action)Delegate.Remove(uIWindow.OnShow, new Action(WindowOnOnShow));
		UIWindow uIWindow2 = window;
		uIWindow2.OnHide = (Action)Delegate.Remove(uIWindow2.OnHide, new Action(WindowOnOnHide));
		base.OnDestroy();
	}

	private void WindowOnOnHide()
	{
	}

	private void WindowOnOnShow()
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.MapStory);
	}

	private void ShowNext(bool navigateToPreviousStateWhenHidden = true)
	{
		if (m_CurrentMessage != null)
		{
			return;
		}
		if (m_PendingMessages.Count > 0)
		{
			ShowImmediately(m_PendingMessages.Dequeue(), navigateToPreviousStateWhenHidden);
			return;
		}
		ShowOtherGUI(show: true);
		window.Hide();
		if (m_AllMessagesShownDelegate != null)
		{
			m_AllMessagesShownDelegate(messageTrigger);
		}
	}

	public void Show(EMapMessageTrigger messageTrigger, List<CMapMessageState> messages, Action onClosed = null, MapStoryControllerDelegate onAllClosed = null)
	{
		this.messageTrigger = messageTrigger;
		m_AllMessagesShownDelegate = onAllClosed;
		foreach (CMapMessageState message in messages)
		{
			MapDialogInfo item = new MapDialogInfo(message, onClosed);
			m_PendingMessages.Enqueue(item);
		}
		ShowNext();
	}

	public void Show(EMapMessageTrigger messageTrigger, List<MapDialogInfo> messages, MapStoryControllerDelegate onAllClosed = null)
	{
		this.messageTrigger = messageTrigger;
		m_AllMessagesShownDelegate = onAllClosed;
		foreach (MapDialogInfo message in messages)
		{
			m_PendingMessages.Enqueue(message);
		}
		ShowNext();
	}

	public void Show(EMapMessageTrigger messageTrigger, MapDialogInfo message, MapStoryControllerDelegate onAllClosed = null, bool navigateToPreviousStateWhenHidden = true)
	{
		this.messageTrigger = messageTrigger;
		m_AllMessagesShownDelegate = onAllClosed;
		m_PendingMessages.Enqueue(message);
		ShowNext(navigateToPreviousStateWhenHidden);
	}

	public void Clear()
	{
		messageTrigger = EMapMessageTrigger.None;
		m_PendingMessages.Clear();
		m_CurrentMessage = null;
		dialogBox.Hide();
	}

	private void ShowImmediately(MapDialogInfo message, bool navigateToPreviousStateWhenHidden = true)
	{
		m_CurrentMessage = message;
		dialogBox.Show(message.DialogPages, OnFinishShow, navigateToPreviousStateWhenHidden);
		ShowOtherGUI(!message.HideOtherGUI);
		window.Show(instant: true);
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.MapStory);
	}

	private void ShowOtherGUI(bool show)
	{
		if (isVisibleOtherUI == show)
		{
			return;
		}
		isVisibleOtherUI = show;
		for (int i = 0; i < elementToHide.Count; i++)
		{
			if (show)
			{
				elementToHide[i].Show();
			}
			else
			{
				elementToHide[i].Hide();
			}
		}
	}

	private void OnFinishShow()
	{
		m_CurrentMessage?.OnClosedAction?.Invoke();
		if (m_CurrentMessage != null && m_CurrentMessage.MapMsg != null)
		{
			Singleton<MapChoreographer>.Instance.OnMapMessageShown(m_CurrentMessage.MapMsg);
		}
		m_CurrentMessage = null;
		ShowNext();
	}
}
