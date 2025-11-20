using System;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;

namespace Script.GUI.SMNavigation;

public abstract class UIStateSwitcher<TTag, TTarget> : MonoBehaviour where TTag : Enum where TTarget : MonoBehaviour, IShowActivity
{
	[SerializeField]
	protected TTarget _activityElement;

	[SerializeField]
	protected TTag _enteringTag;

	[SerializeField]
	protected UiNavigationRoot _enteringRoot;

	[SerializeField]
	protected TTag _exitTag;

	[SerializeField]
	protected UiNavigationRoot _exitRoot;

	[SerializeField]
	protected bool _toPreviousStateOnExit;

	private Enum _previousStateTag;

	private void Awake()
	{
		if (_activityElement == null)
		{
			_activityElement = GetComponent<TTarget>();
		}
		if (!(_activityElement == null))
		{
			ref TTarget activityElement = ref _activityElement;
			ref TTarget reference = ref activityElement;
			Action onShow = (Action)Delegate.Combine(activityElement.OnShow, new Action(OnShow));
			reference.OnShow = onShow;
			activityElement = ref _activityElement;
			ref TTarget reference2 = ref activityElement;
			Action onHide = (Action)Delegate.Combine(activityElement.OnHide, new Action(OnHide));
			reference2.OnHide = onHide;
		}
	}

	private void OnDestroy()
	{
		if (_activityElement != null)
		{
			ref TTarget activityElement = ref _activityElement;
			ref TTarget reference = ref activityElement;
			Action onShow = (Action)Delegate.Remove(activityElement.OnShow, new Action(OnShow));
			reference.OnShow = onShow;
			activityElement = ref _activityElement;
			ref TTarget reference2 = ref activityElement;
			Action onHide = (Action)Delegate.Remove(activityElement.OnHide, new Action(OnHide));
			reference2.OnHide = onHide;
		}
	}

	protected virtual void OnShow()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(_enteringTag);
	}

	protected virtual void OnHide()
	{
		if (_toPreviousStateOnExit)
		{
			StateFilterByTag<MainStateTag> stateFilterByTag = new StateFilterByTag<MainStateTag>(MainStateTag.HouseRules, MainStateTag.GuildMasterCreateGame, MainStateTag.CampaignCreateGame, MainStateTag.MapEscMenu, MainStateTag.MainOptions, MainStateTag.ScenarioEscMenu, MainStateTag.LoadGame);
			Singleton<UINavigation>.Instance.StateMachine.ToPreviousDifferentState<object>(null, new IStateFilter[1] { stateFilterByTag });
		}
		else
		{
			TTag exitTag = _exitTag;
			if (!(exitTag is MainStateTag) || (MainStateTag)(object)/*isinst with value type is only supported in some contexts*/ != MainStateTag.Empty)
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(_exitTag);
			}
		}
	}
}
