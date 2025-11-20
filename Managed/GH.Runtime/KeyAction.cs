public enum KeyAction
{
	None = -1,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	ROTATE_CAMERA_LEFT = 0,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	ROTATE_CAMERA_RIGHT = 1,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera }, KeyRelatedWith = new KeyAction[] { KeyAction.MOVE_CAMERA_UP_MAP })]
	MOVE_CAMERA_UP = 2,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera }, KeyRelatedWith = new KeyAction[] { KeyAction.MOVE_CAMERA_DOWN_MAP })]
	MOVE_CAMERA_DOWN = 3,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera }, KeyRelatedWith = new KeyAction[] { KeyAction.MOVE_CAMERA_RIGHT_MAP })]
	MOVE_CAMERA_RIGHT = 4,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera }, KeyRelatedWith = new KeyAction[] { KeyAction.MOVE_CAMERA_LEFT_MAP })]
	MOVE_CAMERA_LEFT = 5,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	ZOOM_IN_CAMERA = 6,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	ZOOM_OUT_CAMERA = 7,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	MOVE_CAMERA_UP_MAP = 90,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	MOVE_CAMERA_DOWN_MAP = 91,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	MOVE_CAMERA_RIGHT_MAP = 92,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Camera })]
	MOVE_CAMERA_LEFT_MAP = 93,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	HIGHLIGHT = 8,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	DISPLAY_CARDS = 9,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	LOS_VIEW = 12,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.Scenario,
		EKeyActionTag.Camera
	})]
	ROTATE_TARGET = 13,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	DISPLAY_CARDS_HERO_1 = 14,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	DISPLAY_CARDS_HERO_2 = 15,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	DISPLAY_CARDS_HERO_3 = 16,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	DISPLAY_CARDS_HERO_4 = 17,
	ROTATE_CAMERA_WITH_MOUSE = 18,
	MOVE_CAMERA_WITH_MOUSE = 19,
	UNDO_BUTTON = 20,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.Scenario,
		EKeyActionTag.UI
	})]
	CLEAR_TARGETS_OR_UNDO = 21,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	CONFIRM_ACTION_BUTTON = 22,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.Scenario,
		EKeyActionTag.UI
	})]
	SKIP_ATTACK = 23,
	[KeyAction(GroupId = "scenario1", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.Scenario,
		EKeyActionTag.UI
	})]
	BURN_ONE_CARD = 24,
	[KeyAction(GroupId = "scenario1", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.Scenario,
		EKeyActionTag.UI
	})]
	RECEIVE_DAMAGE = 25,
	[KeyAction(GroupId = "scenario1", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.Scenario,
		EKeyActionTag.UI
	})]
	BURN_TWO_CARDS = 26,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	CONTROL_DECK = 34,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	CONTROL_COMBAT_LOG = 35,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	TOGGLE_SPEED = 36,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.Scenario })]
	HOLD_TO_PING = 37,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	CONTROL_INITIATVE_TRACK = 39,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	PREVIOUS_ITEM = 110,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	NEXT_ITEM = 111,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	HORIZONTAL_SHORTCUT_LEFT = 113,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	HORIZONTAL_SHORTCUT_RIGHT = 114,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	VERTICAL_SHORTCUT_UP = 115,
	[KeyAction(GroupId = "scenario", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Scenario,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	VERTICAL_SHORTCUT_DOWN = 116,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_CANCEL = 40,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_SUBMIT = 41,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.LocalAreaControls
	})]
	UI_NEXT_TAB = 42,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.LocalAreaControls
	})]
	UI_PREVIOUS_TAB = 43,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_PAUSE = 44,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_R_LEFT = 45,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_R_RIGHT = 46,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_INFO = 47,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_RETRY = 48,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_TIPS = 49,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_DLC_PROMOTION = 108,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_DLC_PROMOTION_XBOX = 109,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_RENAME = 300,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_FURTHER_ABILITY_CARD = 301,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_Reset_Mercenary = 302,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.LocalAreaControls
	})]
	UI_NEXT_TAB_MERCENARY = 303,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_KICK_PLAYER = 304,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_ALL_CARDS = 305,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	SWITCH_PROFILE = 306,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	VIEW_PROFILE = 307,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	INVITE_USER = 308,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_REPORT_PLAYER = 309,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	CONTROL_QUEST_LOG = 50,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.AreaShortcuts
	})]
	CONTROL_PARTY_PANEL = 51,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	CREATE_NEW_CHARACTER = 52,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	CONCEAL_PQ = 53,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	SWITCH_SKIN = 54,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	DELETE_CHARACTER = 55,
	[KeyAction(GroupId = "creator class", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	CREATOR_SHOW_ABILITY_CARDS = 56,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.LocalAreaControls
	})]
	CONTROL_LOCAL_OPTIONS_LEFT = 57,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.LocalAreaControls
	})]
	CONTROL_LOCAL_OPTIONS_RIGHT = 58,
	[KeyAction(GroupId = "shop", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	NAVIGATE_SHOP_MODE = 59,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	NEXT_SHIELD_TAB = 60,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	PREVIOUS_SHIELD_TAB = 61,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.LocalAreaControls
	})]
	NEXT_MERCENARY_OPTION = 62,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map,
		EKeyActionTag.ControllerExclusive,
		EKeyActionTag.LocalAreaControls
	})]
	PREVIOUS_MERCENARY_OPTION = 63,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	NEXT_SHOP_OPTION_TAB = 64,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	PREVIOUS_SHOP_OPTION_TAB = 65,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_STARTING_PERKS = 66,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_MERCENARY_INFO = 67,
	[KeyAction(GroupId = "menu", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.MainMenu
	})]
	UI_MENU_ALT1 = 70,
	[KeyAction(GroupId = "menu", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.MainMenu
	})]
	UI_MENU_DELETE = 71,
	[KeyAction(GroupId = "menu", Tags = new EKeyActionTag[] { EKeyActionTag.MainMenu })]
	MENU_ENTER = 72,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	CONFIRM_NOTIFICATION = 80,
	[KeyAction(GroupId = "map", Tags = new EKeyActionTag[]
	{
		EKeyActionTag.UI,
		EKeyActionTag.Map
	})]
	CONFIRM_PARTY_ACTION = 81,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	TOGGLE_VOICE_CHAT_CONTROL = 100,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.ControllerExclusive })]
	DEBUG_LEFT_STICK = 101,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.ControllerExclusive })]
	DEBUG_RIGHT_STICK = 102,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.ControllerExclusive })]
	DEBUG_BUTTON_EAST = 103,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.ControllerExclusive })]
	DEBUG_BUTTON_SOUTH = 104,
	[KeyAction(Tags = new EKeyActionTag[]
	{
		EKeyActionTag.Scenario,
		EKeyActionTag.Camera
	})]
	ROTATE_TARGET_BUTTON = 105,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_EDIT = 106,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	UI_ENTER = 107,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	PERSISTENT_SUBMIT = 200,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	NEXT_SHOP_OPTION_TAB_DPAD = 201,
	[KeyAction(Tags = new EKeyActionTag[] { EKeyActionTag.UI })]
	PREVIOUS_SHOP_OPTION_TAB_DPAD = 202
}
