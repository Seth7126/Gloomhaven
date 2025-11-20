using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerSignInConfirmationBox : Singleton<UIMultiplayerSignInConfirmationBox>
{
	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private ExtendedButton cancelButton;

	[SerializeField]
	private UIWindow confirmationBox;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private new void Awake()
	{
		cancelButton.onClick.AddListener(Hide);
	}

	public void ShowConfirmation(Action onActionConfirmed)
	{
		ResetConfirmationBox();
		base.gameObject.SetActive(value: true);
		confirmationBox.ShowOrUpdateStartingState();
		confirmButton.onClick.AddListener(delegate
		{
			confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
			{
				if (state == UIWindow.VisualState.Hidden)
				{
					onActionConfirmed?.Invoke();
				}
			});
			Hide();
		});
		confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				base.gameObject.SetActive(value: false);
			}
		});
	}

	private void OnEnable()
	{
		controllerArea.Enable();
	}

	private void OnDisable()
	{
		controllerArea.Destroy();
	}

	public void Hide()
	{
		confirmationBox.Hide();
	}

	private void ResetConfirmationBox()
	{
		confirmButton.onClick.RemoveAllListeners();
		confirmationBox.onTransitionComplete.RemoveAllListeners();
	}
}
