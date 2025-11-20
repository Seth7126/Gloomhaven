using System;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIItemConfirmationBox : Singleton<UIItemConfirmationBox>
{
	[SerializeField]
	private RectTransform pictureHolder;

	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private ExtendedButton cancelButton;

	[SerializeField]
	private TMP_Text titleText;

	[SerializeField]
	private TMP_Text informationText;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private UIWindow confirmationBox;

	private ItemCardUI itemCardUI;

	private Action _onConfirmedCallback;

	private Action _onCancelCallback;

	public bool IsActive { get; private set; }

	public event Action<BoxConfirmationType> ConfirmationBoxRequested;

	protected override void Awake()
	{
		base.Awake();
		confirmationBox = GetComponent<UIWindow>();
		controllerArea.OnFocusedArea.AddListener(OnFocus);
		ResetConfirmationBox();
		InitGamepadInput();
	}

	private void OnFocus()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.ItemConfirmationBox);
	}

	private void InitGamepadInput()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnControllerSubmit).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnControllerCancelled).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnControllerSubmit);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnControllerCancelled);
		}
	}

	private void OnControllerCancelled()
	{
		OnCancel();
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
	}

	private void OnControllerSubmit()
	{
		OnConfirm();
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
	}

	public void ShowConfirmation(BoxConfirmationType boxConfirmationType, string title, string information, CItem item, Action onActionConfirmed, string confirmActionKey = null, string cancelActionKey = null, string audioConfirmation = "", Action onActionCancelled = null)
	{
		IsActive = true;
		this.ConfirmationBoxRequested?.Invoke(boxConfirmationType);
		_onConfirmedCallback = onActionConfirmed;
		_onCancelCallback = onActionCancelled;
		ResetConfirmationBox();
		if (audioConfirmation.IsNOTNullOrEmpty())
		{
			confirmButton.mouseDownAudioItem = audioConfirmation;
		}
		itemCardUI = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, pictureHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<ItemCardUI>();
		itemCardUI.item = item;
		itemCardUI.ReloadBackground();
		titleText.text = title;
		informationText.text = information;
		if (confirmActionKey != null)
		{
			confirmButton.TextLanguageKey = confirmActionKey;
		}
		if (cancelActionKey != null)
		{
			cancelButton.TextLanguageKey = cancelActionKey;
		}
		confirmButton.onClick.AddListener(OnConfirm);
		cancelButton.onClick.AddListener(OnCancel);
		confirmationBox.Show();
		controllerArea.Enable();
	}

	private void OnCancel()
	{
		if (_onCancelCallback != null)
		{
			confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
			{
				if (state == UIWindow.VisualState.Hidden)
				{
					OnHidden();
					_onCancelCallback();
				}
			});
		}
		Hide();
	}

	private void OnConfirm()
	{
		confirmationBox.onTransitionComplete.RemoveAllListeners();
		confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				OnHidden();
			}
		});
		_onConfirmedCallback?.Invoke();
		Hide();
	}

	public bool IsConfirmingItem(CItem item)
	{
		if (confirmationBox.IsOpen && itemCardUI != null)
		{
			return itemCardUI.item == item;
		}
		return false;
	}

	public void Hide()
	{
		confirmationBox.Hide();
		IsActive = false;
	}

	private void ResetConfirmationBox()
	{
		if (itemCardUI != null)
		{
			ObjectPool.RecycleCard(itemCardUI.CardID, ObjectPool.ECardType.Item, itemCardUI.gameObject);
			itemCardUI = null;
		}
		confirmButton.TextLanguageKey = "YES";
		cancelButton.TextLanguageKey = "NO";
		confirmButton.mouseDownAudioItem = null;
		confirmButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		confirmationBox.onHidden.RemoveAllListeners();
		confirmationBox.onTransitionComplete.RemoveAllListeners();
		confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				OnHidden();
			}
		});
	}

	private void OnHidden()
	{
		if (itemCardUI != null)
		{
			ObjectPool.RecycleCard(itemCardUI.CardID, ObjectPool.ECardType.Item, itemCardUI.gameObject);
			itemCardUI = null;
		}
		controllerArea.Destroy();
	}

	protected override void OnDestroy()
	{
		confirmButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		DisableGamepadInput();
		base.OnDestroy();
	}
}
