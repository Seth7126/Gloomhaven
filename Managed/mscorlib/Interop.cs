using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

internal static class Interop
{
	internal static class Kernel32
	{
		internal struct REG_TZI_FORMAT
		{
			internal int Bias;

			internal int StandardBias;

			internal int DaylightBias;

			internal SYSTEMTIME StandardDate;

			internal SYSTEMTIME DaylightDate;

			internal REG_TZI_FORMAT(in TIME_ZONE_INFORMATION tzi)
			{
				Bias = tzi.Bias;
				StandardDate = tzi.StandardDate;
				StandardBias = tzi.StandardBias;
				DaylightDate = tzi.DaylightDate;
				DaylightBias = tzi.DaylightBias;
			}
		}

		internal struct SYSTEMTIME
		{
			internal ushort Year;

			internal ushort Month;

			internal ushort DayOfWeek;

			internal ushort Day;

			internal ushort Hour;

			internal ushort Minute;

			internal ushort Second;

			internal ushort Milliseconds;

			internal bool Equals(in SYSTEMTIME other)
			{
				if (Year == other.Year && Month == other.Month && DayOfWeek == other.DayOfWeek && Day == other.Day && Hour == other.Hour && Minute == other.Minute && Second == other.Second)
				{
					return Milliseconds == other.Milliseconds;
				}
				return false;
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TIME_DYNAMIC_ZONE_INFORMATION
		{
			internal int Bias;

			internal unsafe fixed char StandardName[32];

			internal SYSTEMTIME StandardDate;

			internal int StandardBias;

			internal unsafe fixed char DaylightName[32];

			internal SYSTEMTIME DaylightDate;

			internal int DaylightBias;

			internal unsafe fixed char TimeZoneKeyName[128];

			internal byte DynamicDaylightTimeDisabled;

			internal unsafe string GetTimeZoneKeyName()
			{
				fixed (char* timeZoneKeyName = TimeZoneKeyName)
				{
					return new string(timeZoneKeyName);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TIME_ZONE_INFORMATION
		{
			internal int Bias;

			internal unsafe fixed char StandardName[32];

			internal SYSTEMTIME StandardDate;

			internal int StandardBias;

			internal unsafe fixed char DaylightName[32];

			internal SYSTEMTIME DaylightDate;

			internal int DaylightBias;

			internal unsafe TIME_ZONE_INFORMATION(in TIME_DYNAMIC_ZONE_INFORMATION dtzi)
			{
				fixed (TIME_ZONE_INFORMATION* ptr = &this)
				{
					fixed (TIME_DYNAMIC_ZONE_INFORMATION* ptr2 = &dtzi)
					{
						*ptr = *(TIME_ZONE_INFORMATION*)ptr2;
					}
				}
			}

			internal unsafe string GetStandardName()
			{
				fixed (char* standardName = StandardName)
				{
					return new string(standardName);
				}
			}

			internal unsafe string GetDaylightName()
			{
				fixed (char* daylightName = DaylightName)
				{
					return new string(daylightName);
				}
			}
		}

		internal enum FILE_INFO_BY_HANDLE_CLASS : uint
		{
			FileBasicInfo,
			FileStandardInfo,
			FileNameInfo,
			FileRenameInfo,
			FileDispositionInfo,
			FileAllocationInfo,
			FileEndOfFileInfo,
			FileStreamInfo,
			FileCompressionInfo,
			FileAttributeTagInfo,
			FileIdBothDirectoryInfo,
			FileIdBothDirectoryRestartInfo,
			FileIoPriorityHintInfo,
			FileRemoteProtocolInfo,
			FileFullDirectoryInfo,
			FileFullDirectoryRestartInfo
		}

		internal struct FILE_TIME
		{
			internal uint dwLowDateTime;

			internal uint dwHighDateTime;

			internal FILE_TIME(long fileTime)
			{
				dwLowDateTime = (uint)fileTime;
				dwHighDateTime = (uint)(fileTime >> 32);
			}

			internal long ToTicks()
			{
				return (long)(((ulong)dwHighDateTime << 32) + dwLowDateTime);
			}

			internal DateTime ToDateTimeUtc()
			{
				return DateTime.FromFileTimeUtc(ToTicks());
			}

			internal DateTimeOffset ToDateTimeOffset()
			{
				return DateTimeOffset.FromFileTime(ToTicks());
			}
		}

		internal enum FINDEX_INFO_LEVELS : uint
		{
			FindExInfoStandard,
			FindExInfoBasic,
			FindExInfoMaxInfoLevel
		}

		internal enum FINDEX_SEARCH_OPS : uint
		{
			FindExSearchNameMatch,
			FindExSearchLimitToDirectories,
			FindExSearchLimitToDevices,
			FindExSearchMaxSearchOp
		}

		internal class FileAttributes
		{
			internal const int FILE_ATTRIBUTE_NORMAL = 128;

			internal const int FILE_ATTRIBUTE_READONLY = 1;

			internal const int FILE_ATTRIBUTE_DIRECTORY = 16;

			internal const int FILE_ATTRIBUTE_REPARSE_POINT = 1024;
		}

		internal class IOReparseOptions
		{
			internal const uint IO_REPARSE_TAG_FILE_PLACEHOLDER = 2147483669u;

			internal const uint IO_REPARSE_TAG_MOUNT_POINT = 2684354563u;
		}

		internal class FileOperations
		{
			internal const int OPEN_EXISTING = 3;

			internal const int COPY_FILE_FAIL_IF_EXISTS = 1;

			internal const int FILE_ACTION_ADDED = 1;

			internal const int FILE_ACTION_REMOVED = 2;

			internal const int FILE_ACTION_MODIFIED = 3;

			internal const int FILE_ACTION_RENAMED_OLD_NAME = 4;

			internal const int FILE_ACTION_RENAMED_NEW_NAME = 5;

			internal const int FILE_FLAG_BACKUP_SEMANTICS = 33554432;

			internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 524288;

			internal const int FILE_FLAG_OVERLAPPED = 1073741824;

			internal const int FILE_LIST_DIRECTORY = 1;
		}

		internal enum GET_FILEEX_INFO_LEVELS : uint
		{
			GetFileExInfoStandard,
			GetFileExMaxInfoLevel
		}

		internal class GenericOperations
		{
			internal const int GENERIC_READ = int.MinValue;

			internal const int GENERIC_WRITE = 1073741824;
		}

		internal struct SECURITY_ATTRIBUTES
		{
			internal uint nLength;

			internal IntPtr lpSecurityDescriptor;

			internal BOOL bInheritHandle;
		}

		internal struct FILE_BASIC_INFO
		{
			internal long CreationTime;

			internal long LastAccessTime;

			internal long LastWriteTime;

			internal long ChangeTime;

			internal uint FileAttributes;
		}

		internal struct WIN32_FILE_ATTRIBUTE_DATA
		{
			internal int dwFileAttributes;

			internal FILE_TIME ftCreationTime;

			internal FILE_TIME ftLastAccessTime;

			internal FILE_TIME ftLastWriteTime;

			internal uint nFileSizeHigh;

			internal uint nFileSizeLow;

			internal void PopulateFrom(ref WIN32_FIND_DATA findData)
			{
				dwFileAttributes = (int)findData.dwFileAttributes;
				ftCreationTime = findData.ftCreationTime;
				ftLastAccessTime = findData.ftLastAccessTime;
				ftLastWriteTime = findData.ftLastWriteTime;
				nFileSizeHigh = findData.nFileSizeHigh;
				nFileSizeLow = findData.nFileSizeLow;
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct WIN32_FIND_DATA
		{
			internal uint dwFileAttributes;

			internal FILE_TIME ftCreationTime;

			internal FILE_TIME ftLastAccessTime;

			internal FILE_TIME ftLastWriteTime;

			internal uint nFileSizeHigh;

			internal uint nFileSizeLow;

			internal uint dwReserved0;

			internal uint dwReserved1;

			private unsafe fixed char _cFileName[260];

			private unsafe fixed char _cAlternateFileName[14];

			internal unsafe ReadOnlySpan<char> cFileName
			{
				get
				{
					fixed (char* pointer = _cFileName)
					{
						return new ReadOnlySpan<char>(pointer, 260);
					}
				}
			}
		}

		internal const int LOAD_LIBRARY_AS_DATAFILE = 2;

		internal const int MAX_PATH = 260;

		internal const uint MUI_PREFERRED_UI_LANGUAGES = 16u;

		internal const uint TIME_ZONE_ID_INVALID = uint.MaxValue;

		internal const uint SEM_FAILCRITICALERRORS = 1u;

		private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;

		private const int FORMAT_MESSAGE_FROM_HMODULE = 2048;

		private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

		private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		private const int InitialBufferSize = 256;

		private const int BufferSizeIncreaseFactor = 4;

		private const int MaxAllowedBufferSize = 66560;

		internal const int REPLACEFILE_IGNORE_MERGE_ERRORS = 2;

		internal static int CopyFile(string src, string dst, bool failIfExists)
		{
			int flags = (failIfExists ? 1 : 0);
			int cancel = 0;
			if (!CopyFileEx(src, dst, IntPtr.Zero, IntPtr.Zero, ref cancel, flags))
			{
				return Marshal.GetLastWin32Error();
			}
			return 0;
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "DeleteVolumeMountPointW", SetLastError = true)]
		internal static extern bool DeleteVolumeMountPointPrivate(string mountPoint);

		internal static bool DeleteVolumeMountPoint(string mountPoint)
		{
			mountPoint = PathInternal.EnsureExtendedPrefixIfNeeded(mountPoint);
			return DeleteVolumeMountPointPrivate(mountPoint);
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryExW", SetLastError = true)]
		internal static extern SafeLibraryHandle LoadLibraryEx(string libFilename, IntPtr reserved, int flags);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal static extern bool GetFileMUIPath(uint flags, string filePath, [Out] StringBuilder language, ref int languageLength, [Out] StringBuilder fileMuiPath, ref int fileMuiPathLength, ref long enumerator);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal static extern uint GetDynamicTimeZoneInformation(out TIME_DYNAMIC_ZONE_INFORMATION pTimeZoneInformation);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal static extern uint GetTimeZoneInformation(out TIME_ZONE_INFORMATION lpTimeZoneInformation);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CopyFileExW", SetLastError = true)]
		private static extern bool CopyFileExPrivate(string src, string dst, IntPtr progressRoutine, IntPtr progressData, ref int cancel, int flags);

		internal static bool CopyFileEx(string src, string dst, IntPtr progressRoutine, IntPtr progressData, ref int cancel, int flags)
		{
			src = PathInternal.EnsureExtendedPrefixIfNeeded(src);
			dst = PathInternal.EnsureExtendedPrefixIfNeeded(dst);
			return CopyFileExPrivate(src, dst, progressRoutine, progressData, ref cancel, flags);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateDirectoryW", SetLastError = true)]
		private static extern bool CreateDirectoryPrivate(string path, ref SECURITY_ATTRIBUTES lpSecurityAttributes);

		internal static bool CreateDirectory(string path, ref SECURITY_ATTRIBUTES lpSecurityAttributes)
		{
			path = PathInternal.EnsureExtendedPrefix(path);
			return CreateDirectoryPrivate(path, ref lpSecurityAttributes);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", ExactSpelling = true, SetLastError = true)]
		private unsafe static extern IntPtr CreateFilePrivate(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES* securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

		internal unsafe static SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, ref SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
		{
			lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
			fixed (SECURITY_ATTRIBUTES* securityAttrs2 = &securityAttrs)
			{
				IntPtr intPtr = CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs2, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
				try
				{
					return new SafeFileHandle(intPtr, ownsHandle: true);
				}
				catch
				{
					CloseHandle(intPtr);
					throw;
				}
			}
		}

		internal static SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
		{
			IntPtr intPtr = CreateFile_IntPtr(lpFileName, dwDesiredAccess, dwShareMode, dwCreationDisposition, dwFlagsAndAttributes);
			try
			{
				return new SafeFileHandle(intPtr, ownsHandle: true);
			}
			catch
			{
				CloseHandle(intPtr);
				throw;
			}
		}

		internal unsafe static IntPtr CreateFile_IntPtr(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
		{
			lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
			return CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, null, dwCreationDisposition, dwFlagsAndAttributes, IntPtr.Zero);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "DeleteFileW", SetLastError = true)]
		private static extern bool DeleteFilePrivate(string path);

		internal static bool DeleteFile(string path)
		{
			path = PathInternal.EnsureExtendedPrefixIfNeeded(path);
			return DeleteFilePrivate(path);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "FindFirstFileExW", SetLastError = true)]
		private static extern SafeFindHandle FindFirstFileExPrivate(string lpFileName, FINDEX_INFO_LEVELS fInfoLevelId, ref WIN32_FIND_DATA lpFindFileData, FINDEX_SEARCH_OPS fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlags);

		internal static SafeFindHandle FindFirstFile(string fileName, ref WIN32_FIND_DATA data)
		{
			fileName = PathInternal.EnsureExtendedPrefixIfNeeded(fileName);
			return FindFirstFileExPrivate(fileName, FINDEX_INFO_LEVELS.FindExInfoBasic, ref data, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, 0);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "FindNextFileW", SetLastError = true)]
		internal static extern bool FindNextFile(SafeFindHandle hndFindFile, ref WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW", SetLastError = true)]
		private unsafe static extern int FormatMessage(int dwFlags, IntPtr lpSource, uint dwMessageId, int dwLanguageId, char* lpBuffer, int nSize, IntPtr[] arguments);

		internal static string GetMessage(int errorCode)
		{
			return GetMessage(IntPtr.Zero, errorCode);
		}

		internal static string GetMessage(IntPtr moduleHandle, int errorCode)
		{
			Span<char> buffer = stackalloc char[256];
			do
			{
				if (TryGetErrorMessage(moduleHandle, errorCode, buffer, out var errorMsg))
				{
					return errorMsg;
				}
				buffer = new char[buffer.Length * 4];
			}
			while (buffer.Length < 66560);
			return $"Unknown error (0x{errorCode:x})";
		}

		private unsafe static bool TryGetErrorMessage(IntPtr moduleHandle, int errorCode, Span<char> buffer, out string errorMsg)
		{
			int num = 12800;
			if (moduleHandle != IntPtr.Zero)
			{
				num |= 0x800;
			}
			int num2;
			fixed (char* reference = &MemoryMarshal.GetReference(buffer))
			{
				num2 = FormatMessage(num, moduleHandle, (uint)errorCode, 0, reference, buffer.Length, null);
			}
			if (num2 != 0)
			{
				int num3;
				for (num3 = num2; num3 > 0; num3--)
				{
					char c = buffer[num3 - 1];
					if (c > ' ' && c != '.')
					{
						break;
					}
				}
				errorMsg = buffer.Slice(0, num3).ToString();
			}
			else
			{
				if (Marshal.GetLastWin32Error() == 122)
				{
					errorMsg = "";
					return false;
				}
				errorMsg = $"Unknown error (0x{errorCode:x})";
			}
			return true;
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "GetFileAttributesExW", SetLastError = true)]
		private static extern bool GetFileAttributesExPrivate(string name, GET_FILEEX_INFO_LEVELS fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

		internal static bool GetFileAttributesEx(string name, GET_FILEEX_INFO_LEVELS fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation)
		{
			name = PathInternal.EnsureExtendedPrefixIfNeeded(name);
			return GetFileAttributesExPrivate(name, fileInfoLevel, ref lpFileInformation);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetLogicalDrives();

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "MoveFileExW", SetLastError = true)]
		private static extern bool MoveFileExPrivate(string src, string dst, uint flags);

		internal static bool MoveFile(string src, string dst)
		{
			src = PathInternal.EnsureExtendedPrefixIfNeeded(src);
			dst = PathInternal.EnsureExtendedPrefixIfNeeded(dst);
			return MoveFileExPrivate(src, dst, 2u);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RemoveDirectoryW", SetLastError = true)]
		private static extern bool RemoveDirectoryPrivate(string path);

		internal static bool RemoveDirectory(string path)
		{
			path = PathInternal.EnsureExtendedPrefixIfNeeded(path);
			return RemoveDirectoryPrivate(path);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "ReplaceFileW", SetLastError = true)]
		private static extern bool ReplaceFilePrivate(string replacedFileName, string replacementFileName, string backupFileName, int dwReplaceFlags, IntPtr lpExclude, IntPtr lpReserved);

		internal static bool ReplaceFile(string replacedFileName, string replacementFileName, string backupFileName, int dwReplaceFlags, IntPtr lpExclude, IntPtr lpReserved)
		{
			replacedFileName = PathInternal.EnsureExtendedPrefixIfNeeded(replacedFileName);
			replacementFileName = PathInternal.EnsureExtendedPrefixIfNeeded(replacementFileName);
			backupFileName = PathInternal.EnsureExtendedPrefixIfNeeded(backupFileName);
			return ReplaceFilePrivate(replacedFileName, replacementFileName, backupFileName, dwReplaceFlags, lpExclude, lpReserved);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetFileAttributesW", SetLastError = true)]
		private static extern bool SetFileAttributesPrivate(string name, int attr);

		internal static bool SetFileAttributes(string name, int attr)
		{
			name = PathInternal.EnsureExtendedPrefixIfNeeded(name);
			return SetFileAttributesPrivate(name, attr);
		}

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool SetFileInformationByHandle(SafeFileHandle hFile, FILE_INFO_BY_HANDLE_CLASS FileInformationClass, ref FILE_BASIC_INFO lpFileInformation, uint dwBufferSize);

		internal unsafe static bool SetFileTime(SafeFileHandle hFile, long creationTime = -1L, long lastAccessTime = -1L, long lastWriteTime = -1L, long changeTime = -1L, uint fileAttributes = 0u)
		{
			FILE_BASIC_INFO lpFileInformation = new FILE_BASIC_INFO
			{
				CreationTime = creationTime,
				LastAccessTime = lastAccessTime,
				LastWriteTime = lastWriteTime,
				ChangeTime = changeTime,
				FileAttributes = fileAttributes
			};
			return SetFileInformationByHandle(hFile, FILE_INFO_BY_HANDLE_CLASS.FileBasicInfo, ref lpFileInformation, (uint)sizeof(FILE_BASIC_INFO));
		}

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool SetThreadErrorMode(uint dwNewMode, out uint lpOldMode);
	}

	internal class BCrypt
	{
		internal enum NTSTATUS : uint
		{
			STATUS_SUCCESS = 0u,
			STATUS_NOT_FOUND = 3221226021u,
			STATUS_INVALID_PARAMETER = 3221225485u,
			STATUS_NO_MEMORY = 3221225495u
		}

		internal const int BCRYPT_USE_SYSTEM_PREFERRED_RNG = 2;

		[DllImport("BCrypt.dll", CharSet = CharSet.Unicode)]
		internal unsafe static extern NTSTATUS BCryptGenRandom(IntPtr hAlgorithm, byte* pbBuffer, int cbBuffer, int dwFlags);
	}

	internal class User32
	{
		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadStringW", SetLastError = true)]
		internal static extern int LoadString(SafeLibraryHandle handle, int id, [Out] StringBuilder buffer, int bufferLength);
	}

	internal enum BOOL
	{
		FALSE,
		TRUE
	}

	internal enum BOOLEAN : byte
	{
		FALSE,
		TRUE
	}

	internal class Errors
	{
		internal const int ERROR_SUCCESS = 0;

		internal const int ERROR_INVALID_FUNCTION = 1;

		internal const int ERROR_FILE_NOT_FOUND = 2;

		internal const int ERROR_PATH_NOT_FOUND = 3;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal const int ERROR_INVALID_HANDLE = 6;

		internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

		internal const int ERROR_INVALID_DATA = 13;

		internal const int ERROR_INVALID_DRIVE = 15;

		internal const int ERROR_NO_MORE_FILES = 18;

		internal const int ERROR_NOT_READY = 21;

		internal const int ERROR_BAD_COMMAND = 22;

		internal const int ERROR_BAD_LENGTH = 24;

		internal const int ERROR_SHARING_VIOLATION = 32;

		internal const int ERROR_LOCK_VIOLATION = 33;

		internal const int ERROR_HANDLE_EOF = 38;

		internal const int ERROR_BAD_NETPATH = 53;

		internal const int ERROR_BAD_NET_NAME = 67;

		internal const int ERROR_FILE_EXISTS = 80;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal const int ERROR_BROKEN_PIPE = 109;

		internal const int ERROR_SEM_TIMEOUT = 121;

		internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;

		internal const int ERROR_INSUFFICIENT_BUFFER = 122;

		internal const int ERROR_INVALID_NAME = 123;

		internal const int ERROR_NEGATIVE_SEEK = 131;

		internal const int ERROR_DIR_NOT_EMPTY = 145;

		internal const int ERROR_BAD_PATHNAME = 161;

		internal const int ERROR_LOCK_FAILED = 167;

		internal const int ERROR_BUSY = 170;

		internal const int ERROR_ALREADY_EXISTS = 183;

		internal const int ERROR_BAD_EXE_FORMAT = 193;

		internal const int ERROR_ENVVAR_NOT_FOUND = 203;

		internal const int ERROR_FILENAME_EXCED_RANGE = 206;

		internal const int ERROR_EXE_MACHINE_TYPE_MISMATCH = 216;

		internal const int ERROR_PIPE_BUSY = 231;

		internal const int ERROR_NO_DATA = 232;

		internal const int ERROR_PIPE_NOT_CONNECTED = 233;

		internal const int ERROR_MORE_DATA = 234;

		internal const int ERROR_NO_MORE_ITEMS = 259;

		internal const int ERROR_DIRECTORY = 267;

		internal const int ERROR_PARTIAL_COPY = 299;

		internal const int ERROR_ARITHMETIC_OVERFLOW = 534;

		internal const int ERROR_PIPE_CONNECTED = 535;

		internal const int ERROR_PIPE_LISTENING = 536;

		internal const int ERROR_OPERATION_ABORTED = 995;

		internal const int ERROR_IO_INCOMPLETE = 996;

		internal const int ERROR_IO_PENDING = 997;

		internal const int ERROR_NO_TOKEN = 1008;

		internal const int ERROR_DLL_INIT_FAILED = 1114;

		internal const int ERROR_COUNTER_TIMEOUT = 1121;

		internal const int ERROR_NO_ASSOCIATION = 1155;

		internal const int ERROR_DDE_FAIL = 1156;

		internal const int ERROR_DLL_NOT_FOUND = 1157;

		internal const int ERROR_NOT_FOUND = 1168;

		internal const int ERROR_NETWORK_UNREACHABLE = 1231;

		internal const int ERROR_NON_ACCOUNT_SID = 1257;

		internal const int ERROR_NOT_ALL_ASSIGNED = 1300;

		internal const int ERROR_UNKNOWN_REVISION = 1305;

		internal const int ERROR_INVALID_OWNER = 1307;

		internal const int ERROR_INVALID_PRIMARY_GROUP = 1308;

		internal const int ERROR_NO_SUCH_PRIVILEGE = 1313;

		internal const int ERROR_PRIVILEGE_NOT_HELD = 1314;

		internal const int ERROR_INVALID_ACL = 1336;

		internal const int ERROR_INVALID_SECURITY_DESCR = 1338;

		internal const int ERROR_INVALID_SID = 1337;

		internal const int ERROR_BAD_IMPERSONATION_LEVEL = 1346;

		internal const int ERROR_CANT_OPEN_ANONYMOUS = 1347;

		internal const int ERROR_NO_SECURITY_ON_OBJECT = 1350;

		internal const int ERROR_CLASS_ALREADY_EXISTS = 1410;

		internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789;

		internal const int ERROR_RESOURCE_LANG_NOT_FOUND = 1815;

		internal const int EFail = -2147467259;

		internal const int E_FILENOTFOUND = -2147024894;
	}

	internal static class Libraries
	{
		internal const string Advapi32 = "advapi32.dll";

		internal const string BCrypt = "BCrypt.dll";

		internal const string CoreComm_L1_1_1 = "api-ms-win-core-comm-l1-1-1.dll";

		internal const string Crypt32 = "crypt32.dll";

		internal const string Error_L1 = "api-ms-win-core-winrt-error-l1-1-0.dll";

		internal const string HttpApi = "httpapi.dll";

		internal const string IpHlpApi = "iphlpapi.dll";

		internal const string Kernel32 = "kernel32.dll";

		internal const string Memory_L1_3 = "api-ms-win-core-memory-l1-1-3.dll";

		internal const string Mswsock = "mswsock.dll";

		internal const string NCrypt = "ncrypt.dll";

		internal const string NtDll = "ntdll.dll";

		internal const string Odbc32 = "odbc32.dll";

		internal const string OleAut32 = "oleaut32.dll";

		internal const string PerfCounter = "perfcounter.dll";

		internal const string RoBuffer = "api-ms-win-core-winrt-robuffer-l1-1-0.dll";

		internal const string Secur32 = "secur32.dll";

		internal const string Shell32 = "shell32.dll";

		internal const string SspiCli = "sspicli.dll";

		internal const string User32 = "user32.dll";

		internal const string Version = "version.dll";

		internal const string WebSocket = "websocket.dll";

		internal const string WinHttp = "winhttp.dll";

		internal const string Ws2_32 = "ws2_32.dll";

		internal const string Wtsapi32 = "wtsapi32.dll";

		internal const string CompressionNative = "clrcompression.dll";

		internal const string ErrorHandling = "api-ms-win-core-errorhandling-l1-1-0.dll";

		internal const string Handle = "api-ms-win-core-handle-l1-1-0.dll";

		internal const string IO = "api-ms-win-core-io-l1-1-0.dll";

		internal const string Memory = "api-ms-win-core-memory-l1-1-0.dll";

		internal const string ProcessEnvironment = "api-ms-win-core-processenvironment-l1-1-0.dll";

		internal const string ProcessThreads = "api-ms-win-core-processthreads-l1-1-0.dll";

		internal const string RealTime = "api-ms-win-core-realtime-l1-1-0.dll";

		internal const string SysInfo = "api-ms-win-core-sysinfo-l1-2-0.dll";

		internal const string ThreadPool = "api-ms-win-core-threadpool-l1-2-0.dll";

		internal const string Localization = "api-ms-win-core-localization-l1-2-1.dll";
	}

	internal struct LongFileTime
	{
		internal long TicksSince1601;

		internal DateTimeOffset ToDateTimeOffset()
		{
			return new DateTimeOffset(DateTime.FromFileTimeUtc(TicksSince1601));
		}
	}

	internal struct UNICODE_STRING
	{
		internal ushort Length;

		internal ushort MaximumLength;

		internal IntPtr Buffer;
	}

	internal class NtDll
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct FILE_FULL_DIR_INFORMATION
		{
			public uint NextEntryOffset;

			public uint FileIndex;

			public LongFileTime CreationTime;

			public LongFileTime LastAccessTime;

			public LongFileTime LastWriteTime;

			public LongFileTime ChangeTime;

			public long EndOfFile;

			public long AllocationSize;

			public FileAttributes FileAttributes;

			public uint FileNameLength;

			public uint EaSize;

			private char _fileName;

			public unsafe ReadOnlySpan<char> FileName
			{
				get
				{
					fixed (char* fileName = &_fileName)
					{
						return new ReadOnlySpan<char>(fileName, (int)FileNameLength / 2);
					}
				}
			}

			public unsafe static FILE_FULL_DIR_INFORMATION* GetNextInfo(FILE_FULL_DIR_INFORMATION* info)
			{
				if (info == null)
				{
					return null;
				}
				uint nextEntryOffset = info->NextEntryOffset;
				if (nextEntryOffset == 0)
				{
					return null;
				}
				return (FILE_FULL_DIR_INFORMATION*)((byte*)info + nextEntryOffset);
			}
		}

		public enum FILE_INFORMATION_CLASS : uint
		{
			FileDirectoryInformation = 1u,
			FileFullDirectoryInformation,
			FileBothDirectoryInformation,
			FileBasicInformation,
			FileStandardInformation,
			FileInternalInformation,
			FileEaInformation,
			FileAccessInformation,
			FileNameInformation,
			FileRenameInformation,
			FileLinkInformation,
			FileNamesInformation,
			FileDispositionInformation,
			FilePositionInformation,
			FileFullEaInformation,
			FileModeInformation,
			FileAlignmentInformation,
			FileAllInformation,
			FileAllocationInformation,
			FileEndOfFileInformation,
			FileAlternateNameInformation,
			FileStreamInformation,
			FilePipeInformation,
			FilePipeLocalInformation,
			FilePipeRemoteInformation,
			FileMailslotQueryInformation,
			FileMailslotSetInformation,
			FileCompressionInformation,
			FileObjectIdInformation,
			FileCompletionInformation,
			FileMoveClusterInformation,
			FileQuotaInformation,
			FileReparsePointInformation,
			FileNetworkOpenInformation,
			FileAttributeTagInformation,
			FileTrackingInformation,
			FileIdBothDirectoryInformation,
			FileIdFullDirectoryInformation,
			FileValidDataLengthInformation,
			FileShortNameInformation,
			FileIoCompletionNotificationInformation,
			FileIoStatusBlockRangeInformation,
			FileIoPriorityHintInformation,
			FileSfioReserveInformation,
			FileSfioVolumeInformation,
			FileHardLinkInformation,
			FileProcessIdsUsingFileInformation,
			FileNormalizedNameInformation,
			FileNetworkPhysicalNameInformation,
			FileIdGlobalTxDirectoryInformation,
			FileIsRemoteDeviceInformation,
			FileUnusedInformation,
			FileNumaNodeInformation,
			FileStandardLinkInformation,
			FileRemoteProtocolInformation,
			FileRenameInformationBypassAccessCheck,
			FileLinkInformationBypassAccessCheck,
			FileVolumeNameInformation,
			FileIdInformation,
			FileIdExtdDirectoryInformation,
			FileReplaceCompletionInformation,
			FileHardLinkFullIdInformation,
			FileIdExtdBothDirectoryInformation,
			FileDispositionInformationEx,
			FileRenameInformationEx,
			FileRenameInformationExBypassAccessCheck,
			FileDesiredStorageClassInformation,
			FileStatInformation
		}

		public struct IO_STATUS_BLOCK
		{
			[StructLayout(LayoutKind.Explicit)]
			public struct IO_STATUS
			{
				[FieldOffset(0)]
				public uint Status;

				[FieldOffset(0)]
				public IntPtr Pointer;
			}

			public IO_STATUS Status;

			public IntPtr Information;
		}

		public struct OBJECT_ATTRIBUTES
		{
			public uint Length;

			public IntPtr RootDirectory;

			public unsafe UNICODE_STRING* ObjectName;

			public ObjectAttributes Attributes;

			public unsafe void* SecurityDescriptor;

			public unsafe void* SecurityQualityOfService;

			public unsafe OBJECT_ATTRIBUTES(UNICODE_STRING* objectName, ObjectAttributes attributes, IntPtr rootDirectory)
			{
				Length = (uint)sizeof(OBJECT_ATTRIBUTES);
				RootDirectory = rootDirectory;
				ObjectName = objectName;
				Attributes = attributes;
				SecurityDescriptor = null;
				SecurityQualityOfService = null;
			}
		}

		[Flags]
		public enum ObjectAttributes : uint
		{
			OBJ_INHERIT = 2u,
			OBJ_PERMANENT = 0x10u,
			OBJ_EXCLUSIVE = 0x20u,
			OBJ_CASE_INSENSITIVE = 0x40u,
			OBJ_OPENIF = 0x80u,
			OBJ_OPENLINK = 0x100u
		}

		public enum CreateDisposition : uint
		{
			FILE_SUPERSEDE,
			FILE_OPEN,
			FILE_CREATE,
			FILE_OPEN_IF,
			FILE_OVERWRITE,
			FILE_OVERWRITE_IF
		}

		public enum CreateOptions : uint
		{
			FILE_DIRECTORY_FILE = 1u,
			FILE_WRITE_THROUGH = 2u,
			FILE_SEQUENTIAL_ONLY = 4u,
			FILE_NO_INTERMEDIATE_BUFFERING = 8u,
			FILE_SYNCHRONOUS_IO_ALERT = 0x10u,
			FILE_SYNCHRONOUS_IO_NONALERT = 0x20u,
			FILE_NON_DIRECTORY_FILE = 0x40u,
			FILE_CREATE_TREE_CONNECTION = 0x80u,
			FILE_COMPLETE_IF_OPLOCKED = 0x100u,
			FILE_NO_EA_KNOWLEDGE = 0x200u,
			FILE_RANDOM_ACCESS = 0x800u,
			FILE_DELETE_ON_CLOSE = 0x1000u,
			FILE_OPEN_BY_FILE_ID = 0x2000u,
			FILE_OPEN_FOR_BACKUP_INTENT = 0x4000u,
			FILE_NO_COMPRESSION = 0x8000u,
			FILE_OPEN_REQUIRING_OPLOCK = 0x10000u,
			FILE_DISALLOW_EXCLUSIVE = 0x20000u,
			FILE_SESSION_AWARE = 0x40000u,
			FILE_RESERVE_OPFILTER = 0x100000u,
			FILE_OPEN_REPARSE_POINT = 0x200000u,
			FILE_OPEN_NO_RECALL = 0x400000u
		}

		[Flags]
		public enum DesiredAccess : uint
		{
			FILE_READ_DATA = 1u,
			FILE_LIST_DIRECTORY = 1u,
			FILE_WRITE_DATA = 2u,
			FILE_ADD_FILE = 2u,
			FILE_APPEND_DATA = 4u,
			FILE_ADD_SUBDIRECTORY = 4u,
			FILE_CREATE_PIPE_INSTANCE = 4u,
			FILE_READ_EA = 8u,
			FILE_WRITE_EA = 0x10u,
			FILE_EXECUTE = 0x20u,
			FILE_TRAVERSE = 0x20u,
			FILE_DELETE_CHILD = 0x40u,
			FILE_READ_ATTRIBUTES = 0x80u,
			FILE_WRITE_ATTRIBUTES = 0x100u,
			FILE_ALL_ACCESS = 0xF01FFu,
			DELETE = 0x10000u,
			READ_CONTROL = 0x20000u,
			WRITE_DAC = 0x40000u,
			WRITE_OWNER = 0x80000u,
			SYNCHRONIZE = 0x100000u,
			STANDARD_RIGHTS_READ = 0x20000u,
			STANDARD_RIGHTS_WRITE = 0x20000u,
			STANDARD_RIGHTS_EXECUTE = 0x20000u,
			FILE_GENERIC_READ = 0x80000000u,
			FILE_GENERIC_WRITE = 0x40000000u,
			FILE_GENERIC_EXECUTE = 0x20000000u
		}

		[DllImport("ntdll.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private unsafe static extern int NtCreateFile(out IntPtr FileHandle, DesiredAccess DesiredAccess, ref OBJECT_ATTRIBUTES ObjectAttributes, out IO_STATUS_BLOCK IoStatusBlock, long* AllocationSize, FileAttributes FileAttributes, FileShare ShareAccess, CreateDisposition CreateDisposition, CreateOptions CreateOptions, void* EaBuffer, uint EaLength);

		internal unsafe static (int status, IntPtr handle) CreateFile(ReadOnlySpan<char> path, IntPtr rootDirectory, CreateDisposition createDisposition, DesiredAccess desiredAccess = DesiredAccess.SYNCHRONIZE | DesiredAccess.FILE_GENERIC_READ, FileShare shareAccess = FileShare.ReadWrite | FileShare.Delete, FileAttributes fileAttributes = (FileAttributes)0, CreateOptions createOptions = CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT, ObjectAttributes objectAttributes = NtDll.ObjectAttributes.OBJ_CASE_INSENSITIVE)
		{
			fixed (char* reference = &MemoryMarshal.GetReference(path))
			{
				UNICODE_STRING uNICODE_STRING = checked(new UNICODE_STRING
				{
					Length = (ushort)(path.Length * 2),
					MaximumLength = (ushort)(path.Length * 2),
					Buffer = (IntPtr)reference
				});
				OBJECT_ATTRIBUTES ObjectAttributes = new OBJECT_ATTRIBUTES(&uNICODE_STRING, objectAttributes, rootDirectory);
				IntPtr FileHandle;
				IO_STATUS_BLOCK IoStatusBlock;
				return (status: NtCreateFile(out FileHandle, desiredAccess, ref ObjectAttributes, out IoStatusBlock, null, fileAttributes, shareAccess, createDisposition, createOptions, null, 0u), handle: FileHandle);
			}
		}

		[DllImport("ntdll.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		public unsafe static extern int NtQueryDirectoryFile(IntPtr FileHandle, IntPtr Event, IntPtr ApcRoutine, IntPtr ApcContext, out IO_STATUS_BLOCK IoStatusBlock, IntPtr FileInformation, uint Length, FILE_INFORMATION_CLASS FileInformationClass, BOOLEAN ReturnSingleEntry, UNICODE_STRING* FileName, BOOLEAN RestartScan);

		[DllImport("ntdll.dll", ExactSpelling = true)]
		public static extern uint RtlNtStatusToDosError(int Status);
	}

	internal class StatusOptions
	{
		internal const uint STATUS_SUCCESS = 0u;

		internal const uint STATUS_SOME_NOT_MAPPED = 263u;

		internal const uint STATUS_NO_MORE_FILES = 2147483654u;

		internal const uint STATUS_INVALID_PARAMETER = 3221225485u;

		internal const uint STATUS_NO_MEMORY = 3221225495u;

		internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 3221225524u;

		internal const uint STATUS_NONE_MAPPED = 3221225587u;

		internal const uint STATUS_INSUFFICIENT_RESOURCES = 3221225626u;

		internal const uint STATUS_ACCESS_DENIED = 3221225506u;

		internal const uint STATUS_ACCOUNT_RESTRICTION = 3221225582u;
	}

	internal class Advapi32
	{
		internal class RegistryOptions
		{
			internal const int REG_OPTION_NON_VOLATILE = 0;

			internal const int REG_OPTION_VOLATILE = 1;

			internal const int REG_OPTION_CREATE_LINK = 2;

			internal const int REG_OPTION_BACKUP_RESTORE = 4;
		}

		internal class RegistryView
		{
			internal const int KEY_WOW64_64KEY = 256;

			internal const int KEY_WOW64_32KEY = 512;
		}

		internal class RegistryOperations
		{
			internal const int KEY_QUERY_VALUE = 1;

			internal const int KEY_SET_VALUE = 2;

			internal const int KEY_CREATE_SUB_KEY = 4;

			internal const int KEY_ENUMERATE_SUB_KEYS = 8;

			internal const int KEY_NOTIFY = 16;

			internal const int KEY_CREATE_LINK = 32;

			internal const int KEY_READ = 131097;

			internal const int KEY_WRITE = 131078;

			internal const int SYNCHRONIZE = 1048576;

			internal const int READ_CONTROL = 131072;

			internal const int STANDARD_RIGHTS_READ = 131072;

			internal const int STANDARD_RIGHTS_WRITE = 131072;
		}

		internal class RegistryValues
		{
			internal const int REG_NONE = 0;

			internal const int REG_SZ = 1;

			internal const int REG_EXPAND_SZ = 2;

			internal const int REG_BINARY = 3;

			internal const int REG_DWORD = 4;

			internal const int REG_DWORD_LITTLE_ENDIAN = 4;

			internal const int REG_DWORD_BIG_ENDIAN = 5;

			internal const int REG_LINK = 6;

			internal const int REG_MULTI_SZ = 7;

			internal const int REG_QWORD = 11;
		}

		[DllImport("advapi32.dll")]
		internal static extern int RegCloseKey(IntPtr hKey);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegConnectRegistryW")]
		internal static extern int RegConnectRegistry(string machineName, SafeRegistryHandle key, out SafeRegistryHandle result);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegCreateKeyExW")]
		internal static extern int RegCreateKeyEx(SafeRegistryHandle hKey, string lpSubKey, int Reserved, string lpClass, int dwOptions, int samDesired, ref Kernel32.SECURITY_ATTRIBUTES secAttrs, out SafeRegistryHandle hkResult, out int lpdwDisposition);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegDeleteKeyExW")]
		internal static extern int RegDeleteKeyEx(SafeRegistryHandle hKey, string lpSubKey, int samDesired, int Reserved);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegDeleteValueW")]
		internal static extern int RegDeleteValue(SafeRegistryHandle hKey, string lpValueName);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegEnumKeyExW")]
		internal static extern int RegEnumKeyEx(SafeRegistryHandle hKey, int dwIndex, char[] lpName, ref int lpcbName, int[] lpReserved, [Out] StringBuilder lpClass, int[] lpcbClass, long[] lpftLastWriteTime);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegEnumValueW")]
		internal static extern int RegEnumValue(SafeRegistryHandle hKey, int dwIndex, char[] lpValueName, ref int lpcbValueName, IntPtr lpReserved_MustBeZero, int[] lpType, byte[] lpData, int[] lpcbData);

		[DllImport("advapi32.dll")]
		internal static extern int RegFlushKey(SafeRegistryHandle hKey);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW")]
		internal static extern int RegOpenKeyEx(SafeRegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW")]
		internal static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegQueryInfoKeyW")]
		internal static extern int RegQueryInfoKey(SafeRegistryHandle hKey, [Out] StringBuilder lpClass, int[] lpcbClass, IntPtr lpReserved_MustBeZero, ref int lpcSubKeys, int[] lpcbMaxSubKeyLen, int[] lpcbMaxClassLen, ref int lpcValues, int[] lpcbMaxValueNameLen, int[] lpcbMaxValueLen, int[] lpcbSecurityDescriptor, int[] lpftLastWriteTime);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW")]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] byte[] lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW")]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref int lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW")]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref long lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW")]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] char[] lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW")]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] StringBuilder lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW")]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, byte[] lpData, int cbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW")]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, char[] lpData, int cbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW")]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref int lpData, int cbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW")]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref long lpData, int cbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW")]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, string lpData, int cbData);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr RegisterServiceCtrlHandler(string serviceName, Delegate callback);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr RegisterServiceCtrlHandlerEx(string serviceName, Delegate callback, IntPtr userData);
	}

