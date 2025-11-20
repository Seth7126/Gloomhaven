using System;
using System.Collections.Generic;
using InControl;

public class GHControls : PlayerActionSet
{
	public class GHAction : PlayerAction
	{
		private bool hasPressedDown;

		public override bool Enabled
		{
			set
			{
				base.Enabled = value;
				if (value)
				{
					hasPressedDown = base.IsPressed;
				}
			}
		}

		public event Action OnPressed;

		public event Action OnReleased;

		public event Action OnValueChanged;

		public GHAction(string name, PlayerActionSet owner)
			: base(name, owner)
		{
			this.OnPressed = null;
		}

		public void CheckInput()
		{
			if (!Enabled)
			{
				hasPressedDown = false;
				return;
			}
			if (base.IsPressed && !hasPressedDown)
			{
				hasPressedDown = true;
				this.OnPressed?.Invoke();
			}
			if (!base.IsPressed && hasPressedDown)
			{
				hasPressedDown = false;
			}
			if (base.WasReleased)
			{
				this.OnReleased?.Invoke();
			}
			if (base.HasChanged)
			{
				this.OnValueChanged?.Invoke();
			}
		}

		public void SimulateOnPress()
		{
			this.OnPressed?.Invoke();
		}
	}

	public PlayerAction MouseUp;

	public PlayerAction MouseDown;

	public PlayerAction MouseLeft;

	public PlayerAction MouseRight;

	public PlayerTwoAxisAction DeltaMouse;

	public PlayerAction MouseWheelUp;

	public PlayerAction MouseWheelDown;

	public PlayerOneAxisAction MouseWheelDelta;

	public PlayerAction MouseClickLeft;

	public PlayerAction MouseClickRight;

	public PlayerAction MouseClickMiddle;

	public PlayerAction MouseClickUndo;

	public GHAction UISubmit;

	public GHAction UICancel;

	public PlayerAction UIAlt1;

	public PlayerAction UIAlt2;

	public PlayerAction UILeft;

	public PlayerAction UIRight;

	public PlayerAction UIUp;

	public PlayerAction UIDown;

	public PlayerTwoAxisAction UIMove;

	public PlayerAction UILeftDPad;

	public PlayerAction UIRightDPad;

	public PlayerAction UIUpDPad;

	public PlayerAction UIDownDPad;

	public PlayerTwoAxisAction UIMoveDPad;

	public GHAction UINextTab;

	public GHAction UIPreviousTab;

	public GHAction UINextTabMercenary;

	public PlayerAction UILeftAlt;

	public PlayerAction UIRightAlt;

	public PlayerAction UIUpAlt;

	public PlayerAction UIDownAlt;

	public PlayerTwoAxisAction UIMoveAlt;

	public PlayerAction UIPause;

	public GHAction UIRLeft;

	public GHAction UIRRight;

	public GHAction UIInfo;

	public GHAction UIRetry;

	public GHAction UIEnter;

	public GHAction UIEdit;

	public GHAction UIRename;

	public GHAction UIAllCards;

	public GHAction ToggleVoiceChatControl;

	public GHAction UITips;

	public GHAction UiDlcPromotion;

	public GHAction UiDlcPromotionXbox;

	public GHAction UISwitchProfile;

	public GHAction UIInviteUser;

	public GHAction UIViewProfile;

	public GHAction UIStartingPerks;

	public GHAction UIMercenaryInfo;

	public GHAction UIFurtherAbilityCard;

	public GHAction UIResetMercenary;

	public GHAction UiKickPlayer;

	public GHAction UiReportPlayer;

	public PlayerAction PanCameraLeft;

	public PlayerAction PanCameraRight;

	public PlayerAction PanCameraDown;

	public PlayerAction PanCameraUp;

	public PlayerTwoAxisAction PanCamera;

	public PlayerAction RotateCameraLeft;

	public PlayerAction RotateCameraRight;

	public PlayerOneAxisAction RotateCamera;

	public PlayerAction ZoomCameraIn;

	public PlayerAction ZoomCameraOut;

	public PlayerOneAxisAction ZoomCamera;

	public PlayerAction MouseCameraPan;

	public PlayerAction MouseCameraRotate;

	public PlayerAction Highlight;

	public PlayerAction DisplayCards;

	public PlayerAction RotateTargetRight;

	public PlayerAction RotateTargetLeft;

	public PlayerAction RotateTargetButton;

	public PlayerAction LOSView;

	public PlayerAction RotateTarget;

