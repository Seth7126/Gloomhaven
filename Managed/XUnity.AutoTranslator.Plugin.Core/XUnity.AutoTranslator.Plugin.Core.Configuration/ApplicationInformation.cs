using System;
using System.Runtime.InteropServices;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration;

internal static class ApplicationInformation
{
	private static HandleRef Null = new HandleRef(null, IntPtr.Zero);

	public static string StartupPath
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			GetModuleFileName(Null, stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
}
