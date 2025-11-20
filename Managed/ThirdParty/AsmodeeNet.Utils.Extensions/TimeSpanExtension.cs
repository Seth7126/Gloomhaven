using System;

namespace AsmodeeNet.Utils.Extensions;

public static class TimeSpanExtension
{
	public static string ToStringExtended(this TimeSpan timeSpan, string format)
	{
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(timeSpan.TotalSeconds);
		string result = string.Empty;
		if (timeSpan2.TotalDays >= 7.0)
		{
			int num = (int)timeSpan2.TotalDays / 7;
			int num2 = num;
			result = num2 + " week" + ((num >= 2) ? "s" : "");
			timeSpan2 = timeSpan2.Subtract(TimeSpan.FromDays(num * 7));
		}
		if (timeSpan2.TotalDays >= 1.0)
		{
			result = (int)timeSpan2.TotalDays + " day" + ((timeSpan2.TotalDays >= 2.0) ? "s" : "");
			timeSpan2 = timeSpan2.Subtract(TimeSpan.FromDays((int)timeSpan2.TotalDays));
		}
		if (timeSpan2.TotalHours >= 1.0)
		{
			result = (int)timeSpan2.TotalHours + " hour" + ((timeSpan2.TotalHours >= 2.0) ? "s" : "");
			timeSpan2 = timeSpan2.Subtract(TimeSpan.FromHours((int)timeSpan2.TotalHours));
		}
		if (timeSpan2.TotalMinutes >= 1.0)
		{
			result = (int)timeSpan2.TotalMinutes + " minute" + ((timeSpan2.TotalMinutes >= 2.0) ? "s" : "");
			timeSpan2 = timeSpan2.Subtract(TimeSpan.FromMinutes((int)timeSpan2.TotalMinutes));
		}
		if (timeSpan2.TotalSeconds >= 1.0)
		{
			result = (int)timeSpan2.TotalSeconds + " second" + ((timeSpan2.TotalSeconds >= 2.0) ? "s" : "");
		}
		return result;
	}
}
