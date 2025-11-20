using System.Collections;
using Code.State;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.UI;

public class GamepadDisconnectionBox : Singleton<GamepadDisconnectionBox>
{
	private const int EuParam = 116;

	private const int UsParam = 115;

	[SerializeField]
	private TextLocalizedListener _titleText;

	[SerializeField]
	private TextLocalizedListener _messageText;

	[SerializeField]
	private Hotkey[] _hotkeys;

	[SerializeField]
	private UIWindow _uiWindow;

	private bool _pauseGame;

	private bool _blockedChoreographer;

	private readonly UiNavigationBlocker _navigationBlocker = new UiNavigationBlocker("GamepadDisconnectionBox");

	private const string _titleTextKey = "Consoles/GUI_DISCONNECTION_BOX_TITLE";

	private string MessageTextKey => "Consoles/GUI_DISCONNECTION_BOX_MESSAGE";

	public UIWindow Window => _uiWindow;

	protected override void Awake()
	{
		base.Awake();
		InitGamepadInput();
	}

	[UsedImplicitly]
	private IEnumerator Start()
	{
		yield return null;
		if (PlatformLayer.GameProvider.GamepadDisconnected)
		{
			PlatformLayer.GameProvider.ShowJoystickDisconnectionMessage();
		}
	}

	protected override void OnDestroy()
	{
		DisableGamepadInput();
		Hotkey[] hotkeys = _hotkeys;
		for (int i = 0; i < hotkeys.Length; i++)
		{
			hotkeys[i].Deinitialize();
		}
		base.OnDestroy();
	}

	private void InitGamepadInput()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.PERSISTENT_SUBMIT, Deactivate, null, null, isPersistent: true).AddBlocker(new UIWindowOpenKeyActionBlocker(Window)));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.PERSISTENT_SUBMIT, Deactivate);
		}
	}

	public void Activate(bool pauseGame = false)
	{
		if (_uiWindow.IsOpen)
		{
			return;
		}
		InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
		Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_navigationBlocker);
		_titleText.SetTextKey("Consoles/GUI_DISCONNECTION_BOX_TITLE");
		_messageText.SetTextKey(MessageTextKey);
		if (InputManager.GamePadInUse)
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj in hotkeys)
			{
				obj.Initialize(Singleton<UINavigation>.Instance.Input);
				obj.DisplayHotkey(active: true);
			}
		}
		_pauseGame = pauseGame;
		if (_pauseGame && Choreographer.s_Choreographer != null && Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForMoveAnim)
		{
			Choreographer.s_Choreographer.AddUpdateBlocker();
			_blockedChoreographer = true;
		}
		Window.Show();
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.GamepadDisconnectionBox);
		Singleton<UINavigation>.Instance.StateMachine.SetFilter(new FullStateFilter());
	}

	public void Deactivate()
	{
		Singleton<UINavigation>.Instance.StateMachine.RemoveFilter();
		if (_blockedChoreographer)
		{
			if (Choreographer.s_Choreographer != null)
			{
				Choreographer.s_Choreographer.RemoveUpdateBlocker();
			}
			_blockedChoreographer = false;
		}
		_pauseGame = false;
		if (InputManager.GamePadInUse)
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj in hotkeys)
			{
				obj.Deinitialize();
				obj.DisplayHotkey(active: false);
			}
		}
		Window.Hide();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
		InputManager.SkipNextSubmitAction();
		if (CanSwitchToPreviousState())
		{
			if (Singleton<UINavigation>.Instance.StateMachine.PreviousState == null)
			{
				Singleton<UINavigation>.Instance.StateMachine.ExitCurrentState();
			}
			else
			{
				Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
			}
		}
	}

	private bool CanSwitchToPreviousState()
	{
		if (Singleton<UINavigation>.Instance.StateMachine.PreviousState != Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.GamepadDisconnectionBox))
		{
			return Singleton<UINavigation>.Instance.StateMachine.CurrentState != Singleton<UINavigation>.Instance.StateMachine.GetState(PopupStateTag.DialogueMessage);
		}
		return false;
	}
}