	public PlayerAction Hero1CardsDisplay;

	public PlayerAction Hero2CardsDisplay;

	public PlayerAction Hero3CardsDisplay;

	public PlayerAction Hero4CardsDisplay;

	public PlayerAction ClearTargetsOrUndo;

	public GHAction ConfirmAction;

	public GHAction SkipAttack;

	public GHAction BurnOneCard;

	public PlayerAction ReceiveDamage;

	public PlayerAction BurnTwoCards;

	public PlayerAction HoldToPing;

	public GHAction ControlInitiativeTrack;

	public GHAction ControlCombatLog;

	public GHAction ControlDeck;

	public PlayerAction ToggleSpeed;

	public PlayerAction DebugMenuKey;

	public GHAction PreviousItem;

	public GHAction NextItem;

	public GHAction HorizontalShortcutLeft;

	public GHAction HorizontalShortcutRight;

	public GHAction VerticalShortcutUp;

	public GHAction VerticalShortcutDown;

	public PlayerAction ControlQuestLog;

	public GHAction CreateNewCharacter;

	public GHAction SwitchSkin;

	public GHAction ConcealPQ;

	public GHAction DeleteCharacter;

	public GHAction CreatorShowAbilityCards;

	public PlayerAction ControlParty;

	public PlayerAction CreatorLocalOptionsLeft;

	public PlayerAction CreatorLocalOptionsRight;

	public GHAction NavigateShopMode;

	public PlayerAction PanCameraLeftMap;

	public PlayerAction PanCameraRightMap;

	public PlayerAction PanCameraDownMap;

	public PlayerAction PanCameraUpMap;

	public PlayerTwoAxisAction PanCameraMap;

	public PlayerAction NextShieldTab;

	public PlayerAction PreviousShieldTab;

	public GHAction NextMercenaryOption;

	public GHAction PreviousMercenaryOption;

	public GHAction NextShopOption;

	public GHAction PreviousShopOption;

	public GHAction NextShopOptionDpad;

	public GHAction PreviousShopOptionDpad;

	public GHAction ConfirmPartyAction;

	public PlayerAction UIMenuAlt1;

	public PlayerAction UIMenuDelete;

	public PlayerAction MenuEnter;

	public PlayerAction FreeCamRotateUp;

	public PlayerAction FreeCamRotateDown;

	public PlayerAction FreeCamRotateLeft;

	public PlayerAction FreeCamRotateRight;

	public PlayerTwoAxisAction FreeCamRotate;

	public PlayerAction FreeCamMoveUp;

	public PlayerAction FreeCamMoveDown;

	public PlayerOneAxisAction FreeCamMoveHorizontal;

	public PlayerAction ConfirmNotification;

	public PlayerAction DebugLeftStick;

	public PlayerAction DebugRightStick;

	public PlayerAction DebugButtonEast;

	public PlayerAction DebugButtonSouth;

	public GHAction UIPersistentSubmit;

	private Dictionary<KeyAction, PlayerAction> m_KeyActionsAssigned;

	private List<GHAction> m_ClickActions;

	private Dictionary<PlayerAction, string> _handledActions = new Dictionary<PlayerAction, string>();

