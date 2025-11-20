using System;

namespace Sony.PS4.SaveData;

public struct SaveDataParams
{
	public const int TITLE_MAXSIZE = 127;

	public const int SUBTITLE_MAXSIZE = 127;

	public const int DETAIL_MAXSIZE = 1023;

	internal string title;

	internal string subTitle;

	internal string detail;

	internal DateTime time;

	internal uint userParam;

	public string Title
	{
		get
		{
			return title;
		}
		set
		{
			title = value;
		}
	}

	public string SubTitle
	{
		get
		{
			return subTitle;
		}
		set
		{
			subTitle = value;
		}
	}

	public string Detail
	{
		get
		{
			return detail;
		}
		set
		{
			detail = value;
		}
	}

	public uint UserParam
	{
		get
		{
			return userParam;
		}
		set
		{
			userParam = value;
		}
	}

	public DateTime Time => time;

	internal void Read(MemoryBuffer buffer)
	{
		buffer.ReadString(ref title);
		buffer.ReadString(ref subTitle);
		buffer.ReadString(ref detail);
		userParam = buffer.ReadUInt32();
		long num = buffer.ReadInt64();
		try
		{
			time = new DateTime(1970, 1, 1).AddSeconds(num);
		}
		catch
		{
			time = new DateTime(1970, 1, 1);
		}
	}
}
