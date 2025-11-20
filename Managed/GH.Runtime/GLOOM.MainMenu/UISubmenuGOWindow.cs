using System;
using AsmodeeNet.Foundation;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UISubmenuGOWindow : MonoBehaviour, IShowActivity
{
	[SerializeField]
	protected ControllerInputAreaLocal controllerArea;

	[SerializeField]
	protected GUIAnimator showAnimator;

	protected UIWindow window;

	public UnityEvent OnHidden = new UnityEvent();

	public Action OnShow { get; set; }

	public Action OnHide { get; set; }

	public Action<bool> OnActivityChanged { get; set; }

	public bool IsActive => base.gameObject.activeSelf;

	protected virtual void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnCompleteHidden);
		window.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Shown)
			{
				OnShown();
			}
		});
		controllerArea.OnFocusedArea.AddListener(OnControllerFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerUnfocused);
	}

	protected virtual void OnDestroy()
	{
		OnHidden = null;
		OnShow = null;
		OnHide = null;
		OnActivityChanged = null;
	}

	protected virtual void OnControllerUnfocused()
	{
	}

	protected virtual void OnControllerFocused()
	{
	}

	public virtual void Show()
	{
		if (base.gameObject.activeSelf && window.IsOpen)
		{
			showAnimator.Play();
		}
		else
		{
			base.gameObject.SetActive(value: true);
			window.ShowOrUpdateStartingState();
		}
		OnShow?.Invoke();
		OnActivityChanged?.Invoke(obj: true);
		controllerArea.Enable();
	}

	protected virtual void OnShown()
	{
		showAnimator.Play();
	}

	public void ToggleVisibility(bool on)
	{
		if (on)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			showAnimator.Stop();
			window.Hide();
		}
	}

	protected virtual void OnCompleteHidden()
	{
		showAnimator.Stop();
		base.gameObject.SetActive(value: false);
		OnHidden.Invoke();
		OnHide?.Invoke();
		OnActivityChanged?.Invoke(obj: true);
	}

	protected virtual void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			controllerArea.Destroy();
			showAnimator.Stop();
		}
	}
}
