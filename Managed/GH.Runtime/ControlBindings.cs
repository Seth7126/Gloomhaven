#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

[CreateAssetMenu(menuName = "FFSInput Configs/Bindings Default")]
public class ControlBindings : ScriptableObject
{
	[Serializable]
	public class ControlsMapping
	{
		public BindingDetails RotateCameraLeft;

		public BindingDetails RotateCameraRight;

		public BindingDetails PanCameraUp;

		public BindingDetails PanCameraDown;

		public BindingDetails PanCameraRight;

		public BindingDetails PanCameraLeft;

		public BindingDetails PanCameraUpMap;

		public BindingDetails PanCameraDownMap;

		public BindingDetails PanCameraRightMap;

		public BindingDetails PanCameraLeftMap;

		public BindingDetails ZoomCameraIn;

		public BindingDetails ZoomCameraOut;

		public BindingDetails PanCursorUp;

		public BindingDetails PanCursorDown;

		public BindingDetails PanCursorRight;

		public BindingDetails PanCursorLeft;

		public BindingDetails MouseRotateButton;

		public BindingDetails MousePanButton;

		public BindingDetails MouseUndoButton;

		public BindingDetails Highlight;

		public BindingDetails DisplayCards;

		public BindingDetails RotateTargetRight;

		public BindingDetails RotateTargetLeft;

		public BindingDetails LOSView;

		public BindingDetails RotateTarget;

		public BindingDetails ToggleSpeed;

		public BindingDetails Hero1CardsDisplay;

		public BindingDetails Hero2CardsDisplay;

		public BindingDetails Hero3CardsDisplay;

		public BindingDetails Hero4CardsDisplay;

		public BindingDetails PreviousItem;

		public BindingDetails NextItem;

		public BindingDetails HorizontalShortcutLeft;

		public BindingDetails HorizontalShortcutRight;

		public BindingDetails VerticalShortcutUp;

		public BindingDetails VerticalShortcutDown;

		public BindingDetails ClearTargetsOrUndo;

		public BindingDetails ConfirmAction;

		public BindingDetails SkipAttack;

		public BindingDetails BurnOneCard;

		public BindingDetails ReceiveDamage;

		public BindingDetails BurnTwoCards;

		public BindingDetails HoldToPing;

		public BindingDetails UISubmit;

		public BindingDetails UICancel;

		public BindingDetails UIPreviousTab;

		public BindingDetails UINextTab;

		public BindingDetails UINextTabMercenary;

		public BindingDetails UIPause;

		public BindingDetails PreviousShieldTab;

		public BindingDetails NextShieldTab;

		public BindingDetails UIRLeft;

		public BindingDetails UIRRight;

		public BindingDetails UIInfo;

		public BindingDetails UIRetry;

		public BindingDetails UIRename;

		public BindingDetails UIFurtherAbilityCard;

		public BindingDetails UIResetMercenary;

		public BindingDetails ControlAbilities;

		public BindingDetails ControlInitiativeTrack;

		public BindingDetails ControlCombatLog;

		public BindingDetails ControlDeck;

		public BindingDetails ControlQuestLog;

		public BindingDetails ControlPartyPanel;

		public BindingDetails CreateNewCharacter;

		public BindingDetails DeleteCharacter;

		public BindingDetails ConcealPQ;

		public BindingDetails SwitchSkin;

		public BindingDetails CreatorShowAbilityCards;

		public BindingDetails ControlLocalOptionsLeft;

		public BindingDetails ControlLocalOptionsRight;

		public BindingDetails NavigateShopMode;

		public BindingDetails UIMenuAlt1;

		public BindingDetails UIMenuDelete;

		public BindingDetails MenuEnter;

		public BindingDetails ConfirmNotification;

		public BindingDetails PreviousMercenaryOption;

		public BindingDetails NextMercenaryOption;

		public BindingDetails NextShopOption;

		public BindingDetails PreviousShopOption;

		public BindingDetails NextShopOptionDpad;

		public BindingDetails PreviousShopOptionDpad;

		public BindingDetails EndSelection;

		public BindingDetails ConfirmTargets;

		public BindingDetails ToggleVoiceChatControl;

		public BindingDetails UITips;

		public BindingDetails DLCPromotion;

