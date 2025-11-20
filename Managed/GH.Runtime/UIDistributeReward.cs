using System;
using Assets.Script.Misc;
using FFSNet;
using GLOOM;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDistributeReward : MonoBehaviour
{
	[SerializeField]
	private UIDistributePointsPopup popup;

	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private LongConfirmHandler longConfirm;

	[SerializeField]
	private UIControllerKeyTip confirmControllerTip;

	[SerializeField]
	private UIReward rewardUI;

	private CallbackPromise promise;

	private Color? defaultConfirmTextColor;

	private DistributeRewardProcess.EDistributeRewardProcessType m_Type;

	public UIDistributePointsPopup PopUp => popup;

	private void Awake()
	{
		if (!defaultConfirmTextColor.HasValue)
		{
			defaultConfirmTextColor = confirmButton.buttonText.color;
		}
		confirmButton.onClick.AddListener(OnConfirmClick);
	}

	private void OnDestroy()
	{
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, OnShortPress);
		confirmButton.onClick.RemoveAllListeners();
		confirmButton.onClick.RemoveAllListeners();
	}

	public ICallbackPromise Distribute(DistributeRewardService service, DistributeRewardProcess.EDistributeRewardProcessType type, string confirmButtonTextLoc = "GUI_CONFIRM", string optionsTextLoc = null)
	{
		m_Type = type;
		rewardUI.ShowReward(service.Reward);
		SetInteractable(service.AvailablePoints == 0);
		popup.Slots.ForEach(delegate(UIDistributePointsSlot slot)
		{
			slot.ResetClickWrapper();
		});
		popup.Show(service, delegate
		{
			SetInteractable(service.AvailablePoints == 0);
		}, null, optionsTextLoc.IsNullOrEmpty() ? null : LocalizationManager.GetTranslation(optionsTextLoc), isRewards: true);
		popup.Slots.ForEach(delegate(UIDistributePointsSlot slot)
		{
			slot.SetClickWrapper(ConfirmInGamePadMode);
		});
		InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, OnShortPress);
		confirmButton.TextLanguageKey = confirmButtonTextLoc;
		bool stateSet = false;
		ActionProcessorStateType currentState = ActionProcessor.CurrentState.StateType;
		ActionPhaseType currentPhase = ActionProcessor.CurrentState.PhaseType;
		if (FFSNetwork.IsClient)
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely);
			stateSet = true;
		}
		promise = new CallbackPromise();
		return promise.Then(delegate
		{
			InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, OnShortPress);
			popup.Hide();
			service.Apply();
			if (stateSet)
			{
				ActionProcessor.SetState(currentState, currentPhase);
			}
		});
	}

	private void OnShortPress()
	{
		if (popup.IsFocused)
		{
			popup.CurrentHoveredSlot.OnClick();
		}
	}

	private void SetInteractable(bool interactable)
	{
		SetButtonInteractable(interactable);
		if (!defaultConfirmTextColor.HasValue)
		{
			defaultConfirmTextColor = confirmButton.buttonText.color;
		}
		confirmButton.buttonText.color = (confirmButton.interactable ? defaultConfirmTextColor.GetValueOrDefault() : UIInfoTools.Instance.greyedOutTextColor);
		confirmControllerTip.ShowInteractable(confirmButton.interactable);
	}

	private void SetButtonInteractable(bool interactable)
	{
		bool flag = interactable && (!FFSNetwork.IsOnline || FFSNetwork.IsHost);
		if (InputManager.GamePadInUse)
		{
			confirmButton.gameObject.SetActive(flag);
			longConfirm?.gameObject.SetActive(flag);
		}
		else
		{
			confirmButton.interactable = flag;
		}
	}

	private void ConfirmInGamePadMode(Action shortClick)
	{
		if (InputManager.GamePadInUse && longConfirm.gameObject.activeSelf)
		{
			longConfirm?.Pressed(RaiseClickEvent, shortClick);
		}
		else
		{
			shortClick?.Invoke();
		}
	}

	private void RaiseClickEvent()
	{
		confirmButton.OnPointerClick(new PointerEventData(EventSystem.current));
	}

	public void OnConfirmClick()
	{
		promise?.Resolve();
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendGameAction(GameActionType.DistributeUIConfirm, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, (int)m_Type);
		}
	}

	public void ProxyConfirmClick()
	{
		promise?.Resolve();
	}
}
