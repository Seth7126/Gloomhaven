using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InControl;

public class UnityKeyboardProvider : IKeyboardProvider
{
	public readonly struct KeyMapping
	{
		public readonly Key source;

		public readonly UnityEngine.InputSystem.Key target0;

		public readonly UnityEngine.InputSystem.Key target1;

		private readonly string name;

		private readonly string macName;

		public readonly KeyCode OldTarget0;

		public readonly KeyCode OldTarget1;

		public bool IsPressed
		{
			get
			{
				Keyboard current = Keyboard.current;
				if (current != null)
				{
					if (target0 != UnityEngine.InputSystem.Key.None && current[target0].isPressed)
					{
						return true;
					}
					if (target1 != UnityEngine.InputSystem.Key.None && current[target1].isPressed)
					{
						return true;
					}
				}
				return false;
			}
		}

		public string Name
		{
			get
			{
				if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
				{
					return macName;
				}
				return name;
			}
		}

		public KeyMapping(Dictionary<UnityEngine.InputSystem.Key, KeyCode> dict, Dictionary<KeyCode, UnityEngine.InputSystem.Key> secondDict, Key source, string name, KeyCode oldTarget0, UnityEngine.InputSystem.Key target0, KeyCode oldTarget1 = KeyCode.None, UnityEngine.InputSystem.Key target1 = UnityEngine.InputSystem.Key.None)
		{
			this.source = source;
			OldTarget0 = oldTarget0;
			OldTarget1 = oldTarget1;
			this.name = name;
			macName = name;
			this.target0 = target0;
			this.target1 = target1;
			dict[target0] = oldTarget0;
			dict[target1] = oldTarget1;
			secondDict[oldTarget0] = target0;
			secondDict[oldTarget1] = target1;
		}

		public KeyMapping(Dictionary<UnityEngine.InputSystem.Key, KeyCode> dict, Dictionary<KeyCode, UnityEngine.InputSystem.Key> secondDict, Key source, string name, string macName, KeyCode oldTarget0, UnityEngine.InputSystem.Key target0, KeyCode oldTarget1 = KeyCode.None, UnityEngine.InputSystem.Key target1 = UnityEngine.InputSystem.Key.None)
		{
			this.source = source;
			OldTarget0 = oldTarget0;
			OldTarget1 = oldTarget1;
			this.name = name;
			this.macName = macName;
			this.target0 = target0;
			this.target1 = UnityEngine.InputSystem.Key.None;
			dict[target0] = oldTarget0;
			dict[target1] = oldTarget1;
			secondDict[oldTarget0] = target0;
			secondDict[oldTarget1] = target1;
		}
	}

	public static readonly Dictionary<UnityEngine.InputSystem.Key, KeyCode> NewToOldCodes = new Dictionary<UnityEngine.InputSystem.Key, KeyCode>();

	public static readonly Dictionary<KeyCode, UnityEngine.InputSystem.Key> OldToNewCodes = new Dictionary<KeyCode, UnityEngine.InputSystem.Key>();

