using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Internal.IO;

internal static class File
{
	internal static bool InternalExists(string fullPath)
	{
		Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
		if (FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: true) == 0 && data.dwFileAttributes != -1)
		{
			return (data.dwFileAttributes & 0x10) == 0;
		}
		return false;
	}

	internal static int FillAttributeInfo(string path, ref Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data, bool returnErrorOnNotFound)
	{
		int num = 0;
		using (DisableMediaInsertionPrompt.Create())
		{
			if (!Interop.Kernel32.GetFileAttributesEx(path, Interop.Kernel32.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, ref data))
			{
				num = Marshal.GetLastWin32Error();
				if (num == 5)
				{
					Interop.Kernel32.WIN32_FIND_DATA data2 = default(Interop.Kernel32.WIN32_FIND_DATA);
					using SafeFindHandle safeFindHandle = Interop.Kernel32.FindFirstFile(path, ref data2);
					if (safeFindHandle.IsInvalid)
					{
						num = Marshal.GetLastWin32Error();
					}
					else
					{
						num = 0;
						data.PopulateFrom(ref data2);
					}
				}
			}
		}
		if (num != 0 && !returnErrorOnNotFound && ((uint)(num - 2) <= 1u || num == 21))
		{
			data.dwFileAttributes = -1;
			return 0;
		}
		return num;
	}
}
