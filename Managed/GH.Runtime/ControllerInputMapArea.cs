using System;
using Code.State;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.SpecialStates;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerInputMapArea : ControllerInputArea
{
	[Flags]
	private enum ECursorState
	{
		Enable = 0,
		Disable = 1,
		ForceDisable = 2
	}

	[SerializeField]
	private GameObject _mapCursor;

	private ECursorState _cursorState = ECursorState.Disable;

	private ControllerOverrideInput input;

	private IStateFilter _stateFilter = new StateFilterByType(typeof(TravelQuestState), typeof(QuestLogState), typeof(MapStoryState), typeof(MapEventState)).InverseFilter();

	private IStateFilter _lockStateFilter = new StateFilterByType(typeof(LockState));

	private void Awake()
	{
		input = base.gameObject.AddComponent<ControllerOverrideInput>();
		Singleton<UINavigation>.Instance.StateMachine.EventStateChanged += OnEventStateChanged;
	}

	private void OnEventStateChanged(IState newState)
	{
		if (newState != null && _lockStateFilter.IsValid(newState))
		{
			UpdateCursorState(_cursorState | ECursorState.ForceDisable);
		}
		else if (_cursorState.HasFlag(ECursorState.ForceDisable))
		{
			UpdateCursorState(_cursorState ^ ECursorState.ForceDisable);
		}
	}

	public override void EnableGroup(bool isFocused)
	{
		EventSystem.current.currentInputModule.inputOverride = input;
		base.EnableGroup(isFocused);
	}

	public override void DisableGroup()
	{
		EventSystem.current.currentInputModule.inputOverride = null;
		base.DisableGroup();
	}

	protected void BaseFocus()
	{
		base.Focus();
	}

	public override void Focus()
	{
		base.Focus();
		if (!(Singleton<UINavigation>.Instance == null) && (!(Singleton<MapFTUEManager>.Instance != null) || !Singleton<MapFTUEManager>.Instance.HasToShowFTUEOnNoneOrInitialStep) && (Singleton<UINavigation>.Instance.StateMachine.CurrentState == null || _stateFilter.IsCurrentStateValid()))
		{
			if (_cursorState.HasFlag(ECursorState.Disable))
			{
				UpdateCursorState(_cursorState ^ ECursorState.Disable);
			}
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.WorldMap);
		}
	}

	public override void Unfocus()
	{
		UpdateCursorState(_cursorState | ECursorState.Disable);
		base.Unfocus();
	}

	public void EnableCursor()
	{
		UpdateCursorState(ECursorState.Enable);
	}

	private void UpdateCursorState(ECursorState newState)
	{
		if (_mapCursor != null && _cursorState != newState)
		{
			_cursorState = newState;
			_mapCursor.SetActive(_cursorState.Equals(ECursorState.Enable));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Singleton<UINavigation>.Instance != null)
		{
			Singleton<UINavigation>.Instance.StateMachine.EventStateChanged -= OnEventStateChanged;
		}
	}
}
