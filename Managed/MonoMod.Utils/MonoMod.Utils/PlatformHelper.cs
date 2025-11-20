using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MonoMod.Utils;

public static class PlatformHelper
{
	public static Platform Current { get; private set; }

	public static string LibrarySuffix { get; private set; }

	static PlatformHelper()
	{
		Current = Platform.Unknown;
		PropertyInfo property = typeof(Environment).GetProperty("Platform", BindingFlags.Static | BindingFlags.NonPublic);
		string text = ((property == null) ? Environment.OSVersion.Platform.ToString() : property.GetValue(null, new object[0]).ToString());
		text = text.ToLowerInvariant();
		if (text.Contains("win"))
		{
			Current = Platform.Windows;
		}
		else if (text.Contains("mac") || text.Contains("osx"))
		{
			Current = Platform.MacOS;
		}
		else if (text.Contains("lin") || text.Contains("unix"))
		{
			Current = Platform.Linux;
		}
		if (Directory.Exists("/data") && File.Exists("/system/build.prop"))
		{
			Current = Platform.Android;
		}
		else if (Directory.Exists("/Applications") && Directory.Exists("/System"))
		{
			Current = Platform.iOS;
		}
		MethodInfo methodInfo = typeof(Environment).GetProperty("Is64BitOperatingSystem")?.GetGetMethod();
		if (methodInfo != null)
		{
			Current |= (Platform)(((bool)methodInfo.Invoke(null, new object[0])) ? 2 : 0);
		}
		else
		{
			Current |= (Platform)((IntPtr.Size >= 8) ? 2 : 0);
		}
		if (Is(Platform.Unix) && Type.GetType("Mono.Runtime") != null)
		{
			try
			{
				string text2;
				using (Process process = Process.Start(new ProcessStartInfo("uname", "-m")
				{
					UseShellExecute = false,
					RedirectStandardOutput = true
				}))
				{
					text2 = process.StandardOutput.ReadLine().Trim();
				}
				if (text2.StartsWith("aarch") || text2.StartsWith("arm"))
				{
					Current |= Platform.ARM;
				}
			}
			catch (Exception)
			{
			}
		}
		else
		{
			typeof(object).Module.GetPEKind(out var _, out var machine);
			if (machine == (ImageFileMachine)452)
			{
				Current |= Platform.ARM;
			}
		}
		LibrarySuffix = (Is(Platform.MacOS) ? "dylib" : (Is(Platform.Unix) ? "so" : "dll"));
	}

	public static bool Is(Platform platform)
	{
		return (Current & platform) == platform;
	}
}
