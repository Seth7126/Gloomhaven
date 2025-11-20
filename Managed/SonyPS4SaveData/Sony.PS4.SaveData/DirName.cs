using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public struct DirName
{
	public const int DIRNAME_DATA_MAXSIZE = 31;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	internal string data;

	public string Data
	{
		get
		{
			return data;
		}
		set
		{
			if (value != null && value.Length > 31)
			{
				throw new SaveDataException("The length of the directory name string is more than " + 31 + " characters (DIRNAME_DATA_MAXSIZE)");
			}
			data = value;
		}
	}

	public bool IsEmpty => data == null || data.Length == 0;

	internal void Read(MemoryBuffer buffer)
	{
		buffer.ReadString(ref data);
	}

	public override string ToString()
	{
		return data;
	}
}
