using System;

namespace AsmodeeNet.Utils.Extensions;

public static class DateTimeExtension
{
	public static DateTime UnixTimeStampToDateTime(this DateTime dateTime, double unixTimeStamp)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimeStamp).ToLocalTime();
	}
}