	public static readonly KeyMapping[] KeyMappings = new KeyMapping[132]
	{
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.None, "None", KeyCode.None, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Shift, "Shift", KeyCode.LeftShift, UnityEngine.InputSystem.Key.LeftShift, KeyCode.RightShift, UnityEngine.InputSystem.Key.RightShift),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Alt, "Alt", "Option", KeyCode.LeftAlt, UnityEngine.InputSystem.Key.LeftAlt, KeyCode.RightAlt, UnityEngine.InputSystem.Key.RightAlt),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Command, "Command", KeyCode.LeftMeta, UnityEngine.InputSystem.Key.LeftMeta, KeyCode.RightMeta, UnityEngine.InputSystem.Key.RightMeta),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Control, "Control", KeyCode.LeftControl, UnityEngine.InputSystem.Key.LeftCtrl, KeyCode.RightControl, UnityEngine.InputSystem.Key.RightCtrl),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftShift, "Left Shift", KeyCode.LeftShift, UnityEngine.InputSystem.Key.LeftShift),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftAlt, "Left Alt", "Left Option", KeyCode.LeftAlt, UnityEngine.InputSystem.Key.LeftAlt),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftCommand, "Left Command", KeyCode.LeftMeta, UnityEngine.InputSystem.Key.LeftMeta),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftControl, "Left Control", KeyCode.LeftControl, UnityEngine.InputSystem.Key.LeftCtrl),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightShift, "Right Shift", KeyCode.RightShift, UnityEngine.InputSystem.Key.RightShift),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightAlt, "Right Alt", "Right Option", KeyCode.RightAlt, UnityEngine.InputSystem.Key.RightAlt),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightCommand, "Right Command", KeyCode.RightMeta, UnityEngine.InputSystem.Key.RightMeta),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightControl, "Right Control", KeyCode.RightControl, UnityEngine.InputSystem.Key.RightCtrl),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Escape, "Escape", KeyCode.Escape, UnityEngine.InputSystem.Key.Escape),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F1, "F1", KeyCode.F1, UnityEngine.InputSystem.Key.F1),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F2, "F2", KeyCode.F2, UnityEngine.InputSystem.Key.F2),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F3, "F3", KeyCode.F3, UnityEngine.InputSystem.Key.F3),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F4, "F4", KeyCode.F4, UnityEngine.InputSystem.Key.F4),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F5, "F5", KeyCode.F5, UnityEngine.InputSystem.Key.F5),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F6, "F6", KeyCode.F6, UnityEngine.InputSystem.Key.F6),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F7, "F7", KeyCode.F7, UnityEngine.InputSystem.Key.F7),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F8, "F8", KeyCode.F8, UnityEngine.InputSystem.Key.F8),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F9, "F9", KeyCode.F9, UnityEngine.InputSystem.Key.F9),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F10, "F10", KeyCode.F10, UnityEngine.InputSystem.Key.F10),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F11, "F11", KeyCode.F11, UnityEngine.InputSystem.Key.F11),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F12, "F12", KeyCode.F12, UnityEngine.InputSystem.Key.F12),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key0, "0", KeyCode.Alpha0, UnityEngine.InputSystem.Key.Digit0),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key1, "1", KeyCode.Alpha1, UnityEngine.InputSystem.Key.Digit1),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key2, "2", KeyCode.Alpha2, UnityEngine.InputSystem.Key.Digit2),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key3, "3", KeyCode.Alpha3, UnityEngine.InputSystem.Key.Digit3),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key4, "4", KeyCode.Alpha4, UnityEngine.InputSystem.Key.Digit4),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key5, "5", KeyCode.Alpha5, UnityEngine.InputSystem.Key.Digit5),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key6, "6", KeyCode.Alpha6, UnityEngine.InputSystem.Key.Digit6),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key7, "7", KeyCode.Alpha7, UnityEngine.InputSystem.Key.Digit7),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key8, "8", KeyCode.Alpha8, UnityEngine.InputSystem.Key.Digit8),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Key9, "9", KeyCode.Alpha9, UnityEngine.InputSystem.Key.Digit9),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.A, "A", KeyCode.A, UnityEngine.InputSystem.Key.A),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.B, "B", KeyCode.B, UnityEngine.InputSystem.Key.B),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.C, "C", KeyCode.C, UnityEngine.InputSystem.Key.C),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.D, "D", KeyCode.D, UnityEngine.InputSystem.Key.D),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.E, "E", KeyCode.E, UnityEngine.InputSystem.Key.E),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F, "F", KeyCode.F, UnityEngine.InputSystem.Key.F),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.G, "G", KeyCode.G, UnityEngine.InputSystem.Key.G),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.H, "H", KeyCode.H, UnityEngine.InputSystem.Key.H),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.I, "I", KeyCode.I, UnityEngine.InputSystem.Key.I),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.J, "J", KeyCode.J, UnityEngine.InputSystem.Key.J),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.K, "K", KeyCode.K, UnityEngine.InputSystem.Key.K),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.L, "L", KeyCode.L, UnityEngine.InputSystem.Key.L),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.M, "M", KeyCode.M, UnityEngine.InputSystem.Key.M),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.N, "N", KeyCode.N, UnityEngine.InputSystem.Key.N),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.O, "O", KeyCode.O, UnityEngine.InputSystem.Key.O),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.P, "P", KeyCode.P, UnityEngine.InputSystem.Key.P),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Q, "Q", KeyCode.Q, UnityEngine.InputSystem.Key.Q),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.R, "R", KeyCode.R, UnityEngine.InputSystem.Key.R),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.S, "S", KeyCode.S, UnityEngine.InputSystem.Key.S),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.T, "T", KeyCode.T, UnityEngine.InputSystem.Key.T),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.U, "U", KeyCode.U, UnityEngine.InputSystem.Key.U),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.V, "V", KeyCode.V, UnityEngine.InputSystem.Key.V),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.W, "W", KeyCode.W, UnityEngine.InputSystem.Key.W),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.X, "X", KeyCode.X, UnityEngine.InputSystem.Key.X),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Y, "Y", KeyCode.Y, UnityEngine.InputSystem.Key.Y),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Z, "Z", KeyCode.Z, UnityEngine.InputSystem.Key.Z),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Backquote, "Backquote", KeyCode.BackQuote, UnityEngine.InputSystem.Key.Backquote),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Minus, "Minus", KeyCode.Minus, UnityEngine.InputSystem.Key.Minus),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Equals, "Equals", KeyCode.Equals, UnityEngine.InputSystem.Key.Equals),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Backspace, "Backspace", KeyCode.Backspace, UnityEngine.InputSystem.Key.Backspace),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Tab, "Tab", KeyCode.Tab, UnityEngine.InputSystem.Key.Tab),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftBracket, "Left Bracket", KeyCode.LeftBracket, UnityEngine.InputSystem.Key.LeftBracket),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightBracket, "Right Bracket", KeyCode.RightBracket, UnityEngine.InputSystem.Key.RightBracket),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Backslash, "Backslash", KeyCode.Backslash, UnityEngine.InputSystem.Key.Backslash),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Semicolon, "Semicolon", KeyCode.Semicolon, UnityEngine.InputSystem.Key.Semicolon),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Quote, "Quote", KeyCode.Quote, UnityEngine.InputSystem.Key.Quote),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Return, "Return", KeyCode.Return, UnityEngine.InputSystem.Key.Enter),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Comma, "Comma", KeyCode.Comma, UnityEngine.InputSystem.Key.Comma),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Period, "Period", KeyCode.Period, UnityEngine.InputSystem.Key.Period),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Slash, "Slash", KeyCode.Slash, UnityEngine.InputSystem.Key.Slash),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Space, "Space", KeyCode.Space, UnityEngine.InputSystem.Key.Space),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Insert, "Insert", KeyCode.Insert, UnityEngine.InputSystem.Key.Insert),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Delete, "Delete", KeyCode.Delete, UnityEngine.InputSystem.Key.Delete),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Home, "Home", KeyCode.Home, UnityEngine.InputSystem.Key.Home),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.End, "End", KeyCode.End, UnityEngine.InputSystem.Key.End),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PageUp, "PageUp", KeyCode.PageUp, UnityEngine.InputSystem.Key.PageUp),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PageDown, "PageDown", KeyCode.PageDown, UnityEngine.InputSystem.Key.PageDown),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftArrow, "Left Arrow", KeyCode.LeftArrow, UnityEngine.InputSystem.Key.LeftArrow),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightArrow, "Right Arrow", KeyCode.RightArrow, UnityEngine.InputSystem.Key.RightArrow),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.UpArrow, "Up Arrow", KeyCode.UpArrow, UnityEngine.InputSystem.Key.UpArrow),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.DownArrow, "Down Arrow", KeyCode.DownArrow, UnityEngine.InputSystem.Key.DownArrow),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad0, "Numpad 0", KeyCode.Keypad0, UnityEngine.InputSystem.Key.Numpad0),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad1, "Numpad 1", KeyCode.Keypad1, UnityEngine.InputSystem.Key.Numpad1),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad2, "Numpad 2", KeyCode.Keypad2, UnityEngine.InputSystem.Key.Numpad2),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad3, "Numpad 3", KeyCode.Keypad3, UnityEngine.InputSystem.Key.Numpad3),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad4, "Numpad 4", KeyCode.Keypad4, UnityEngine.InputSystem.Key.Numpad4),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad5, "Numpad 5", KeyCode.Keypad5, UnityEngine.InputSystem.Key.Numpad5),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad6, "Numpad 6", KeyCode.Keypad6, UnityEngine.InputSystem.Key.Numpad6),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad7, "Numpad 7", KeyCode.Keypad7, UnityEngine.InputSystem.Key.Numpad7),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad8, "Numpad 8", KeyCode.Keypad8, UnityEngine.InputSystem.Key.Numpad8),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pad9, "Numpad 9", KeyCode.Keypad9, UnityEngine.InputSystem.Key.Numpad9),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Numlock, "Numlock", KeyCode.Numlock, UnityEngine.InputSystem.Key.NumLock),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PadDivide, "Numpad Divide", KeyCode.KeypadDivide, UnityEngine.InputSystem.Key.NumpadDivide),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PadMultiply, "Numpad Multiply", KeyCode.KeypadMultiply, UnityEngine.InputSystem.Key.NumpadMultiply),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PadMinus, "Numpad Minus", KeyCode.KeypadMinus, UnityEngine.InputSystem.Key.NumpadMinus),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PadPlus, "Numpad Plus", KeyCode.KeypadPlus, UnityEngine.InputSystem.Key.NumpadPlus),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PadEnter, "Numpad Enter", KeyCode.KeypadEnter, UnityEngine.InputSystem.Key.NumpadEnter),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PadPeriod, "Numpad Period", KeyCode.KeypadPeriod, UnityEngine.InputSystem.Key.NumpadPeriod),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Clear, "Clear", KeyCode.Clear, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.PadEquals, "Numpad Equals", KeyCode.KeypadEquals, UnityEngine.InputSystem.Key.NumpadEquals),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F13, "F13", KeyCode.F13, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F14, "F14", KeyCode.F14, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.F15, "F15", KeyCode.F15, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.AltGr, "AltGr", KeyCode.AltGr, UnityEngine.InputSystem.Key.RightAlt),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.CapsLock, "Caps Lock", KeyCode.CapsLock, UnityEngine.InputSystem.Key.CapsLock),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.ExclamationMark, "Exclamation Mark", KeyCode.Exclaim, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Tilde, "Tilde", KeyCode.Tilde, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.At, "At", KeyCode.At, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Hash, "Hash", KeyCode.Hash, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Dollar, "Dollar", KeyCode.Dollar, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Percent, "Percent", KeyCode.Percent, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Caret, "Caret", KeyCode.Caret, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Ampersand, "Ampersand", KeyCode.Ampersand, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Asterisk, "Asterisk", KeyCode.Asterisk, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftParen, "Left Paren", KeyCode.LeftParen, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightParen, "Right Paren", KeyCode.RightParen, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Underscore, "Underscore", KeyCode.Underscore, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Plus, "Plus", KeyCode.Plus, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LeftBrace, "Left Brace", KeyCode.LeftBracket, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.RightBrace, "Right Brace", KeyCode.RightBracket, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Pipe, "Pipe", KeyCode.Pipe, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.Colon, "Colon", KeyCode.Colon, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.DoubleQuote, "Double Quote", KeyCode.DoubleQuote, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.LessThan, "Less Than", KeyCode.Less, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.GreaterThan, "Greater Than", KeyCode.Greater, UnityEngine.InputSystem.Key.None),
		new KeyMapping(NewToOldCodes, OldToNewCodes, Key.QuestionMark, "Question Mark", KeyCode.Question, UnityEngine.InputSystem.Key.None)
	};

	public void Setup()
	{
	}

	public void Reset()
	{
	}

	public void Update()
	{
	}

	public bool AnyKeyIsPressed()
	{
		return Keyboard.current?.anyKey.isPressed ?? false;
	}

	public bool GetKeyIsPressed(Key control)
	{
		return KeyMappings[(int)control].IsPressed;
	}

	public string GetNameForKey(Key control)
	{
		return KeyMappings[(int)control].Name;
	}

	public static Key GetKeyForKeyCode(KeyCode keyCode)
	{
		return KeyMappings.FirstOrDefault((KeyMapping m) => m.OldTarget0 == keyCode && m.target1 == UnityEngine.InputSystem.Key.None).source;
	}

	public static KeyCode GetKeyCodeForMouse(Mouse mouse)
	{
		return mouse switch
		{
			Mouse.LeftButton => KeyCode.Mouse0, 
			Mouse.RightButton => KeyCode.Mouse1, 
			Mouse.MiddleButton => KeyCode.Mouse2, 
			Mouse.Button4 => KeyCode.Mouse3, 
			Mouse.Button5 => KeyCode.Mouse4, 
			Mouse.Button6 => KeyCode.Mouse5, 
			Mouse.Button7 => KeyCode.Mouse6, 
			_ => KeyCode.None, 
		};
	}

	public static Mouse GeMouseForKeyCode(KeyCode keyCode)
	{
		return keyCode switch
		{
			KeyCode.Mouse0 => Mouse.LeftButton, 
			KeyCode.Mouse1 => Mouse.RightButton, 
			KeyCode.Mouse2 => Mouse.MiddleButton, 
			KeyCode.Mouse3 => Mouse.Button4, 
			KeyCode.Mouse4 => Mouse.Button5, 
			KeyCode.Mouse5 => Mouse.Button6, 
			KeyCode.Mouse6 => Mouse.Button7, 
			_ => Mouse.None, 
		};
	}
}
