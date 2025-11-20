using System;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using UnityEngine;

public class UIUseSlot<T> : MonoBehaviour, IEscapable
{
	[SerializeField]
	protected ExtendedButton button;

	[SerializeField]
	private GameObject selectedMask;

	[SerializeField]
	protected GameObject optionalHiglight;

	[SerializeField]
	private GameObject mandatoryHiglight;

	[SerializeField]
	private float disabledAlpha = 0.25f;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	protected string selectAudioItem = "PlaySound_UIButtonSelect";

	[SerializeField]
	protected string unselectAudioItem = "PlaySound_UIButtonSelect";

	[SerializeField]
	private Hotkey hotkey;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	protected bool selected;

	protected bool interactable;

	protected CActor actor;

	protected T element;

	protected Action<T> onSelect;

	private Action<T> onUnselect;

	private Func<T, bool> isMandatoryChecker;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	protected virtual void Init(CActor actor, T element, Action<T> onSelect = null, Action<T> onUnselect = null, Func<T, bool> isMandatoryChecker = null, bool isSelected = false)
	{
		this.actor = actor;
		selected = isSelected;
		this.element = element;
		this.onSelect = onSelect;
		this.onUnselect = onUnselect;
		this.isMandatoryChecker = isMandatoryChecker;
		SetInteractable(interactable: true);
		Refresh();
	}

	[UsedImplicitly]
	protected void OnDestroy()
	{
		HideHotkey();
	}

	public virtual void Show()
	{
		Show(instant: false);
	}

	public void Show(bool instant)
	{
		base.gameObject.SetActive(value: true);
		CancelAnimations();
		if (instant)
		{
			showAnimation.GoToFinishState();
		}
		else
		{
			showAnimation.Play();
		}
	}

	public virtual void Hide()
	{
		CancelAnimations();
		base.gameObject.SetActive(value: false);
	}

	public virtual void Select()
	{
		selected = true;
		Refresh();
		onSelect?.Invoke(element);
	}

	protected void ShowHotkey()
	{
		if (hotkey != null)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	protected void HideHotkey()
	{
		if (hotkey != null)
		{
			hotkey.Deinitialize();
			hotkey.DisplayHotkey(active: false);
		}
	}

	public virtual void Unselect()
	{
		if (selected)
		{
			AudioControllerUtils.PlaySound(unselectAudioItem);
			ClearSelection(fromClick: true);
			onUnselect?.Invoke(element);
		}
	}

	public virtual void ClearSelection(bool fromClick = false)
	{
		if (selected)
		{
			selected = false;
			Refresh();
		}
	}

	public bool IsSelected()
	{
		return selected;
	}

	public virtual void Refresh()
	{
		if (selectedMask != null)
		{
			selectedMask.SetActive(selected);
		}
		bool flag = isMandatoryChecker != null && isMandatoryChecker(element);
		if (mandatoryHiglight != null)
		{
			mandatoryHiglight.SetActive(flag && interactable && !selected);
		}
		if (optionalHiglight != null)
		{
			optionalHiglight.SetActive(selected && interactable);
		}
	}

	public virtual void OnPointerDown()
	{
		if (interactable)
		{
			InputManager.SkipNextSubmitAction();
			Toggle();
		}
	}

	public void Toggle()
	{
		if (selected)
		{
			Unselect();
			return;
		}
		AudioControllerUtils.PlaySound(selectAudioItem);
		Select();
	}

	protected virtual void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			button.DisableNavigation();
			CancelAnimations();
		}
	}

	protected virtual void CancelAnimations()
	{
		showAnimation.Stop();
		showAnimation.GoToFinishState();
	}

	public void SetInteractable(bool interactable)
	{
		this.interactable = interactable && (!FFSNetwork.IsOnline || (actor != null && Choreographer.s_Choreographer.ActorOrHisSummonerIsUnderMyControl(actor)));
		canvasGroup.alpha = (this.interactable ? 1f : disabledAlpha);
		Refresh();
	}

	public virtual bool Escape()
	{
		if (!selected || !interactable)
		{
			return false;
		}
		OnPointerDown();
		return false;
	}

	public int Order()
	{
		return 0;
	}
}