		public BindingDetails DLCPromotionXbox;

		public BindingDetails SwitchProfile;

		public BindingDetails ViewProfile;

		public BindingDetails InviteUser;

		public BindingDetails UIStartingPerks;

		public BindingDetails UIMercenaryInfo;

		public BindingDetails DebugLeftStick;

		public BindingDetails DebugRightStick;

		public BindingDetails DebugButtonEast;

		public BindingDetails DebugButtonSouth;

		public BindingDetails UIPersistentSubmit;

		public BindingDetails RotateTargetButton;

		public BindingDetails UIEdit;

		public BindingDetails UIEnter;

		public BindingDetails UiKickPlayer;

		public BindingDetails UIAllCards;

		public BindingDetails ConfirmPartyAction;

		public BindingDetails ReportPlayerActionAction;

		public void MapBindingsToPlayerControls(GHControls playerControls, Dictionary<KeyAction, KeyCode> savedKeyBindingsToUse = null)
		{
			bool flag = false;
			foreach (KeyAction value in Enum.GetValues(typeof(KeyAction)))
			{
				if (value == KeyAction.None)
				{
					continue;
				}
				flag = false;
				if (savedKeyBindingsToUse != null && savedKeyBindingsToUse.ContainsKey(value))
				{
					Key keyForKeyCode = UnityKeyboardProvider.GetKeyForKeyCode(savedKeyBindingsToUse[value]);
					if (keyForKeyCode != Key.None)
					{
						playerControls.GetPlayerActionForKeyAction(value).AddBinding(new KeyBindingSource(keyForKeyCode));
						flag = true;
					}
					else
					{
						Mouse mouse = UnityKeyboardProvider.GeMouseForKeyCode(savedKeyBindingsToUse[value]);
						if (mouse != Mouse.None)
						{
							playerControls.GetPlayerActionForKeyAction(value).AddBinding(new MouseBindingSource(mouse));
							flag = true;
						}
					}
				}
				if (!flag)
				{
					GetBindingForKeyAction(value).AddBindingToControl(playerControls.GetPlayerActionForKeyAction(value));
				}
			}
		}

		public void MapRemovingFromPlayerControls(GHControls playerControls, Dictionary<KeyAction, KeyCode> savedKeyBindingsToUse = null)
		{
			bool flag = false;
			foreach (KeyAction value in Enum.GetValues(typeof(KeyAction)))
			{
				if (value == KeyAction.None)
				{
					continue;
				}
				flag = false;
				if (savedKeyBindingsToUse != null && savedKeyBindingsToUse.ContainsKey(value))
				{
					Key keyForKeyCode = UnityKeyboardProvider.GetKeyForKeyCode(savedKeyBindingsToUse[value]);
					if (keyForKeyCode != Key.None)
					{
						playerControls.GetPlayerActionForKeyAction(value).RemoveBinding(keyForKeyCode);
						flag = true;
					}
					else
					{
						Mouse mouse = UnityKeyboardProvider.GeMouseForKeyCode(savedKeyBindingsToUse[value]);
						if (mouse != Mouse.None)
						{
							playerControls.GetPlayerActionForKeyAction(value).RemoveBinding(mouse);
							flag = true;
						}
					}
				}
				if (!flag)
				{
					BindingDetails bindingForKeyAction = GetBindingForKeyAction(value);
					PlayerAction playerActionForKeyAction = playerControls.GetPlayerActionForKeyAction(value);
					bindingForKeyAction.RemoveBindingFromControl(playerActionForKeyAction);
				}
			}
		}

