using System;
using System.Runtime.InteropServices;

namespace Sony.PS4.Dialog;

public class Ime
{
	public enum EnterLabel
	{
		DEFAULT,
		SEND,
		LABEL_SEARCH,
		LABEL_GO
	}

	public enum InputMethod
	{
		DEFAULT
	}

	public enum Type
	{
		DEFAULT,
		BASIC_LATIN,
		URL,
		MAIL,
		NUMBER
	}

	public enum HorizontalAlignment
	{
		LEFT,
		CENTER,
		RIGHT
	}

	public enum VerticalAlignment
	{
		TOP,
		ENTER,
		BOTTOM
	}

	public enum PanelPriority
	{
		DEFAULT,
		ALPHABET,
		SYMBOL,
		ACCENT
	}

	[Flags]
	public enum FlagsSupportedLanguages : long
	{
		LANGUAGE_DANISH = 1L,
		LANGUAGE_GERMAN = 2L,
		LANGUAGE_ENGLISH_US = 4L,
		LANGUAGE_SPANISH = 8L,
		LANGUAGE_FRENCH = 0x10L,
		LANGUAGE_ITALIAN = 0x20L,
		LANGUAGE_DUTCH = 0x40L,
		LANGUAGE_NORWEGIAN = 0x80L,
		LANGUAGE_POLISH = 0x100L,
		LANGUAGE_PORTUGUESE_PT = 0x200L,
		LANGUAGE_RUSSIAN = 0x400L,
		LANGUAGE_FINNISH = 0x800L,
		LANGUAGE_SWEDISH = 0x1000L,
		LANGUAGE_JAPANESE = 0x2000L,
		LANGUAGE_KOREAN = 0x4000L,
		LANGUAGE_SIMPLIFIED_CHINESE = 0x8000L,
		LANGUAGE_TRADITIONAL_CHINESE = 0x10000L,
		LANGUAGE_PORTUGUESE_BR = 0x20000L,
		LANGUAGE_ENGLISH_GB = 0x40000L,
		LANGUAGE_TURKISH = 0x80000L,
		LANGUAGE_SPANISH_LA = 0x100000L
	}

	[Flags]
	public enum Option
	{
		DEFAULT = 0,
		MULTILINE = 1,
		NO_AUTO_CAPITALIZATION = 2,
		PASSWORD = 4,
		LANGUAGES_FORCED = 8,
		EXT_KEYBOARD = 0x10,
		NO_LEARNING = 0x20,
		FIXED_POSITION = 0x40,
		DISABLE_COPY_PASTE = 0x80,
		DISABLE_RESUME = 0x100,
		DISABLE_AUTO_SPACE = 0x200
	}

	[Flags]
	public enum ExtOption
	{
		DEFAULT = 0,
		SET_COLOR = 1,
		SET_PRIORITY = 2,
		PRIORITY_SHIFT = 4,
		PRIORITY_FULL_WIDTH = 8,
		PRIORITY_FIXED_PANEL = 0x10,
		DISABLE_POINTER = 0x40,
		ENABLE_ADDITIONAL_DICTIONARY = 0x80,
		DISABLE_STARTUP_SE = 0x100,
		DISABLE_LIST_FOR_EXT_KEYBOARD = 0x200,
		HIDE_KEYPANEL_IF_EXT_KEYBOARD = 0x400,
		INIT_EXT_KEYBOARD_MODE = 0x800
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class SceImeDialogParam
	{
		public uint userId;

		public Type type;

		public FlagsSupportedLanguages supportedLanguages;

		public EnterLabel enterLabel;

		public InputMethod inputMethod;

		public IntPtr _filter;

		public Option option;

		public uint maxTextLength;

		private IntPtr _inputTextBuffer;

		public float posx;

		public float posy;

		public HorizontalAlignment horizontalAlignment;

		public VerticalAlignment verticalAlignment;

		private IntPtr _placeholder;

		private IntPtr _title;

		private byte r0;

		private byte r1;

		private byte r2;

		private byte r3;

		private byte r4;

		private byte r5;

		private byte r6;

		private byte r7;

		private byte r8;

		private byte r9;

		private byte r10;

		private byte r11;

		private byte r12;

		private byte r13;

		private byte r14;

		private byte r15;

		public string title
		{
			get
			{
				return Marshal.PtrToStringUni(_title);
			}
			set
			{
				_title = Marshal.StringToCoTaskMemUni(value);
			}
		}

		public string inputTextBuffer
		{
			get
			{
				return Marshal.PtrToStringUni(_inputTextBuffer);
			}
			set
			{
				_inputTextBuffer = Marshal.StringToCoTaskMemUni(value);
			}
		}

		public string placeholder
		{
			get
			{
				return Marshal.PtrToStringUni(_placeholder);
			}
			set
			{
				_placeholder = Marshal.StringToCoTaskMemUni(value);
			}
		}

		public SceImeDialogParam()
		{
			userId = 0u;
			type = Type.DEFAULT;
			supportedLanguages = (FlagsSupportedLanguages)0L;
			enterLabel = EnterLabel.DEFAULT;
			inputMethod = InputMethod.DEFAULT;
			_filter = IntPtr.Zero;
			option = Option.DEFAULT;
			maxTextLength = 0u;
			_inputTextBuffer = IntPtr.Zero;
			posx = 0f;
			posy = 0f;
			horizontalAlignment = HorizontalAlignment.LEFT;
			verticalAlignment = VerticalAlignment.TOP;
			_placeholder = IntPtr.Zero;
			_title = IntPtr.Zero;
			r0 = (r1 = (r2 = (r3 = (r4 = (r5 = (r6 = (r7 = (r8 = (r9 = (r10 = (r11 = (r12 = (r13 = (r14 = (r15 = 0)))))))))))))));
		}

		~SceImeDialogParam()
		{
			Marshal.FreeCoTaskMem(_title);
			Marshal.FreeCoTaskMem(_inputTextBuffer);
			Marshal.FreeCoTaskMem(_placeholder);
		}
	}

