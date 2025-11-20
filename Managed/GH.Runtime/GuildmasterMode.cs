using System;
using UnityEngine;

public abstract class GuildmasterMode
{
	protected Action onEnter;

	protected Action onExit;

	protected UIGuildmasterButton button;

	private AWScreenName analyticLogScreen;

	private EControllerInputAreaType controllerAreaId;

	public Transform Button => button.transform;

	public abstract bool IsUnlocked { get; }

	protected GuildmasterMode(EGuildmasterMode mode, UIGuildmasterButton button, Action onEnter, Action onExit, EControllerInputAreaType controllerAreaId, AWScreenName analyticLogScreen, Func<bool> canToggle = null)
	{
		this.analyticLogScreen = analyticLogScreen;
		this.onEnter = onEnter;
		this.onExit = onExit;
		this.button = button;
		this.controllerAreaId = controllerAreaId;
		button.SetMode(mode, newNotification: false, canToggle);
		RefreshNotifications();
	}

	public virtual void Enter()
	{
		AnalyticsWrapper.LogScreenDisplay(analyticLogScreen);
		button.Select();
		ControllerInputAreaManager.Instance.SetDefaultFocusArea(controllerAreaId);
		onEnter?.Invoke();
		NewPartyDisplayUI.PartyDisplay.ResetLastTabElementsCount();
	}

	public virtual void Exit()
	{
		button.Deselect();
		onExit?.Invoke();
	}

	public void RefreshNotifications()
	{
		button.ShowNewNotification(CheckNewNotifications());
	}

	protected abstract bool CheckNewNotifications();

	public virtual void RefreshUnlocked()
	{
		button.gameObject.SetActive(IsUnlocked);
	}

	public void EnableNavigation(Action navigatePrevious, Action navigateNext)
	{
		button.EnableNavigation(navigatePrevious, navigateNext);
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}
}
