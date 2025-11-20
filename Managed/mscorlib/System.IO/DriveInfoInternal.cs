namespace System.IO;

internal static class DriveInfoInternal
{
	public static string[] GetLogicalDrives()
	{
		int num = 0;
		num = Interop.Kernel32.GetLogicalDrives();
		if (num == 0)
		{
			throw Win32Marshal.GetExceptionForLastWin32Error();
		}
		uint num2 = (uint)num;
		int num3 = 0;
		while (num2 != 0)
		{
			if ((num2 & 1) != 0)
			{
				num3++;
			}
			num2 >>= 1;
		}
		string[] array = new string[num3];
		char[] array2 = new char[3] { 'A', ':', '\\' };
		num2 = (uint)num;
		num3 = 0;
		while (num2 != 0)
		{
			if ((num2 & 1) != 0)
			{
				array[num3++] = new string(array2);
			}
			num2 >>= 1;
			array2[0] += '\u0001';
		}
		return array;
	}
}