	public GHControls()
	{
		m_KeyActionsAssigned = new Dictionary<KeyAction, PlayerAction>();
		m_ClickActions = new List<GHAction>();
		MouseUp = CreatePlayerAction("Mouse Up");
		MouseDown = CreatePlayerAction("Mouse Down");
		MouseLeft = CreatePlayerAction("Mouse Left");
		MouseRight = CreatePlayerAction("Mouse Right");
		DeltaMouse = CreateTwoAxisPlayerAction(MouseLeft, MouseRight, MouseDown, MouseUp);
		MouseWheelUp = CreatePlayerAction("Mouse Wheel Up");
		MouseWheelDown = CreatePlayerAction("Mouse Wheel Down");
		MouseWheelDelta = CreateOneAxisPlayerAction(MouseWheelDown, MouseWheelUp);
		MouseClickLeft = CreatePlayerAction("Mouse Click Left");
		MouseClickRight = CreatePlayerAction("Mouse Click Right");
		MouseClickMiddle = CreatePlayerAction("Mouse Click Middle");
		MouseClickUndo = CreatePlayerAction("Mouse Click Undo", KeyAction.UNDO_BUTTON);
		ConfirmAction = CreateGHAction("Confirm Action", KeyAction.CONFIRM_ACTION_BUTTON);
		UISubmit = CreateGHAction("Submit", KeyAction.UI_SUBMIT);
		UICancel = CreateGHAction("Cancel", KeyAction.UI_CANCEL);
		UIAlt1 = CreatePlayerAction("Alternate Action 1");
		UIAlt2 = CreatePlayerAction("Alternate Action 2");
		UILeft = CreatePlayerAction("UI Move Left");
		UIRight = CreatePlayerAction("UI Move Right");
		UIUp = CreatePlayerAction("UI Move Up");
		UIDown = CreatePlayerAction("UI Move Down");
		UIMove = CreateTwoAxisPlayerAction(UILeft, UIRight, UIDown, UIUp);
		UILeftDPad = CreatePlayerAction("UI Move Left DPad");
		UIRightDPad = CreatePlayerAction("UI Move Right DPad");
		UIUpDPad = CreatePlayerAction("UI Move Up DPad");
		UIDownDPad = CreatePlayerAction("UI Move Down DPad");
		UIMoveDPad = CreateTwoAxisPlayerAction(UILeftDPad, UIRightDPad, UIDownDPad, UIUpDPad);
		UINextTab = CreateGHAction("UI Next Tab", KeyAction.UI_NEXT_TAB);
		UIPreviousTab = CreateGHAction("UI Previous Tab", KeyAction.UI_PREVIOUS_TAB);
		UINextTabMercenary = CreateGHAction("UI Next Tab Mercenary", KeyAction.UI_NEXT_TAB_MERCENARY);
		UiKickPlayer = CreateGHAction("Ui Kick Player", KeyAction.UI_KICK_PLAYER);
		UiReportPlayer = CreateGHAction("Ui Report Player", KeyAction.UI_REPORT_PLAYER);
		UIRLeft = CreateGHAction("Switch_Left", KeyAction.UI_R_LEFT);
		UIRLeft.StateThreshold = 0.7f;
		UIRRight = CreateGHAction("Switch_Right", KeyAction.UI_R_RIGHT);
		UIRRight.StateThreshold = 0.7f;
		UIInfo = CreateGHAction("Info", KeyAction.UI_INFO);
		UIRetry = CreateGHAction("Retry", KeyAction.UI_RETRY);
		UIEdit = CreateGHAction("Edit", KeyAction.UI_EDIT);
		UIEnter = CreateGHAction("Enter", KeyAction.UI_ENTER);
		UIRename = CreateGHAction("Rename", KeyAction.UI_RENAME);
		UIAllCards = CreateGHAction("AllCards", KeyAction.UI_ALL_CARDS);
		ToggleVoiceChatControl = CreateGHAction("ToggleVoiceChatControl", KeyAction.TOGGLE_VOICE_CHAT_CONTROL);
		UITips = CreateGHAction("Tips", KeyAction.UI_TIPS);
		UiDlcPromotion = CreateGHAction("DLC Promotion", KeyAction.UI_DLC_PROMOTION);
		UiDlcPromotionXbox = CreateGHAction("DLC Promotion Xbox", KeyAction.UI_DLC_PROMOTION_XBOX);
		UISwitchProfile = CreateGHAction("Switch Profile", KeyAction.SWITCH_PROFILE);
		UIInviteUser = CreateGHAction("Invite User", KeyAction.INVITE_USER);
		UIViewProfile = CreateGHAction("View Profile", KeyAction.VIEW_PROFILE);
		UIStartingPerks = CreateGHAction("Starting Perks", KeyAction.UI_STARTING_PERKS);
		UIMercenaryInfo = CreateGHAction("MercenaryInfo", KeyAction.UI_MERCENARY_INFO);
		UIFurtherAbilityCard = CreateGHAction("FurtherAbilityCard", KeyAction.UI_FURTHER_ABILITY_CARD);
		UIResetMercenary = CreateGHAction("ResetMercenary", KeyAction.UI_Reset_Mercenary);
		UILeftAlt = CreatePlayerAction("UI Move Left Alt");
		UIRightAlt = CreatePlayerAction("UI Move Right Alt");
		UIUpAlt = CreatePlayerAction("UI Move Up Alt");
		UIDownAlt = CreatePlayerAction("UI Move Down Alt");
		UIMoveAlt = CreateTwoAxisPlayerAction(UILeftAlt, UIRightAlt, UIDownAlt, UIUpAlt);
		UIPause = CreateGHAction("UI Pause", KeyAction.UI_PAUSE);
		PanCameraLeft = CreatePlayerAction("Pan Camera Left", KeyAction.MOVE_CAMERA_LEFT);
		PanCameraRight = CreatePlayerAction("Pan Camera Right", KeyAction.MOVE_CAMERA_RIGHT);
		PanCameraDown = CreatePlayerAction("Pan Camera Down", KeyAction.MOVE_CAMERA_DOWN);
		PanCameraUp = CreatePlayerAction("Pan Camera Up", KeyAction.MOVE_CAMERA_UP);
		PanCamera = CreateTwoAxisPlayerAction(PanCameraLeft, PanCameraRight, PanCameraDown, PanCameraUp);
		RotateCameraLeft = CreatePlayerAction("Rotate Camera Left", KeyAction.ROTATE_CAMERA_LEFT);
		RotateCameraRight = CreatePlayerAction("Rotate Camera Right", KeyAction.ROTATE_CAMERA_RIGHT);
		RotateCamera = CreateOneAxisPlayerAction(RotateCameraRight, RotateCameraLeft);
		ZoomCameraOut = CreatePlayerAction("Zoom Out", KeyAction.ZOOM_OUT_CAMERA);
		ZoomCameraIn = CreatePlayerAction("Zoom In", KeyAction.ZOOM_IN_CAMERA);
		ZoomCamera = CreateOneAxisPlayerAction(ZoomCameraOut, ZoomCameraIn);
		MouseCameraPan = CreatePlayerAction("Mouse Camera Pan", KeyAction.MOVE_CAMERA_WITH_MOUSE);
		MouseCameraRotate = CreatePlayerAction("Mouse Camera Rotate", KeyAction.ROTATE_CAMERA_WITH_MOUSE);
		HorizontalShortcutLeft = CreateGHAction("HorizontalShortcutLeft", KeyAction.HORIZONTAL_SHORTCUT_LEFT);
		HorizontalShortcutRight = CreateGHAction("HorizontalShortcutRight", KeyAction.HORIZONTAL_SHORTCUT_RIGHT);
		VerticalShortcutUp = CreateGHAction("VerticalShortcutUp", KeyAction.VERTICAL_SHORTCUT_UP);
		VerticalShortcutDown = CreateGHAction("VerticalShortcutDown", KeyAction.VERTICAL_SHORTCUT_DOWN);
		Highlight = CreatePlayerAction("Highlight", KeyAction.HIGHLIGHT);
		DisplayCards = CreatePlayerAction("Display Cards", KeyAction.DISPLAY_CARDS);
		RotateTargetButton = CreatePlayerAction("Rotate Target Button", KeyAction.ROTATE_TARGET_BUTTON);
		LOSView = CreatePlayerAction("LOS View", KeyAction.LOS_VIEW);
		RotateTarget = CreatePlayerAction("Rotate Target", KeyAction.ROTATE_TARGET);
		ToggleSpeed = CreateGHAction("Toggle Speed", KeyAction.TOGGLE_SPEED);
		Hero1CardsDisplay = CreateGHAction("Hero 1 Cards Display", KeyAction.DISPLAY_CARDS_HERO_1);
		Hero2CardsDisplay = CreateGHAction("Hero 2 Cards Display", KeyAction.DISPLAY_CARDS_HERO_2);
		Hero3CardsDisplay = CreateGHAction("Hero 3 Cards Display", KeyAction.DISPLAY_CARDS_HERO_3);
		Hero4CardsDisplay = CreateGHAction("Hero 4 Cards Display", KeyAction.DISPLAY_CARDS_HERO_4);
		ClearTargetsOrUndo = CreatePlayerAction("Clear Targets or Undo", KeyAction.CLEAR_TARGETS_OR_UNDO);
		SkipAttack = CreateGHAction("Skip Attack", KeyAction.SKIP_ATTACK);
		BurnOneCard = CreateGHAction("Burn One Card", KeyAction.BURN_ONE_CARD);
		ReceiveDamage = CreatePlayerAction("Receive Damage", KeyAction.RECEIVE_DAMAGE);
		BurnTwoCards = CreatePlayerAction("Burn Two Cards", KeyAction.BURN_TWO_CARDS);
		HoldToPing = CreatePlayerAction("Hold To Ping", KeyAction.HOLD_TO_PING);
		ControlInitiativeTrack = CreateGHAction("Control Initiative Track", KeyAction.CONTROL_INITIATVE_TRACK);
		ControlCombatLog = CreateGHAction("Control Combat Log", KeyAction.CONTROL_COMBAT_LOG);
		ControlDeck = CreateGHAction("Control Deck", KeyAction.CONTROL_DECK);
		PreviousItem = CreateGHAction("Previous Item", KeyAction.PREVIOUS_ITEM);
		NextItem = CreateGHAction("Next Item", KeyAction.NEXT_ITEM);
		DebugMenuKey = CreatePlayerAction("Debug Menu Shortcut");
		ControlQuestLog = CreateGHAction("Control Quest Log", KeyAction.CONTROL_QUEST_LOG);
		ControlParty = CreateGHAction("Control Party", KeyAction.CONTROL_PARTY_PANEL);
		CreateNewCharacter = CreateGHAction("Create new character", KeyAction.CREATE_NEW_CHARACTER);
		DeleteCharacter = CreateGHAction("Delete character", KeyAction.DELETE_CHARACTER);
		ConcealPQ = CreateGHAction("Conceal PQ", KeyAction.CONCEAL_PQ);
		SwitchSkin = CreateGHAction("Switch Skin", KeyAction.SWITCH_SKIN);
		CreatorShowAbilityCards = CreateGHAction("Starting Ability Cards", KeyAction.CREATOR_SHOW_ABILITY_CARDS);
		CreatorLocalOptionsLeft = CreateGHAction("Control Local Left", KeyAction.CONTROL_LOCAL_OPTIONS_LEFT);
		CreatorLocalOptionsRight = CreateGHAction("Control Local Right", KeyAction.CONTROL_LOCAL_OPTIONS_RIGHT);
		NavigateShopMode = CreateGHAction("Navigate shop mode", KeyAction.NAVIGATE_SHOP_MODE);
		PanCameraLeftMap = CreatePlayerAction("Pan Camera Left Map", KeyAction.MOVE_CAMERA_LEFT_MAP);
		PanCameraRightMap = CreatePlayerAction("Pan Camera Right Map", KeyAction.MOVE_CAMERA_RIGHT_MAP);
		PanCameraDownMap = CreatePlayerAction("Pan Camera Down Map", KeyAction.MOVE_CAMERA_DOWN_MAP);
		PanCameraUpMap = CreatePlayerAction("Pan Camera Up Map", KeyAction.MOVE_CAMERA_UP_MAP);
		PanCameraMap = CreateTwoAxisPlayerAction(PanCameraLeftMap, PanCameraRightMap, PanCameraDownMap, PanCameraUpMap);
		NextShieldTab = CreateGHAction("Next Shield Tab", KeyAction.NEXT_SHIELD_TAB);
		PreviousShieldTab = CreateGHAction("Previous Shield Tab", KeyAction.PREVIOUS_SHIELD_TAB);
		NextMercenaryOption = CreateGHAction("Next Mercenary Option", KeyAction.NEXT_MERCENARY_OPTION);
		PreviousMercenaryOption = CreateGHAction("Previous Mercenary Option", KeyAction.PREVIOUS_MERCENARY_OPTION);
		NextShopOption = CreateGHAction("Next Shop Option", KeyAction.NEXT_SHOP_OPTION_TAB);
		PreviousShopOption = CreateGHAction("Previous Shop Option", KeyAction.PREVIOUS_SHOP_OPTION_TAB);
		NextShopOptionDpad = CreateGHAction("Next Shop Option Dpad", KeyAction.NEXT_SHOP_OPTION_TAB_DPAD);
		NextShopOptionDpad.StateThreshold = 0.95f;
		PreviousShopOptionDpad = CreateGHAction("Previous Shop Option Dpad", KeyAction.PREVIOUS_SHOP_OPTION_TAB_DPAD);
		PreviousShopOptionDpad.StateThreshold = 0.95f;
		ConfirmPartyAction = CreateGHAction("Confirm Party Action", KeyAction.CONFIRM_PARTY_ACTION);
		UIMenuAlt1 = CreateGHAction("UI Menu Alt1", KeyAction.UI_MENU_ALT1);
		UIMenuDelete = CreateGHAction("UI Menu Delete", KeyAction.UI_MENU_DELETE);
		MenuEnter = CreateGHAction("Menu Enter", KeyAction.MENU_ENTER);
		FreeCamRotateUp = CreatePlayerAction("Free Cam Rotate Up");
		FreeCamRotateDown = CreatePlayerAction("Free Cam Rotate Down");
		FreeCamRotateLeft = CreatePlayerAction("Free Cam Rotate Left");
		FreeCamRotateRight = CreatePlayerAction("Free Cam Rotate Right");
		FreeCamRotate = CreateTwoAxisPlayerAction(FreeCamRotateLeft, FreeCamRotateRight, FreeCamRotateDown, FreeCamRotateUp);
		FreeCamMoveUp = CreatePlayerAction("Free Cam Move Up");
		FreeCamMoveDown = CreatePlayerAction("Free Cam Move Down");
		FreeCamMoveHorizontal = CreateOneAxisPlayerAction(FreeCamMoveDown, FreeCamMoveUp);
		ConfirmNotification = CreateGHAction("Accept Notification", KeyAction.CONFIRM_NOTIFICATION);
		DebugLeftStick = CreateGHAction("Debug left stick", KeyAction.DEBUG_LEFT_STICK);
		DebugRightStick = CreateGHAction("Debug right stick", KeyAction.DEBUG_RIGHT_STICK);
		DebugButtonEast = CreateGHAction("Debug button east", KeyAction.DEBUG_BUTTON_EAST);
		DebugButtonSouth = CreateGHAction("Debug button south", KeyAction.DEBUG_BUTTON_SOUTH);
		UIPersistentSubmit = CreateGHAction("PersistentSubmit", KeyAction.PERSISTENT_SUBMIT);
	}

