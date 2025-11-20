using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data;

internal static class LocalDBAPI
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	private delegate int LocalDBFormatMessageDelegate(int hrLocalDB, uint dwFlags, uint dwLanguageId, StringBuilder buffer, ref uint buflen);

	private static LocalDBFormatMessageDelegate s_localDBFormatMessage = null;

	private static IntPtr s_userInstanceDLLHandle = IntPtr.Zero;

	private static readonly object s_dllLock = new object();

	private const uint const_LOCALDB_TRUNCATE_ERR_MESSAGE = 1u;

	private const int const_ErrorMessageBufferSize = 1024;

	private const string const_localDbPrefix = "(localdb)\\";

	private static LocalDBFormatMessageDelegate LocalDBFormatMessage
	{
		get
		{
			if (s_localDBFormatMessage == null)
			{
				lock (s_dllLock)
				{
					if (s_localDBFormatMessage == null)
					{
						IntPtr intPtr = LoadProcAddress();
						if (intPtr == IntPtr.Zero)
						{
							Marshal.GetLastWin32Error();
							throw CreateLocalDBException("Invalid SQLUserInstance.dll found at the location specified in the registry. Verify that the Local Database Runtime feature of SQL Server Express is properly installed.");
						}
						s_localDBFormatMessage = Marshal.GetDelegateForFunctionPointer<LocalDBFormatMessageDelegate>(intPtr);
					}
				}
			}
			return s_localDBFormatMessage;
		}
	}

	private static IntPtr UserInstanceDLLHandle
	{
		get
		{
			if (s_userInstanceDLLHandle == IntPtr.Zero)
			{
				lock (s_dllLock)
				{
					if (s_userInstanceDLLHandle == IntPtr.Zero)
					{
						SNINativeMethodWrapper.SNIQueryInfo(SNINativeMethodWrapper.QTypes.SNI_QUERY_LOCALDB_HMODULE, ref s_userInstanceDLLHandle);
						if (s_userInstanceDLLHandle == IntPtr.Zero)
						{
							SNINativeMethodWrapper.SNIGetLastError(out var pErrorStruct);
							throw CreateLocalDBException(global::SR.GetString("LocalDB_FailedGetDLLHandle"), null, 0, (int)pErrorStruct.sniError);
						}
					}
				}
			}
			return s_userInstanceDLLHandle;
		}
	}

	internal static void ReleaseDLLHandles()
	{
		s_userInstanceDLLHandle = IntPtr.Zero;
		s_localDBFormatMessage = null;
	}

	internal static string GetLocalDBMessage(int hrCode)
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			uint buflen = (uint)stringBuilder.Capacity;
			int num = LocalDBFormatMessage(hrCode, 1u, (uint)CultureInfo.CurrentCulture.LCID, stringBuilder, ref buflen);
			if (num >= 0)
			{
				return stringBuilder.ToString();
			}
			stringBuilder = new StringBuilder(1024);
			buflen = (uint)stringBuilder.Capacity;
			num = LocalDBFormatMessage(hrCode, 1u, 0u, stringBuilder, ref buflen);
			if (num >= 0)
			{
				return stringBuilder.ToString();
			}
			return string.Format(CultureInfo.CurrentCulture, "{0} (0x{1:X}).", "Cannot obtain Local Database Runtime error message", num);
		}
		catch (SqlException ex)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} ({1}).", "Cannot obtain Local Database Runtime error message", ex.Message);
		}
	}

	private static SqlException CreateLocalDBException(string errorMessage, string instance = null, int localDbError = 0, int sniError = 0)
	{
		SqlErrorCollection sqlErrorCollection = new SqlErrorCollection();
		int infoNumber = ((localDbError == 0) ? sniError : localDbError);
		if (sniError != 0)
		{
			string sNIErrorMessage = SQL.GetSNIErrorMessage(sniError);
			errorMessage = string.Format(null, "{0} (error: {1} - {2})", errorMessage, sniError, sNIErrorMessage);
		}
		sqlErrorCollection.Add(new SqlError(infoNumber, 0, 20, instance, errorMessage, null, 0));
		if (localDbError != 0)
		{
			sqlErrorCollection.Add(new SqlError(infoNumber, 0, 20, instance, GetLocalDBMessage(localDbError), null, 0));
		}
		SqlException ex = SqlException.CreateException(sqlErrorCollection, null);
		ex._doNotReconnect = true;
		return ex;
	}

	private static IntPtr LoadProcAddress()
	{
		return SafeNativeMethods.GetProcAddress(UserInstanceDLLHandle, "LocalDBFormatMessage");
	}

	internal static string GetLocalDbInstanceNameFromServerName(string serverName)
	{
		if (serverName == null)
		{
			return null;
		}
		serverName = serverName.TrimStart();
		if (!serverName.StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		string text = serverName.Substring("(localdb)\\".Length).Trim();
		if (text.Length == 0)
		{
			return null;
		}
		return text;
	}
}