		public BindingDetails GetBindingForKeyAction(KeyAction keyAction)
		{
			switch (keyAction)
			{
			case KeyAction.ROTATE_CAMERA_LEFT:
				return RotateCameraLeft;
			case KeyAction.ROTATE_CAMERA_RIGHT:
				return RotateCameraRight;
			case KeyAction.MOVE_CAMERA_UP:
				return PanCameraUp;
			case KeyAction.MOVE_CAMERA_DOWN:
				return PanCameraDown;
			case KeyAction.MOVE_CAMERA_RIGHT:
				return PanCameraRight;
			case KeyAction.MOVE_CAMERA_LEFT:
				return PanCameraLeft;
			case KeyAction.MOVE_CAMERA_UP_MAP:
				return PanCameraUpMap;
			case KeyAction.MOVE_CAMERA_DOWN_MAP:
				return PanCameraDownMap;
			case KeyAction.MOVE_CAMERA_RIGHT_MAP:
				return PanCameraRightMap;
			case KeyAction.MOVE_CAMERA_LEFT_MAP:
				return PanCameraLeftMap;
			case KeyAction.ZOOM_IN_CAMERA:
				return ZoomCameraIn;
			case KeyAction.ZOOM_OUT_CAMERA:
				return ZoomCameraOut;
			case KeyAction.HIGHLIGHT:
				return Highlight;
			case KeyAction.DISPLAY_CARDS:
				return DisplayCards;
			case KeyAction.ROTATE_TARGET_BUTTON:
				return RotateTargetButton;
			case KeyAction.UI_ENTER:
				return UIEnter;
			case KeyAction.UI_EDIT:
				return UIEdit;
			case KeyAction.LOS_VIEW:
				return LOSView;
			case KeyAction.ROTATE_TARGET:
				return RotateTarget;
			case KeyAction.DISPLAY_CARDS_HERO_1:
				return Hero1CardsDisplay;
			case KeyAction.DISPLAY_CARDS_HERO_2:
				return Hero2CardsDisplay;
			case KeyAction.DISPLAY_CARDS_HERO_3:
				return Hero3CardsDisplay;
			case KeyAction.DISPLAY_CARDS_HERO_4:
				return Hero4CardsDisplay;
			case KeyAction.ROTATE_CAMERA_WITH_MOUSE:
				return MouseRotateButton;
			case KeyAction.MOVE_CAMERA_WITH_MOUSE:
				return MousePanButton;
			case KeyAction.UNDO_BUTTON:
				return MouseUndoButton;
			case KeyAction.CLEAR_TARGETS_OR_UNDO:
				return ClearTargetsOrUndo;
			case KeyAction.CONFIRM_ACTION_BUTTON:
				return ConfirmAction;
			case KeyAction.SKIP_ATTACK:
				return SkipAttack;
			case KeyAction.BURN_ONE_CARD:
				return BurnOneCard;
			case KeyAction.RECEIVE_DAMAGE:
				return ReceiveDamage;
			case KeyAction.BURN_TWO_CARDS:
				return BurnTwoCards;
			case KeyAction.HOLD_TO_PING:
				return HoldToPing;
			case KeyAction.UI_CANCEL:
				return UICancel;
			case KeyAction.UI_SUBMIT:
				return UISubmit;
			case KeyAction.CONTROL_INITIATVE_TRACK:
				return ControlInitiativeTrack;
			case KeyAction.UI_NEXT_TAB:
				return UINextTab;
			case KeyAction.UI_PREVIOUS_TAB:
				return UIPreviousTab;
			case KeyAction.CONTROL_COMBAT_LOG:
				return ControlCombatLog;
			case KeyAction.CONTROL_DECK:
				return ControlDeck;
			case KeyAction.CONTROL_QUEST_LOG:
				return ControlQuestLog;
			case KeyAction.CONTROL_PARTY_PANEL:
				return ControlPartyPanel;
			case KeyAction.CREATE_NEW_CHARACTER:
				return CreateNewCharacter;
			case KeyAction.DELETE_CHARACTER:
				return DeleteCharacter;
			case KeyAction.CONCEAL_PQ:
				return ConcealPQ;
			case KeyAction.SWITCH_SKIN:
				return SwitchSkin;
			case KeyAction.CREATOR_SHOW_ABILITY_CARDS:
				return CreatorShowAbilityCards;
			case KeyAction.CONTROL_LOCAL_OPTIONS_LEFT:
				return ControlLocalOptionsLeft;
			case KeyAction.CONTROL_LOCAL_OPTIONS_RIGHT:
				return ControlLocalOptionsRight;
			case KeyAction.NAVIGATE_SHOP_MODE:
				return NavigateShopMode;
			case KeyAction.UI_MENU_ALT1:
				return UIMenuAlt1;
			case KeyAction.UI_MENU_DELETE:
				return UIMenuDelete;
			case KeyAction.MENU_ENTER:
				return MenuEnter;
			case KeyAction.UI_PAUSE:
				return UIPause;
			case KeyAction.CONFIRM_NOTIFICATION:
				return ConfirmNotification;
			case KeyAction.TOGGLE_SPEED:
				return ToggleSpeed;
			case KeyAction.PREVIOUS_ITEM:
				return PreviousItem;
			case KeyAction.NEXT_ITEM:
				return NextItem;
			case KeyAction.NEXT_SHIELD_TAB:
				return NextShieldTab;
			case KeyAction.PREVIOUS_SHIELD_TAB:
				return PreviousShieldTab;
			case KeyAction.NEXT_MERCENARY_OPTION:
				return NextMercenaryOption;
			case KeyAction.PREVIOUS_MERCENARY_OPTION:
				return PreviousMercenaryOption;
			case KeyAction.NEXT_SHOP_OPTION_TAB:
				return NextShopOption;
			case KeyAction.PREVIOUS_SHOP_OPTION_TAB:
				return PreviousShopOption;
			case KeyAction.NEXT_SHOP_OPTION_TAB_DPAD:
				return NextShopOptionDpad;
			case KeyAction.PREVIOUS_SHOP_OPTION_TAB_DPAD:
				return PreviousShopOptionDpad;
			case KeyAction.UI_R_LEFT:
				return UIRLeft;
			case KeyAction.UI_R_RIGHT:
				return UIRRight;
			case KeyAction.UI_INFO:
				return UIInfo;
			case KeyAction.UI_RETRY:
				return UIRetry;
			case KeyAction.UI_RENAME:
				return UIRename;
			case KeyAction.UI_FURTHER_ABILITY_CARD:
				return UIFurtherAbilityCard;
			case KeyAction.TOGGLE_VOICE_CHAT_CONTROL:
				return ToggleVoiceChatControl;
			case KeyAction.DEBUG_LEFT_STICK:
				return DebugLeftStick;
			case KeyAction.DEBUG_RIGHT_STICK:
				return DebugRightStick;
			case KeyAction.DEBUG_BUTTON_EAST:
				return DebugButtonEast;
			case KeyAction.DEBUG_BUTTON_SOUTH:
				return DebugButtonSouth;
			case KeyAction.UI_TIPS:
				return UITips;
			case KeyAction.UI_DLC_PROMOTION:
				return DLCPromotion;
			case KeyAction.UI_DLC_PROMOTION_XBOX:
				return DLCPromotionXbox;
			case KeyAction.SWITCH_PROFILE:
				return SwitchProfile;
			case KeyAction.VIEW_PROFILE:
				return ViewProfile;
			case KeyAction.INVITE_USER:
				return InviteUser;
			case KeyAction.UI_STARTING_PERKS:
				return UIStartingPerks;
			case KeyAction.UI_MERCENARY_INFO:
				return UIMercenaryInfo;
			case KeyAction.PERSISTENT_SUBMIT:
				return UIPersistentSubmit;
			case KeyAction.UI_Reset_Mercenary:
				return UIResetMercenary;
			case KeyAction.UI_NEXT_TAB_MERCENARY:
				return UINextTabMercenary;
			case KeyAction.UI_KICK_PLAYER:
				return UiKickPlayer;
			case KeyAction.UI_ALL_CARDS:
				return UIAllCards;
			case KeyAction.HORIZONTAL_SHORTCUT_LEFT:
				return HorizontalShortcutLeft;
			case KeyAction.HORIZONTAL_SHORTCUT_RIGHT:
				return HorizontalShortcutRight;
			case KeyAction.VERTICAL_SHORTCUT_UP:
				return VerticalShortcutUp;
			case KeyAction.VERTICAL_SHORTCUT_DOWN:
				return VerticalShortcutDown;
			case KeyAction.CONFIRM_PARTY_ACTION:
				return ConfirmPartyAction;
			case KeyAction.UI_REPORT_PLAYER:
				return ReportPlayerActionAction;
			default:
				Debug.LogWarningFormat("[INPUT MANAGER] - Key Action \"{0}\" not supported currently, please update this method.", keyAction.ToString());
				return null;
			}
		}
	}

	[Header("Default Bindings")]
	public ControlsMapping MouseKeyboardDefaultBindings;

	public ControlsMapping XboxGamepadDefaultBindings;
}