	private void SwitchKeyActions(KeyAction firstAction, KeyAction secondAction)
	{
		PlayerAction playerAction = m_KeyActionsAssigned[firstAction];
		bool enabled = playerAction.Enabled;
		PlayerAction playerAction2 = m_KeyActionsAssigned[secondAction];
		bool enabled2 = playerAction2.Enabled;
		m_KeyActionsAssigned[firstAction] = playerAction2;
		m_KeyActionsAssigned[secondAction] = playerAction;
		m_KeyActionsAssigned[firstAction].Enabled = enabled;
		m_KeyActionsAssigned[secondAction].Enabled = enabled2;
	}

	private void ReplaceBindings(PlayerAction inAction, InputControlType fromInput, InputControlType toInput)
	{
		DeviceBindingSource findBinding = null;
		foreach (BindingSource binding in inAction.Bindings)
		{
			if (binding is DeviceBindingSource deviceBindingSource && deviceBindingSource.Control == fromInput)
			{
				findBinding = deviceBindingSource;
			}
		}
		inAction.ReplaceBinding(findBinding, new DeviceBindingSource(toInput));
	}

	public void ResetBindings()
	{
		foreach (PlayerAction value in m_KeyActionsAssigned.Values)
		{
			value.ResetBindings();
		}
	}

