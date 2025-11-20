using System.Globalization;
using System.Threading;

namespace System;

/// <summary>Represents a time zone.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[Obsolete("System.TimeZone has been deprecated.  Please investigate the use of System.TimeZoneInfo instead.")]
public abstract class TimeZone
{
	private static volatile TimeZone currentTimeZone;

	private static object s_InternalSyncObject;

	private static object InternalSyncObject
	{
		get
		{
			if (s_InternalSyncObject == null)
			{
				object value = new object();
				Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
			}
			return s_InternalSyncObject;
		}
	}

	/// <summary>Gets the time zone of the current computer.</summary>
	/// <returns>A <see cref="T:System.TimeZone" /> object that represents the current local time zone.</returns>
	/// <filterpriority>1</filterpriority>
	public static TimeZone CurrentTimeZone
	{
		get
		{
			TimeZone timeZone = currentTimeZone;
			if (timeZone == null)
			{
				lock (InternalSyncObject)
				{
					if (currentTimeZone == null)
					{
						currentTimeZone = new CurrentSystemTimeZone();
					}
					timeZone = currentTimeZone;
				}
			}
			return timeZone;
		}
	}

	/// <summary>Gets the standard time zone name.</summary>
	/// <returns>The standard time zone name.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set this property to null. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract string StandardName { get; }

	/// <summary>Gets the daylight saving time zone name.</summary>
	/// <returns>The daylight saving time zone name.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string DaylightName { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.TimeZone" /> class.</summary>
	protected TimeZone()
	{
	}

	internal static void ResetTimeZone()
	{
		if (currentTimeZone != null)
		{
			lock (InternalSyncObject)
			{
				currentTimeZone = null;
			}
		}
	}

	/// <summary>Returns the Coordinated Universal Time (UTC) offset for the specified local time.</summary>
	/// <returns>The Coordinated Universal Time (UTC) offset from <paramref name="time" />.</returns>
	/// <param name="time">A date and time value.</param>
	/// <filterpriority>2</filterpriority>
	public abstract TimeSpan GetUtcOffset(DateTime time);

	/// <summary>Returns the Coordinated Universal Time (UTC) that corresponds to a specified time.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object whose value is the Coordinated Universal Time (UTC) that corresponds to <paramref name="time" />.</returns>
	/// <param name="time">A date and time. </param>
	/// <filterpriority>2</filterpriority>
	public virtual DateTime ToUniversalTime(DateTime time)
	{
		if (time.Kind == DateTimeKind.Utc)
		{
			return time;
		}
		long num = time.Ticks - GetUtcOffset(time).Ticks;
		if (num > 3155378975999999999L)
		{
			return new DateTime(3155378975999999999L, DateTimeKind.Utc);
		}
		if (num < 0)
		{
			return new DateTime(0L, DateTimeKind.Utc);
		}
		return new DateTime(num, DateTimeKind.Utc);
	}

	/// <summary>Returns the local time that corresponds to a specified date and time value.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object whose value is the local time that corresponds to <paramref name="time" />.</returns>
	/// <param name="time">A Coordinated Universal Time (UTC) time. </param>
	/// <filterpriority>2</filterpriority>
	public virtual DateTime ToLocalTime(DateTime time)
	{
		if (time.Kind == DateTimeKind.Local)
		{
			return time;
		}
		bool isAmbiguousLocalDst = false;
		long utcOffsetFromUniversalTime = ((CurrentSystemTimeZone)CurrentTimeZone).GetUtcOffsetFromUniversalTime(time, ref isAmbiguousLocalDst);
		return new DateTime(time.Ticks + utcOffsetFromUniversalTime, DateTimeKind.Local, isAmbiguousLocalDst);
	}

	/// <summary>Returns the daylight saving time period for a particular year.</summary>
	/// <returns>A <see cref="T:System.Globalization.DaylightTime" /> object that contains the start and end date for daylight saving time in <paramref name="year" />.</returns>
	/// <param name="year">The year that the daylight saving time period applies to. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract DaylightTime GetDaylightChanges(int year);

	/// <summary>Returns a value indicating whether the specified date and time is within a daylight saving time period.</summary>
	/// <returns>true if <paramref name="time" /> is in a daylight saving time period; otherwise, false.</returns>
	/// <param name="time">A date and time. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsDaylightSavingTime(DateTime time)
	{
		return IsDaylightSavingTime(time, GetDaylightChanges(time.Year));
	}

	/// <summary>Returns a value indicating whether the specified date and time is within the specified daylight saving time period.</summary>
	/// <returns>true if <paramref name="time" /> is in <paramref name="daylightTimes" />; otherwise, false.</returns>
	/// <param name="time">A date and time. </param>
	/// <param name="daylightTimes">A daylight saving time period. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="daylightTimes" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDaylightSavingTime(DateTime time, DaylightTime daylightTimes)
	{
		return CalculateUtcOffset(time, daylightTimes) != TimeSpan.Zero;
	}

	internal static TimeSpan CalculateUtcOffset(DateTime time, DaylightTime daylightTimes)
	{
		if (daylightTimes == null)
		{
			return TimeSpan.Zero;
		}
		if (time.Kind == DateTimeKind.Utc)
		{
			return TimeSpan.Zero;
		}
		DateTime dateTime = daylightTimes.Start + daylightTimes.Delta;
		DateTime end = daylightTimes.End;
		DateTime dateTime2;
		DateTime dateTime3;
		if (daylightTimes.Delta.Ticks > 0)
		{
			dateTime2 = end - daylightTimes.Delta;
			dateTime3 = end;
		}
		else
		{
			dateTime2 = dateTime;
			dateTime3 = dateTime - daylightTimes.Delta;
		}
		bool flag = false;
		if (dateTime > end)
		{
			if (time >= dateTime || time < end)
			{
				flag = true;
			}
		}
		else if (time >= dateTime && time < end)
		{
			flag = true;
		}
		if (flag && time >= dateTime2 && time < dateTime3)
		{
			flag = time.IsAmbiguousDaylightSavingTime();
		}
		if (flag)
		{
			return daylightTimes.Delta;
		}
		return TimeSpan.Zero;
	}

	internal static void ClearCachedData()
	{
		currentTimeZone = null;
	}
}