	internal static class mincore
	{
		[DllImport("api-ms-win-core-heap-l1-1-0.dll")]
		internal static extern IntPtr GetProcessHeap();

		[DllImport("api-ms-win-core-heap-l1-1-0.dll")]
		internal static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

		[DllImport("api-ms-win-core-heap-l1-1-0.dll")]
		internal static extern int HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

		[DllImport("api-ms-win-core-threadpool-l1-2-0.dll", SetLastError = true)]
		internal static extern SafeThreadPoolIOHandle CreateThreadpoolIo(SafeHandle fl, IntPtr pfnio, IntPtr context, IntPtr pcbe);

		[DllImport("api-ms-win-core-threadpool-l1-2-0.dll")]
		internal static extern void CloseThreadpoolIo(IntPtr pio);

		[DllImport("api-ms-win-core-threadpool-l1-2-0.dll")]
		internal static extern void StartThreadpoolIo(SafeThreadPoolIOHandle pio);

		[DllImport("api-ms-win-core-threadpool-l1-2-0.dll")]
		internal static extern void CancelThreadpoolIo(SafeThreadPoolIOHandle pio);
	}

	internal delegate void NativeIoCompletionCallback(IntPtr instance, IntPtr context, IntPtr overlapped, uint ioResult, UIntPtr numberOfBytesTransferred, IntPtr io);

	internal unsafe static void GetRandomBytes(byte* buffer, int length)
	{
		switch (BCrypt.BCryptGenRandom(IntPtr.Zero, buffer, length, 2))
		{
		case BCrypt.NTSTATUS.STATUS_NO_MEMORY:
			throw new OutOfMemoryException();
		default:
			throw new InvalidOperationException();
		case BCrypt.NTSTATUS.STATUS_SUCCESS:
			break;
		}
	}

	internal static IntPtr MemAlloc(UIntPtr sizeInBytes)
	{
		IntPtr intPtr = mincore.HeapAlloc(mincore.GetProcessHeap(), 0u, sizeInBytes);
		if (intPtr == IntPtr.Zero)
		{
			throw new OutOfMemoryException();
		}
		return intPtr;
	}

	internal static void MemFree(IntPtr allocatedMemory)
	{
		mincore.HeapFree(mincore.GetProcessHeap(), 0u, allocatedMemory);
	}
}