	private GHAction CreateGHAction(string name, KeyAction keyAction)
	{
		GHAction gHAction = new GHAction(name, this);
		m_KeyActionsAssigned[keyAction] = gHAction;
		m_ClickActions.Add(gHAction);
		return gHAction;
	}

	private PlayerAction CreatePlayerAction(string name, KeyAction keyAction)
	{
		PlayerAction playerAction = CreatePlayerAction(name);
		m_KeyActionsAssigned[keyAction] = playerAction;
		return playerAction;
	}

	public PlayerAction GetPlayerActionForKeyAction(KeyAction keyAction)
	{
		if (keyAction == KeyAction.None)
		{
			return null;
		}
		if (m_KeyActionsAssigned.ContainsKey(keyAction))
		{
			return m_KeyActionsAssigned[keyAction];
		}
		Debug.LogErrorFormat("[INPUT MANAGER] - Key Action \"{0}\" not supported currently, please update this method.", keyAction.ToString());
		return null;
	}

	public void CheckClicks()
	{
		if (Singleton<InputManager>.Instance.PlayerActionControls == null)
		{
			return;
		}
		for (int i = 0; i < m_ClickActions.Count; i++)
		{
			if (Singleton<InputManager>.Instance.PlayerActionControls.ControlIsHandled(m_ClickActions[i]))
			{
				m_ClickActions[i].ClearInputState();
			}
			else
			{
				m_ClickActions[i].CheckInput();
			}
		}
	}

	public void MarkActionAsHandled(PlayerAction action, string handler)
	{
		if (!_handledActions.ContainsKey(action))
		{
			_handledActions.Add(action, handler);
		}
	}

	public void ResetHandledActions()
	{
		_handledActions.Clear();
	}

	public bool ActionIsHandled(PlayerAction action)
	{
		return _handledActions.ContainsKey(action);
	}
}
