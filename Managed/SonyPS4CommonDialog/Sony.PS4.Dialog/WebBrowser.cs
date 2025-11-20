using System;
using System.Runtime.InteropServices;

namespace Sony.PS4.Dialog;

public class WebBrowser
{
	public enum EnumWebBrowserDialogResult
	{
		RESULT_OK,
		RESULT_USER_CANCELED
	}

	public enum DialogMode
	{
		DEFAULT = 1,
		CUSTOM
	}

	[Flags]
	public enum CustomParts : uint
	{
		NONE = 0u,
		TITLE = 1u,
		ADDRESS = 2u,
		FOOTER = 4u,
		BACKGROUND = 8u,
		WAIT_DIALOG = 0x10u
	}

	[Flags]
	public enum CustomControl : uint
	{
		NONE = 0u,
		EXIT = 1u,
		RELOAD = 2u,
		BACK = 4u,
		FORWARD = 8u,
		ZOOM = 0x10u,
		EXIT_UNTIL_COMPLETE = 0x20u,
		OPTION_MENU = 0x40u
	}

	public enum Animation : uint
	{
		DEFAULT,
		DISABLE
	}

	[Flags]
	public enum ImeOption : uint
	{
		DEFAULT = 0u,
		NO_AUTO_CAPITALIZATION = 2u,
		NO_LEARNING = 0x20u,
		DISABLE_COPY_PASTE = 0x80u,
		DISABLE_AUTO_SPACE = 0x200u
	}

	[Flags]
	public enum WebViewOption : uint
	{
		NONE = 0u,
		BACKGROUND_TRANSPARENCY = 1u,
		CURSOR_NONE = 2u,
		OSK_NO_SUBMIT = 4u
	}

	[StructLayout(LayoutKind.Sequential)]
	public class WebBrowserParam
	{
		public DialogMode mode;

		public int userId;

		public string url;

		public ushort width;

		public ushort height;

		public ushort positionX;

		public ushort positionY;

		public CustomParts parts;

		public ushort headerWidth;

		public ushort headerPositionX;

		public ushort headerPositionY;

		public CustomControl control;

		public ImeOption imeOption;

		public WebViewOption webViewOption;

		public Animation animation;

		public WebBrowserParam()
		{
			mode = DialogMode.DEFAULT;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class WebBrowserPredeterminedContentParam
	{
		public string[] domain;

		public WebBrowserPredeterminedContentParam()
		{
			domain = new string[5];
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct WebBrowserDialogResult
	{
		public EnumWebBrowserDialogResult result;
	}

	public static bool IsDialogOpen => PrxWebBrowserDialogIsDialogOpen();

	public static event Messages.EventHandler OnGotWebBrowserDialogResult;

	[DllImport("CommonDialog")]
	private static extern int PrxWebBrowserDialogTerminate();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxWebBrowserDialogIsDialogOpen();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxWebBrowserDialogOpen(WebBrowserParam webBrowserParam);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxWebBrowserDialogOpenForPredeterminedContent(WebBrowserParam webBrowserParam, string domain0, string domain1, string domain2, string domain3, string domain4);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern int PrxWebBrowserDialogResetCookie();

	[DllImport("CommonDialog")]
	private static extern int PrxWebBrowserDialogSetCookie(string url, string cookie);

	[DllImport("CommonDialog")]
	private static extern bool PrxWebBrowserDialogGetResult(out WebBrowserDialogResult result);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogHasMessage();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogGetFirstMessage(out Messages.PluginMessage msg);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogRemoveFirstMessage();

	public static bool Open(WebBrowserParam webBrowserParam)
	{
		return PrxWebBrowserDialogOpen(webBrowserParam);
	}

	public static bool OpenForPredeterminedContent(WebBrowserParam webBrowserParam, WebBrowserPredeterminedContentParam param2)
	{
		return PrxWebBrowserDialogOpenForPredeterminedContent(webBrowserParam, param2.domain[0], param2.domain[1], param2.domain[2], param2.domain[3], param2.domain[4]);
	}

	public static int ResetCookie()
	{
		return PrxWebBrowserDialogResetCookie();
	}

	public static int SetCookie(string url, string cookie)
	{
		return PrxWebBrowserDialogSetCookie(url, cookie);
	}

	public static WebBrowserDialogResult GetResult()
	{
		WebBrowserDialogResult result = default(WebBrowserDialogResult);
		PrxWebBrowserDialogGetResult(out result);
		return result;
	}

	public static void ProcessMessage(Messages.PluginMessage msg)
	{
		Messages.MessageType type = msg.type;
		if (type == Messages.MessageType.kDialog_GotWebBrowserDialogResult && WebBrowser.OnGotWebBrowserDialogResult != null)
		{
			WebBrowser.OnGotWebBrowserDialogResult(msg);
		}
	}

	public static void Terminate()
	{
		PrxWebBrowserDialogTerminate();
	}
}
