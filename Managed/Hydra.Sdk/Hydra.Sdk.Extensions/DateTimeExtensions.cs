using System;

namespace Hydra.Sdk.Extensions;

public static class DateTimeExtensions
{
	private static readonly DateTime UnixStartTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static long ToUnixNanoseconds(this DateTime date)
	{
		return (date < UnixStartTimeUtc) ? 0 : (date.Subtract(UnixStartTimeUtc).Ticks * 100);
	}

	public static long ToUnixMilliseconds(this DateTime date)
	{
		return (date < UnixStartTimeUtc) ? 0 : ((long)date.Subtract(UnixStartTimeUtc).TotalMilliseconds);
	}

	public static long ToUnixMilliseconds(this DateTime? date)
	{
		return date.HasValue ? date.Value.ToUnixMilliseconds() : 0;
	}

	public static DateTime FromUnixMilliseconds(this long milliseconds)
	{
		DateTime unixStartTimeUtc = UnixStartTimeUtc;
		return unixStartTimeUtc.AddMilliseconds(milliseconds);
	}

	public static int ToUnixSeconds(this DateTime date)
	{
		return (!(date < UnixStartTimeUtc)) ? ((int)date.Subtract(UnixStartTimeUtc).TotalSeconds) : 0;
	}

	public static DateTime FromUnixSeconds(this int value)
	{
		DateTime unixStartTimeUtc = UnixStartTimeUtc;
		return unixStartTimeUtc.AddSeconds(value);
	}

	public static int ToUnixMinutes(this DateTime date)
	{
		return (int)date.Subtract(UnixStartTimeUtc).TotalMinutes;
	}

	public static DateTime FromUnixMinutes(this int minutes)
	{
		DateTime unixStartTimeUtc = UnixStartTimeUtc;
		return unixStartTimeUtc.AddMinutes(minutes);
	}

	public static int ToUnixHours(this DateTime date)
	{
		return (int)date.Subtract(UnixStartTimeUtc).TotalHours;
	}

	public static DateTime FromUnixHours(this int hours)
	{
		DateTime unixStartTimeUtc = UnixStartTimeUtc;
		return unixStartTimeUtc.AddHours(hours);
	}

	public static int ToUnixDays(this DateTime date)
	{
		return (int)date.Subtract(UnixStartTimeUtc).TotalDays;
	}

	public static DateTime FromUnixDays(this int days)
	{
		DateTime unixStartTimeUtc = UnixStartTimeUtc;
		return unixStartTimeUtc.AddDays(days);
	}

	public static DateTime UtcTimeWithoutSeconds(this DateTime time)
	{
		return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, DateTimeKind.Utc);
	}

	public static DateTime UtcDay(this DateTime time)
	{
		return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
	}

	public static DateTime TrimToHours(this DateTime time)
	{
		return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, DateTimeKind.Utc);
	}

	public static DateTime TrimToMinutes(this DateTime time)
	{
		return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, DateTimeKind.Utc);
	}

	public static DateTime TrimToDays(this DateTime time)
	{
		return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
	}

	public static DateTime TrimToWeeks(this DateTime time, DayOfWeek weekStart = DayOfWeek.Sunday)
	{
		DateTime result = time.TrimToDays();
		while (result.DayOfWeek != weekStart)
		{
			result = result.AddDays(-1.0);
		}
		return result;
	}

	public static DateTime TrimToMonth(this DateTime time)
	{
		return new DateTime(time.Year, time.Month, 1, 0, 0, 0, DateTimeKind.Utc);
	}

	public static DateTime TrimToYears(this DateTime time)
	{
		return new DateTime(time.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	}

	public static DateTime SetHour(this DateTime time, int hour)
	{
		return new DateTime(time.Year, time.Month, time.Day, hour, 0, 0, DateTimeKind.Utc);
	}

	public static DateTime SetMinute(this DateTime time, int minute)
	{
		return new DateTime(time.Year, time.Month, time.Day, time.Hour, minute, 0, DateTimeKind.Utc);
	}

	public static DateTime SpecifyAsUtc(this DateTime time)
	{
		return DateTime.SpecifyKind(time, DateTimeKind.Utc);
	}

	public static string PrettyUtcOffset(this TimeSpan span)
	{
		return string.Format("UTC{0}{1:hh}:{1:mm}", (span < TimeSpan.Zero) ? "-" : "+", span);
	}

	public static bool SubtractedLessThan(this DateTime self, DateTime other, TimeSpan interval)
	{
		return Math.Abs((self - other.ToUniversalTime()).TotalMilliseconds) <= interval.TotalMilliseconds;
	}

	public static DateTime? SetUTCKindIfNotSpecified(this DateTime? value)
	{
		if (value.HasValue)
		{
			return (value.Value.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value.Value;
		}
		return null;
	}

	public static DateTime SetUTCKindIfNotSpecified(this DateTime value)
	{
		return (value.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value;
	}

	public static DateTime OrLater(this DateTime current, DateTime other)
	{
		return (current >= other) ? current : other;
	}

	public static DateTime OrEarlier(this DateTime current, DateTime other)
	{
		return (current <= other) ? current : other;
	}

	public static DateTime Min(DateTime x, DateTime y)
	{
		return (x.ToUniversalTime() < y.ToUniversalTime()) ? x : y;
	}

	public static DateTime Max(DateTime x, DateTime y)
	{
		return (x.ToUniversalTime() > y.ToUniversalTime()) ? x : y;
	}
}
