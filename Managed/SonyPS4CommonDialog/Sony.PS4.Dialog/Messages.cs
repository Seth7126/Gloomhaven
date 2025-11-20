using System;
using System.Runtime.InteropServices;

namespace Sony.PS4.Dialog;

public class Messages
{
	public enum MessageType
	{
		kDialog_NotSet,
		kDialog_Log,
		kDialog_LogWarning,
		kDialog_LogError,
		kDialog_GotDialogResult,
		kDialog_GotIMEDialogResult,
		kDialog_GotSigninDialogResult,
		kDialog_GotWebBrowserDialogResult
	}

	public delegate void EventHandler(PluginMessage msg);

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PluginMessage
	{
		public MessageType type;

		public int dataSize;

		public IntPtr data;

		public string Text
		{
			get
			{
				switch (type)
				{
				case MessageType.kDialog_Log:
				case MessageType.kDialog_LogWarning:
				case MessageType.kDialog_LogError:
					return Marshal.PtrToStringAnsi(data);
				default:
					return "no text";
				}
			}
		}

		public int Int => type switch
		{
			MessageType.kDialog_GotDialogResult => (int)data, 
			MessageType.kDialog_GotIMEDialogResult => (int)data, 
			MessageType.kDialog_GotSigninDialogResult => (int)data, 
			MessageType.kDialog_GotWebBrowserDialogResult => (int)data, 
			_ => 0, 
		};
	}

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogHasMessage();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogGetFirstMessage(out PluginMessage msg);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogRemoveFirstMessage();

	public static bool HasMessage()
	{
		return PrxCommonDialogHasMessage();
	}

	public static void RemoveFirstMessage()
	{
		PrxCommonDialogRemoveFirstMessage();
	}

	public static void GetFirstMessage(out PluginMessage msg)
	{
		PrxCommonDialogGetFirstMessage(out msg);
	}
}
