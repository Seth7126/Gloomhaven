#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using Code.State;
using FFSNet;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Message;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates;
using UnityEngine;
using UnityEngine.UI;

public class StoryController : Singleton<StoryController>
{
	public class DialogInfo
	{
		public CLevelMessage LevelMsg;

		public CMapMessageState MapMsg;

		public List<DialogLineDTO> DialogPages;

		public Action OnClosedAction;

		public bool HideOtherGUI = true;

		public DialogInfo(List<DialogLineDTO> dialogs, Action onClosed, bool hideOtherUI = true)
		{
			DialogPages = dialogs;
			OnClosedAction = onClosed;
			HideOtherGUI = hideOtherUI;
		}

		public DialogInfo(CLevelMessage levelMessage, Action onClosed)
		{
			LevelMsg = levelMessage;
			DialogPages = new List<DialogLineDTO>();
			foreach (CLevelMessagePage page in levelMessage.Pages)
			{
				DialogPages.Add(new DialogLineDTO(page));
			}
			OnClosedAction = onClosed;
			HideOtherGUI = levelMessage.DisplayTrigger.EventTriggerTypeInt != 30 && levelMessage.DisplayTrigger.EventTriggerTypeInt != 31;
		}

		public DialogInfo(CMapMessageState mapMessageState, Action onClosed)
		{
			MapMsg = mapMessageState;
			DialogPages = new List<DialogLineDTO>();
			foreach (MapDialogueLine dialogueLine in mapMessageState.MapMessage.DialogueLines)
			{
				DialogPages.Add(new DialogLineDTO(dialogueLine));
			}
			OnClosedAction = onClosed;
		}
	}

	[SerializeField]
	private UICharacterStoryBox dialogBox;

	[SerializeField]
	private UIWindow window;

	public static bool DisplayDelayInEffect;

	private readonly IStateFilter _stateFilter = new StateFilterByType(typeof(AnimationScenarioState), typeof(SelectInputDeviceBoxState)).InverseFilter();

	private Queue<DialogInfo> m_PendingMessages = new Queue<DialogInfo>();

	private DialogInfo m_CurrentMessage;

	[SerializeField]
	private List<StoryHide> elementToHide;

	private bool isVisibleOtherUI = true;

	private bool isBlockedUpdate;

	public UICharacterStoryBox DialogBox => dialogBox;

	public bool IsVisible => window.IsVisible;

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

	private void ShowNext()
	{
		if (m_PendingMessages.Count > 0)
		{
			m_PendingMessages.Dequeue();
		}
		if (m_PendingMessages.Count > 0)
		{
			ShowImmediately(m_PendingMessages.Peek());
			return;
		}
		CoroutineHelper.RunNextFrame(delegate
		{
			ShowOtherGUI(show: true);
		});
		window.Hide();
		TryUnblockedUpdate();
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(_stateFilter);
		if (!FFSNetwork.IsOnline || SaveData.Instance.Global.CurrentGameState != EGameState.Scenario || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest || ActionProcessor.CurrentPhase == ActionPhaseType.ScenarioEnded)
		{
			return;
		}
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.StartOfRound);
		if (FFSNetwork.IsClient)
		{
			ControllableRegistry.AllControllables.ForEach(delegate(NetworkControllable x)
			{
				x.ApplyState();
			});
			if (!SceneController.Instance.GameLoadedAndClientReady)
			{
				Synchronizer.SendSideAction(GameActionType.GameLoadedAndClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
				SceneController.Instance.GameLoadedAndClientReady = true;
			}
		}
	}

	public void Show(CLevelMessage message, Action onClosed = null)
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.DialogueMessage);
		Show(new DialogInfo(message, onClosed));
	}

	public void Show(DialogInfo dialogInf)
	{
		StartCoroutine(DisplayMessageInWindowAfterDelay(dialogInf));
	}

	public void Clear()
	{
		Debug.LogGUI("story controller clear");
		m_PendingMessages.Clear();
		m_CurrentMessage = null;
		dialogBox.Hide();
	}

	private IEnumerator DisplayMessageInWindowAfterDelay(DialogInfo msg)
	{
		if (msg.LevelMsg != null)
		{
			if (!AutoTestController.s_Instance.AutotestStarted)
			{
				DisplayDelayInEffect = true;
				if (msg.LevelMsg.DisplayDelay > 0f)
				{
					yield return new WaitForSecondsRealtime(msg.LevelMsg.DisplayDelay);
				}
				DisplayDelayInEffect = false;
			}
			LevelEventsController.s_Instance.MessageWasDisplayed(msg.LevelMsg, fromStoryController: true);
		}
		m_PendingMessages.Enqueue(msg);
		if (m_PendingMessages.Count == 1)
		{
			ShowImmediately(msg);
		}
	}

	private void ShowImmediately(DialogInfo message)
	{
		m_CurrentMessage = message;
		dialogBox.Show(message.DialogPages, OnFinishShow, navigateToPreviousStateWhenHidden: false);
		TryBlockedUpdate();
		ShowOtherGUI(!message.HideOtherGUI);
		if (!AutoTestController.s_Instance.AutotestStarted)
		{
			window.Show(instant: true);
		}
		else
		{
			OnFinishShow();
		}
	}

	private void OnFinishShow()
	{
		m_CurrentMessage?.OnClosedAction?.Invoke();
		if (m_CurrentMessage != null && m_CurrentMessage.LevelMsg != null)
		{
			LevelEventsController.s_Instance.MessageWasDismissed(m_CurrentMessage.LevelMsg);
		}
		ShowNext();
	}

	private void TryBlockedUpdate()
	{
		if (!isBlockedUpdate)
		{
			isBlockedUpdate = true;
			Choreographer.s_Choreographer.AddUpdateBlocker();
			ActionProcessor.LockProcessingAction();
		}
	}

	private void TryUnblockedUpdate()
	{
		if (isBlockedUpdate)
		{
			isBlockedUpdate = false;
			Choreographer.s_Choreographer.RemoveUpdateBlocker();
			ActionProcessor.UnlockProcessingAction();
		}
	}
}