	public struct SceImeColor
	{
		public byte r;

		public byte g;

		public byte b;

		public byte a;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class SceImeParamExtended
	{
		public ExtOption option;

		public SceImeColor colorBase;

		public SceImeColor colorLine;

		public SceImeColor colorTextField;

		public SceImeColor colorPreedit;

		public SceImeColor colorButtonDefault;

		public SceImeColor colorButtonFunction;

		public SceImeColor colorButtonSymbol;

		public SceImeColor colorText;

		public SceImeColor colorSpecial;

		public PanelPriority priority;

		private uint padding;

		private IntPtr _additionalDictionaryPath;

		public IntPtr _extKeyboardFilter;

		public uint disableDevice;

		public uint extKeyboardMode;

		public string additionalDictionaryPath
		{
			get
			{
				return Marshal.PtrToStringUni(_additionalDictionaryPath);
			}
			set
			{
				_additionalDictionaryPath = Marshal.StringToCoTaskMemUni(value);
			}
		}

		public SceImeParamExtended()
		{
			option = ExtOption.DEFAULT;
		}

		~SceImeParamExtended()
		{
			Marshal.FreeCoTaskMem(_additionalDictionaryPath);
		}
	}

	public enum EnumImeDialogResult
	{
		RESULT_OK,
		RESULT_USER_CANCELED,
		RESULT_ABORTED
	}

	public enum EnumImeDialogResultButton
	{
		BUTTON_NONE,
		BUTTON_CLOSE,
		BUTTON_ENTER
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ImeDialogResult
	{
		public EnumImeDialogResult result;

		public EnumImeDialogResultButton button;

		private IntPtr _text;

		public string text => Marshal.PtrToStringAnsi(_text);
	}

	public static bool IsDialogOpen => PrxImeDialogIsDialogOpen();

	public static event Messages.EventHandler OnGotIMEDialogResult;

	[DllImport("CommonDialog")]
	private static extern int PrxImeDialogInitialise();

	[DllImport("CommonDialog")]
	private static extern void PrxImeDialogUpdate();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxImeDialogIsDialogOpen();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxImeDialogOpen(SceImeDialogParam parameters, SceImeParamExtended extended);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxImeDialogGetResult(out ImeDialogResult result);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogHasMessage();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogGetFirstMessage(out Messages.PluginMessage msg);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogRemoveFirstMessage();

	public static bool Open(SceImeDialogParam Imeparams, SceImeParamExtended extended)
	{
		return PrxImeDialogOpen(Imeparams, extended);
	}

	public static ImeDialogResult GetResult()
	{
		ImeDialogResult result = default(ImeDialogResult);
		PrxImeDialogGetResult(out result);
		return result;
	}

	public static void ProcessMessage(Messages.PluginMessage msg)
	{
		Messages.MessageType type = msg.type;
		if (type == Messages.MessageType.kDialog_GotIMEDialogResult && Ime.OnGotIMEDialogResult != null)
		{
			Ime.OnGotIMEDialogResult(msg);
		}
	}

	public static void Initialise()
	{
		PrxImeDialogInitialise();
	}

	public static void Update()
	{
		PrxImeDialogUpdate();
	}
}
