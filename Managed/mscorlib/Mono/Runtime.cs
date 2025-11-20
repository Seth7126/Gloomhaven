using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono;

internal static class Runtime
{
	private enum CrashReportLogLevel
	{
		MonoSummaryNone,
		MonoSummarySetup,
		MonoSummarySuspendHandshake,
		MonoSummaryUnmanagedStacks,
		MonoSummaryManagedStacks,
		MonoSummaryStateWriter,
		MonoSummaryStateWriterDone,
		MonoSummaryMerpWriter,
		MonoSummaryMerpInvoke,
		MonoSummaryCleanup,
		MonoSummaryDone,
		MonoSummaryDoubleFault
	}

	private static object dump = new object();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void mono_runtime_install_handlers();

	internal static void InstallSignalHandlers()
	{
		mono_runtime_install_handlers();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string GetDisplayName();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string GetNativeStackTrace(Exception exception);

	public static bool SetGCAllowSynchronousMajor(bool flag)
	{
		return true;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string ExceptionToState_internal(Exception exc, out ulong portable_hash, out ulong unportable_hash);

	private static Tuple<string, ulong, ulong> ExceptionToState(Exception exc)
	{
		ulong portable_hash;
		ulong unportable_hash;
		return new Tuple<string, ulong, ulong>(ExceptionToState_internal(exc, out portable_hash, out unportable_hash), portable_hash, unportable_hash);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void DisableMicrosoftTelemetry();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void EnableMicrosoftTelemetry_internal(IntPtr appBundleID, IntPtr appSignature, IntPtr appVersion, IntPtr merpGUIPath, IntPtr appPath, IntPtr configDir);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SendMicrosoftTelemetry_internal(IntPtr payload, ulong portable_hash, ulong unportable_hash);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void WriteStateToFile_internal(IntPtr payload, ulong portable_hash, ulong unportable_hash);

	private static void WriteStateToFile(Exception exc)
	{
		ulong portable_hash;
		ulong unportable_hash;
		using SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(ExceptionToState_internal(exc, out portable_hash, out unportable_hash));
		WriteStateToFile_internal(safeStringMarshal.Value, portable_hash, unportable_hash);
	}

	private static void SendMicrosoftTelemetry(string payload_str, ulong portable_hash, ulong unportable_hash)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			using (SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(payload_str))
			{
				SendMicrosoftTelemetry_internal(safeStringMarshal.Value, portable_hash, unportable_hash);
				return;
			}
		}
		throw new PlatformNotSupportedException("Merp support is currently only supported on OSX.");
	}

	private static void SendExceptionToTelemetry(Exception exc)
	{
		lock (dump)
		{
			SendMicrosoftTelemetry(ExceptionToState_internal(exc, out var portable_hash, out var unportable_hash), portable_hash, unportable_hash);
		}
	}

	private static void EnableMicrosoftTelemetry(string appBundleID_str, string appSignature_str, string appVersion_str, string merpGUIPath_str, string unused, string appPath_str, string configDir_str)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			using (SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(appBundleID_str))
			{
				using SafeStringMarshal safeStringMarshal2 = RuntimeMarshal.MarshalString(appSignature_str);
				using SafeStringMarshal safeStringMarshal3 = RuntimeMarshal.MarshalString(appVersion_str);
				using SafeStringMarshal safeStringMarshal4 = RuntimeMarshal.MarshalString(merpGUIPath_str);
				using SafeStringMarshal safeStringMarshal5 = RuntimeMarshal.MarshalString(appPath_str);
				using SafeStringMarshal safeStringMarshal6 = RuntimeMarshal.MarshalString(configDir_str);
				EnableMicrosoftTelemetry_internal(safeStringMarshal.Value, safeStringMarshal2.Value, safeStringMarshal3.Value, safeStringMarshal4.Value, safeStringMarshal5.Value, safeStringMarshal6.Value);
				return;
			}
		}
		throw new PlatformNotSupportedException("Merp support is currently only supported on OSX.");
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string DumpStateSingle_internal(out ulong portable_hash, out ulong unportable_hash);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string DumpStateTotal_internal(out ulong portable_hash, out ulong unportable_hash);

	private static Tuple<string, ulong, ulong> DumpStateSingle()
	{
		string item;
		ulong portable_hash;
		ulong unportable_hash;
		lock (dump)
		{
			item = DumpStateSingle_internal(out portable_hash, out unportable_hash);
		}
		return new Tuple<string, ulong, ulong>(item, portable_hash, unportable_hash);
	}

	private static Tuple<string, ulong, ulong> DumpStateTotal()
	{
		string item;
		ulong portable_hash;
		ulong unportable_hash;
		lock (dump)
		{
			item = DumpStateTotal_internal(out portable_hash, out unportable_hash);
		}
		return new Tuple<string, ulong, ulong>(item, portable_hash, unportable_hash);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RegisterReportingForAllNativeLibs_internal();

	private static void RegisterReportingForAllNativeLibs()
	{
		RegisterReportingForAllNativeLibs_internal();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RegisterReportingForNativeLib_internal(IntPtr modulePathSuffix, IntPtr moduleName);

	private static void RegisterReportingForNativeLib(string modulePathSuffix_str, string moduleName_str)
	{
		using SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(modulePathSuffix_str);
		using SafeStringMarshal safeStringMarshal2 = RuntimeMarshal.MarshalString(moduleName_str);
		RegisterReportingForNativeLib_internal(safeStringMarshal.Value, safeStringMarshal2.Value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void EnableCrashReportLog_internal(IntPtr directory);

	private static void EnableCrashReportLog(string directory_str)
	{
		using SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(directory_str);
		EnableCrashReportLog_internal(safeStringMarshal.Value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int CheckCrashReportLog_internal(IntPtr directory, bool clear);

	private static CrashReportLogLevel CheckCrashReportLog(string directory_str, bool clear)
	{
		using SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(directory_str);
		return (CrashReportLogLevel)CheckCrashReportLog_internal(safeStringMarshal.Value, clear);
	}

	private static string get_breadcrumb_value(string file_prefix, string directory_str, bool clear)
	{
		string[] files = Directory.GetFiles(directory_str, file_prefix + "_*");
		if (files.Length == 0)
		{
			return string.Empty;
		}
		if (files.Length > 1)
		{
			try
			{
				Array.ForEach(files, delegate(string f)
				{
					File.Delete(f);
				});
			}
			catch (Exception)
			{
			}
			return string.Empty;
		}
		if (clear)
		{
			File.Delete(files[0]);
		}
		return Path.GetFileName(files[0]).Substring(file_prefix.Length + 1);
	}

	private static long CheckCrashReportHash(string directory_str, bool clear)
	{
		string text = get_breadcrumb_value("crash_hash", directory_str, clear);
		if (text == string.Empty)
		{
			return 0L;
		}
		return Convert.ToInt64(text, 16);
	}

	private static string CheckCrashReportReason(string directory_str, bool clear)
	{
		return get_breadcrumb_value("crash_reason", directory_str, clear);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void AnnotateMicrosoftTelemetry_internal(IntPtr key, IntPtr val);

	private static void AnnotateMicrosoftTelemetry(string key, string val)
	{
		using SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(key);
		using SafeStringMarshal safeStringMarshal2 = RuntimeMarshal.MarshalString(val);
		lock (dump)
		{
			AnnotateMicrosoftTelemetry_internal(safeStringMarshal.Value, safeStringMarshal2.Value);
		}
	}
}
