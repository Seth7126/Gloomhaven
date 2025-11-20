using System;
using System.Collections.Generic;
using MapRuleLibrary.Party;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIRetireCharacterConfirmationBox : MonoBehaviour
{
	[SerializeField]
	private List<UICharacterInformation> information;

	[SerializeField]
	protected ExtendedButton confirmButton;

	[SerializeField]
	private GUIAnimator showAnimator;

	[SerializeField]
	private GUIAnimator confirmAnimator;

	[SerializeField]
	private List<CanvasGroup> completedMasks;

	private ControllerInputAreaLocal controllerArea;

	private Action onActionConfirmed;

	private Action onHidden;

	private UIWindow window;

	private void Awake()
	{
		controllerArea = GetComponent<ControllerInputAreaLocal>();
		controllerArea?.OnFocusedArea.AddListener(OnFocused);
		window = GetComponent<UIWindow>();
		window.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Shown)
			{
				showAnimator.Play();
			}
		});
		window.onHidden.AddListener(OnHidden);
		showAnimator.OnAnimationFinished.AddListener(OnShowFinished);
		confirmAnimator.OnAnimationFinished.AddListener(Hide);
		confirmButton.onClick.AddListener(delegate
		{
			Confirm();
		});
	}

	private void OnDestroy()
	{
		confirmButton.onClick.RemoveAllListeners();
	}

	public void ShowConfirmationBox(CMapCharacter character, Action onActionConfirmed, Action onHidden)
	{
		this.onActionConfirmed = onActionConfirmed;
		this.onHidden = onHidden;
		confirmAnimator.Stop();
		confirmAnimator.GoInitState();
		confirmButton.gameObject.SetActive(value: false);
		showAnimator.Stop();
		showAnimator.GoInitState();
		for (int i = 0; i < completedMasks.Count; i++)
		{
			completedMasks[i].alpha = 0f;
		}
		foreach (UICharacterInformation item in information)
		{
			item.Display(character);
		}
		window.Show();
	}

	public void Hide()
	{
		window.Hide(instant: true);
	}

	private void Confirm()
	{
		confirmButton.gameObject.SetActive(value: false);
		confirmAnimator.Play();
		onActionConfirmed?.Invoke();
	}

	private void OnDisable()
	{
		confirmAnimator.Stop();
		showAnimator.Stop();
	}

	private void OnShowFinished()
	{
		for (int i = 0; i < completedMasks.Count; i++)
		{
			completedMasks[i].alpha = 1f;
		}
		confirmButton.gameObject.SetActive(value: true);
		controllerArea?.Enable();
	}

	private void OnHidden()
	{
		confirmAnimator.Stop();
		showAnimator.Stop();
		controllerArea?.Destroy();
		onHidden?.Invoke();
	}

	private void OnFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.RetirementConfirmation);
	}
}
