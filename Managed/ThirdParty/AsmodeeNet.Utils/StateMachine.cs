using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsmodeeNet.Utils;

[Serializable]
public class StateMachine
{
	private List<ActionState> _listState = new List<ActionState>();

	private List<Transition> _listTransition = new List<Transition>();

	public ActionState CurrentState;

	public ActionState PreviousState;

	public bool IsDebug;

	public Action OnStateChanged;

	public bool Enabled = true;

	public bool FirstUpdate { get; set; }

	public string FSMName { get; set; }

	public StateMachine(string fsmName)
	{
		FSMName = fsmName;
	}

	public void Update()
	{
		if (!Enabled)
		{
			return;
		}
		if (CurrentState.ActionUpdate != null)
		{
			CurrentState.ActionUpdate();
		}
		FirstUpdate = false;
		foreach (Transition item in _listTransition)
		{
			if ((item.ActionStateStart != CurrentState && (item.ActionStateEnd == CurrentState || item.ActionStateStart != null)) || !item.Condition())
			{
				continue;
			}
			if (IsDebug)
			{
				Debug.Log("FSM " + FSMName + " : " + CurrentState.Name + " -> " + item.ActionStateEnd.Name);
			}
			if (item.TransitionType == TransitionType.WithDuration)
			{
				if (CurrentState.ActionExit != null)
				{
					CurrentState.ActionExit();
				}
				PreviousState = item.ActionStateStart;
				CurrentState = item.ActionStateEnd;
				FirstUpdate = true;
				if (OnStateChanged != null)
				{
					OnStateChanged();
				}
				if (CurrentState.ActionEnter != null)
				{
					CurrentState.ActionEnter();
				}
			}
			else
			{
				if (CurrentState.ActionExit != null)
				{
					CurrentState.ActionExit();
				}
				PreviousState = item.ActionStateStart;
				CurrentState = item.ActionStateEnd;
				FirstUpdate = true;
				if (OnStateChanged != null)
				{
					OnStateChanged();
				}
				if (CurrentState.ActionEnter != null)
				{
					CurrentState.ActionEnter();
				}
			}
			break;
		}
	}

	public ActionState AddActionState(string name)
	{
		return AddActionState(name, null, null, null);
	}

	public ActionState AddActionState(string name, Action actionEnter)
	{
		return AddActionState(name, actionEnter, null, null);
	}

	public ActionState AddActionState(string name, Action actionEnter, Action actionUpdate, Action actionExit)
	{
		ActionState actionState = new ActionState(name, actionEnter, actionUpdate, actionExit);
		_listState.Add(actionState);
		return actionState;
	}

	public Transition AddTransition(ActionState actionStateEnd, Func<bool> condition)
	{
		return AddTransition(null, actionStateEnd, condition);
	}

	public Transition AddTransition(ActionState actionStateStart, ActionState actionStateEnd, Func<bool> condition)
	{
		Transition transition = new Transition(actionStateStart, actionStateEnd, condition, TransitionType.Normal);
		_listTransition.Add(transition);
		return transition;
	}

	public Transition AddTransition(ActionState actionStateStart, ActionState actionStateEnd, float transitionDuration)
	{
		Transition transition = new Transition(actionStateStart, actionStateEnd, transitionDuration, TransitionType.WithDuration);
		_listTransition.Add(transition);
		return transition;
	}

	public void Reset()
	{
		_listState = new List<ActionState>();
		_listTransition = new List<Transition>();
	}
}
