using System.Data.Common;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents the date and time data ranging in value from January 1, 1753 to December 31, 9999 to an accuracy of 3.33 milliseconds to be stored in or retrieved from a database. The <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure has a different underlying data structure from its corresponding .NET Framework type, <see cref="T:System.DateTime" />, which can represent any time between 12:00:00 AM 1/1/0001 and 11:59:59 PM 12/31/9999, to the accuracy of 100 nanoseconds. <see cref="T:System.Data.SqlTypes.SqlDateTime" /> actually stores the relative difference to 00:00:00 AM 1/1/1900. Therefore, a conversion from "00:00:00 AM 1/1/1900" to an integer will return 0.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlDateTime : INullable, IComparable, IXmlSerializable
{
	private bool m_fNotNull;

	private int m_day;

	private int m_time;

	private static readonly double s_SQLTicksPerMillisecond = 0.3;

	/// <summary>A constant whose value is the number of ticks equivalent to one second.</summary>
	public static readonly int SQLTicksPerSecond = 300;

	/// <summary>A constant whose value is the number of ticks equivalent to one minute.</summary>
	public static readonly int SQLTicksPerMinute = SQLTicksPerSecond * 60;

	/// <summary>A constant whose value is the number of ticks equivalent to one hour.</summary>
	public static readonly int SQLTicksPerHour = SQLTicksPerMinute * 60;

	private static readonly int s_SQLTicksPerDay = SQLTicksPerHour * 24;

	private static readonly long s_ticksPerSecond = 10000000L;

	private static readonly DateTime s_SQLBaseDate = new DateTime(1900, 1, 1);

	private static readonly long s_SQLBaseDateTicks = s_SQLBaseDate.Ticks;

	private static readonly int s_minYear = 1753;

	private static readonly int s_maxYear = 9999;

	private static readonly int s_minDay = -53690;

	private static readonly int s_maxDay = 2958463;

	private static readonly int s_minTime = 0;

	private static readonly int s_maxTime = s_SQLTicksPerDay - 1;

	private static readonly int s_dayBase = 693595;

	private static readonly int[] s_daysToMonth365 = new int[13]
	{
		0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
		304, 334, 365
	};

	private static readonly int[] s_daysToMonth366 = new int[13]
	{
		0, 31, 60, 91, 121, 152, 182, 213, 244, 274,
		305, 335, 366
	};

	private static readonly DateTime s_minDateTime = new DateTime(1753, 1, 1);

	private static readonly DateTime s_maxDateTime = DateTime.MaxValue;

	private static readonly TimeSpan s_minTimeSpan = s_minDateTime.Subtract(s_SQLBaseDate);

	private static readonly TimeSpan s_maxTimeSpan = s_maxDateTime.Subtract(s_SQLBaseDate);

	private static readonly string s_ISO8601_DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";

	private static readonly string[] s_dateTimeFormats = new string[8] { "MMM d yyyy hh:mm:ss:ffftt", "MMM d yyyy hh:mm:ss:fff", "d MMM yyyy hh:mm:ss:ffftt", "d MMM yyyy hh:mm:ss:fff", "hh:mm:ss:ffftt", "hh:mm:ss:fff", "yyMMdd", "yyyyMMdd" };

	private const DateTimeStyles x_DateTimeStyle = DateTimeStyles.AllowWhiteSpaces;

	/// <summary>Represents the minimum valid date value for a <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	public static readonly SqlDateTime MinValue = new SqlDateTime(s_minDay, 0);

	/// <summary>Represents the maximum valid date value for a <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	public static readonly SqlDateTime MaxValue = new SqlDateTime(s_maxDay, s_maxTime);

	/// <summary>Represents a <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	public static readonly SqlDateTime Null = new SqlDateTime(fNull: true);

	/// <summary>Indicates whether this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure is null.</summary>
	/// <returns>true if null. Otherwise, false. </returns>
	public bool IsNull => !m_fNotNull;

	/// <summary>Gets the value of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. This property is read-only.</summary>
	/// <returns>The value of this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</returns>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The exception that is thrown when the Value property of a <see cref="N:System.Data.SqlTypes" /> structure is set to null.</exception>
	public DateTime Value
	{
		get
		{
			if (m_fNotNull)
			{
				return ToDateTime(this);
			}
			throw new SqlNullValueException();
		}
	}

	/// <summary>Gets the number of ticks representing the date of this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	/// <returns>The number of ticks representing the date that is contained in the <see cref="P:System.Data.SqlTypes.SqlDateTime.Value" /> property of this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</returns>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The exception that is thrown when the Value property of a <see cref="N:System.Data.SqlTypes" /> structure is set to null.</exception>
	public int DayTicks
	{
		get
		{
			if (m_fNotNull)
			{
				return m_day;
			}
			throw new SqlNullValueException();
		}
	}

	/// <summary>Gets the number of ticks representing the time of this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	/// <returns>The number of ticks representing the time of this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</returns>
	public int TimeTicks
	{
		get
		{
			if (m_fNotNull)
			{
				return m_time;
			}
			throw new SqlNullValueException();
		}
	}

	private SqlDateTime(bool fNull)
	{
		m_fNotNull = false;
		m_day = 0;
		m_time = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure using the specified <see cref="T:System.DateTime" /> value.</summary>
	/// <param name="value">A DateTime structure. </param>
	public SqlDateTime(DateTime value)
	{
		this = FromDateTime(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure using the supplied parameters to initialize the year, month, day.</summary>
	/// <param name="year">An integer representing the year of the of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="month">An integer value representing the month of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="day">An integer value representing the day number of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public SqlDateTime(int year, int month, int day)
		: this(year, month, day, 0, 0, 0, 0.0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure using the supplied parameters to initialize the year, month, day, hour, minute, and second of the new structure.</summary>
	/// <param name="year">An integer value representing the year of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="month">An integer value representing the month of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="day">An integer value representing the day of the month of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="hour">An integer value representing the hour of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="minute">An integer value representing the minute of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="second">An integer value representing the second of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public SqlDateTime(int year, int month, int day, int hour, int minute, int second)
		: this(year, month, day, hour, minute, second, 0.0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure using the supplied parameters to initialize the year, month, day, hour, minute, second, and millisecond of the new structure.</summary>
	/// <param name="year">An integer value representing the year of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="month">An integer value representing the month of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="day">An integer value representing the day of the month of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="hour">An integer value representing the hour of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="minute">An integer value representing the minute of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="second">An integer value representing the second of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="millisecond">An double value representing the millisecond of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public SqlDateTime(int year, int month, int day, int hour, int minute, int second, double millisecond)
	{
		if (year >= s_minYear && year <= s_maxYear && month >= 1 && month <= 12)
		{
			int[] array = (IsLeapYear(year) ? s_daysToMonth366 : s_daysToMonth365);
			if (day >= 1 && day <= array[month] - array[month - 1])
			{
				int num = year - 1;
				int num2 = num * 365 + num / 4 - num / 100 + num / 400 + array[month - 1] + day - 1;
				num2 -= s_dayBase;
				if (num2 >= s_minDay && num2 <= s_maxDay && hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60 && millisecond >= 0.0 && millisecond < 1000.0)
				{
					double num3 = millisecond * s_SQLTicksPerMillisecond + 0.5;
					int num4 = hour * SQLTicksPerHour + minute * SQLTicksPerMinute + second * SQLTicksPerSecond + (int)num3;
					if (num4 > s_maxTime)
					{
						num4 = 0;
						num2++;
					}
					this = new SqlDateTime(num2, num4);
					return;
				}
			}
		}
		throw new SqlTypeException(SQLResource.InvalidDateTimeMessage);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure using the supplied parameters to initialize the year, month, day, hour, minute, second, and billisecond of the new structure.</summary>
	/// <param name="year">An integer value representing the year of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="month">An integer value representing the month of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="day">An integer value representing the day of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="hour">An integer value representing the hour of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="minute">An integer value representing the minute of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="second">An integer value representing the second of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="bilisecond">An integer value representing the bilisecond (billionth of a second) of the new <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public SqlDateTime(int year, int month, int day, int hour, int minute, int second, int bilisecond)
		: this(year, month, day, hour, minute, second, (double)bilisecond / 1000.0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure using the supplied parameters.</summary>
	/// <param name="dayTicks">An integer value that represents the date as ticks. </param>
	/// <param name="timeTicks">An integer value that represents the time as ticks. </param>
	public SqlDateTime(int dayTicks, int timeTicks)
	{
		if (dayTicks < s_minDay || dayTicks > s_maxDay || timeTicks < s_minTime || timeTicks > s_maxTime)
		{
			m_fNotNull = false;
			throw new OverflowException(SQLResource.DateTimeOverflowMessage);
		}
		m_day = dayTicks;
		m_time = timeTicks;
		m_fNotNull = true;
	}

	internal SqlDateTime(double dblVal)
	{
		if (dblVal < (double)s_minDay || dblVal >= (double)(s_maxDay + 1))
		{
			throw new OverflowException(SQLResource.DateTimeOverflowMessage);
		}
		int num = (int)dblVal;
		int num2 = (int)((dblVal - (double)num) * (double)s_SQLTicksPerDay);
		if (num2 < 0)
		{
			num--;
			num2 += s_SQLTicksPerDay;
		}
		else if (num2 >= s_SQLTicksPerDay)
		{
			num++;
			num2 -= s_SQLTicksPerDay;
		}
		this = new SqlDateTime(num, num2);
	}

	private static TimeSpan ToTimeSpan(SqlDateTime value)
	{
		long num = (long)((double)value.m_time / s_SQLTicksPerMillisecond + 0.5);
		return new TimeSpan(value.m_day * 864000000000L + num * 10000);
	}

	private static DateTime ToDateTime(SqlDateTime value)
	{
		return s_SQLBaseDate.Add(ToTimeSpan(value));
	}

	internal static DateTime ToDateTime(int daypart, int timepart)
	{
		if (daypart < s_minDay || daypart > s_maxDay || timepart < s_minTime || timepart > s_maxTime)
		{
			throw new OverflowException(SQLResource.DateTimeOverflowMessage);
		}
		long num = daypart * 864000000000L;
		long num2 = (long)((double)timepart / s_SQLTicksPerMillisecond + 0.5) * 10000;
		return new DateTime(s_SQLBaseDateTicks + num + num2);
	}

	private static SqlDateTime FromTimeSpan(TimeSpan value)
	{
		if (value < s_minTimeSpan || value > s_maxTimeSpan)
		{
			throw new SqlTypeException(SQLResource.DateTimeOverflowMessage);
		}
		int num = value.Days;
		long num2 = value.Ticks - num * 864000000000L;
		if (num2 < 0)
		{
			num--;
			num2 += 864000000000L;
		}
		int num3 = (int)((double)num2 / 10000.0 * s_SQLTicksPerMillisecond + 0.5);
		if (num3 > s_maxTime)
		{
			num3 = 0;
			num++;
		}
		return new SqlDateTime(num, num3);
	}

	private static SqlDateTime FromDateTime(DateTime value)
	{
		if (value == DateTime.MaxValue)
		{
			return MaxValue;
		}
		return FromTimeSpan(value.Subtract(s_SQLBaseDate));
	}

	/// <summary>Converts a <see cref="T:System.DateTime" /> structure to a <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure whose <see cref="P:System.Data.SqlTypes.SqlDateTime.Value" /> is equal to the combined <see cref="P:System.DateTime.Date" /> and <see cref="P:System.DateTime.TimeOfDay" /> properties of the supplied <see cref="T:System.DateTime" /> structure.</returns>
	/// <param name="value">A DateTime structure. </param>
	public static implicit operator SqlDateTime(DateTime value)
	{
		return new SqlDateTime(value);
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure to a <see cref="T:System.DateTime" /> structure.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object whose <see cref="P:System.DateTime.Date" /> and <see cref="P:System.DateTime.TimeOfDay" /> properties contain the same date and time values as the <see cref="P:System.Data.SqlTypes.SqlDateTime.Value" /> property of the supplied <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</param>
	public static explicit operator DateTime(SqlDateTime x)
	{
		return ToDateTime(x);
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure to a <see cref="T:System.String" />.</summary>
	/// <returns>A String representing the <see cref="P:System.Data.SqlTypes.SqlDateTime.Value" /> property of this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		if (IsNull)
		{
			return SQLResource.NullString;
		}
		return ToDateTime(this).ToString((IFormatProvider)null);
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> representation of a date and time to its <see cref="T:System.Data.SqlTypes.SqlDateTime" /> equivalent.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure equal to the date and time represented by the specified string.</returns>
	/// <param name="s">The string to be parsed. </param>
	public static SqlDateTime Parse(string s)
	{
		if (s == SQLResource.NullString)
		{
			return Null;
		}
		DateTime value;
		try
		{
			value = DateTime.Parse(s, CultureInfo.InvariantCulture);
		}
		catch (FormatException)
		{
			DateTimeFormatInfo provider = (DateTimeFormatInfo)CultureInfo.CurrentCulture.GetFormat(typeof(DateTimeFormatInfo));
			value = DateTime.ParseExact(s, s_dateTimeFormats, provider, DateTimeStyles.AllowWhiteSpaces);
		}
		return new SqlDateTime(value);
	}

	/// <summary>Adds the period of time indicated by the supplied <see cref="T:System.TimeSpan" /> parameter, <paramref name="t" />, to the supplied <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDateTime" />. If either argument is <see cref="F:System.Data.SqlTypes.SqlDateTime.Null" />, the new <see cref="P:System.Data.SqlTypes.SqlDateTime.Value" /> is <see cref="F:System.Data.SqlTypes.SqlDateTime.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="t">A <see cref="T:System.TimeSpan" /> structure. </param>
	public static SqlDateTime operator +(SqlDateTime x, TimeSpan t)
	{
		if (!x.IsNull)
		{
			return FromDateTime(ToDateTime(x) + t);
		}
		return Null;
	}

	/// <summary>Subtracts the supplied <see cref="T:System.TimeSpan" /> structure, <paramref name="t" />, from the supplied <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure representing the results of the subtraction.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="t">A <see cref="T:System.TimeSpan" /> structure. </param>
	public static SqlDateTime operator -(SqlDateTime x, TimeSpan t)
	{
		if (!x.IsNull)
		{
			return FromDateTime(ToDateTime(x) - t);
		}
		return Null;
	}

	/// <summary>Adds a <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to the specified TimeSpan.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> value.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> value.</param>
	/// <param name="t">A Timespan value.</param>
	public static SqlDateTime Add(SqlDateTime x, TimeSpan t)
	{
		return x + t;
	}

	/// <summary>Subtracts the specified Timespan from this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> instance.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> value.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> value.</param>
	/// <param name="t">A Timespan value.</param>
	public static SqlDateTime Subtract(SqlDateTime x, TimeSpan t)
	{
		return x - t;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlString" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlDateTime" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure whose <see cref="P:System.Data.SqlTypes.SqlDateTime.Value" /> is equal to the date and time represented by the <see cref="T:System.Data.SqlTypes.SqlString" /> parameter. If the <see cref="T:System.Data.SqlTypes.SqlString" /> is null, the Value of the newly created <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure will be null.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlString" />.</param>
	public static explicit operator SqlDateTime(SqlString x)
	{
		if (!x.IsNull)
		{
			return Parse(x.Value);
		}
		return Null;
	}

	private static bool IsLeapYear(int year)
	{
		if (year % 4 == 0)
		{
			if (year % 100 == 0)
			{
				return year % 400 == 0;
			}
			return true;
		}
		return false;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structures to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean operator ==(SqlDateTime x, SqlDateTime y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_day == y.m_day && x.m_time == y.m_time);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean operator !=(SqlDateTime x, SqlDateTime y)
	{
		return !(x == y);
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean operator <(SqlDateTime x, SqlDateTime y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_day < y.m_day || (x.m_day == y.m_day && x.m_time < y.m_time));
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean operator >(SqlDateTime x, SqlDateTime y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_day > y.m_day || (x.m_day == y.m_day && x.m_time > y.m_time));
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean operator <=(SqlDateTime x, SqlDateTime y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_day < y.m_day || (x.m_day == y.m_day && x.m_time <= y.m_time));
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean operator >=(SqlDateTime x, SqlDateTime y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_day > y.m_day || (x.m_day == y.m_day && x.m_time >= y.m_time));
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structures to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean Equals(SqlDateTime x, SqlDateTime y)
	{
		return x == y;
	}

	/// <summary>Performs a logical comparison of two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean NotEquals(SqlDateTime x, SqlDateTime y)
	{
		return x != y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean LessThan(SqlDateTime x, SqlDateTime y)
	{
		return x < y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean GreaterThan(SqlDateTime x, SqlDateTime y)
	{
		return x > y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean LessThanOrEqual(SqlDateTime x, SqlDateTime y)
	{
		return x <= y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure. </param>
	public static SqlBoolean GreaterThanOrEqual(SqlDateTime x, SqlDateTime y)
	{
		return x >= y;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure to <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>A SqlString structure whose value is a string representing the date and time that is contained in this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SqlString ToSqlString()
	{
		return (SqlString)this;
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure to the supplied <see cref="T:System.Object" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing as Visual Basic). </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be compared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlDateTime value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlDateTime));
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure to the supplied <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than <see cref="T:System.Data.SqlTypes.SqlDateTime" />. Zero This instance is the same as <see cref="T:System.Data.SqlTypes.SqlDateTime" />. Greater than zero This instance is greater than <see cref="T:System.Data.SqlTypes.SqlDateTime" />-or- <see cref="T:System.Data.SqlTypes.SqlDateTime" /> is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlDateTime" /> structure to be compared.</param>
	public int CompareTo(SqlDateTime value)
	{
		if (IsNull)
		{
			if (!value.IsNull)
			{
				return -1;
			}
			return 0;
		}
		if (value.IsNull)
		{
			return 1;
		}
		if (this < value)
		{
			return -1;
		}
		if (this > value)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>Compares the supplied object parameter to the <see cref="P:System.Data.SqlTypes.SqlDateTime.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlDateTime" /> object.</summary>
	/// <returns>true if the object is an instance of <see cref="T:System.Data.SqlTypes.SqlDateTime" /> and the two are equal; otherwise false.</returns>
	/// <param name="value">The object to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlDateTime sqlDateTime))
		{
			return false;
		}
		if (sqlDateTime.IsNull || IsNull)
		{
			if (sqlDateTime.IsNull)
			{
				return IsNull;
			}
			return false;
		}
		return (this == sqlDateTime).Value;
	}

	/// <summary>Gets the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		if (!IsNull)
		{
			return Value.GetHashCode();
		}
		return 0;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An XmlSchema.</returns>
	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="reader">XmlReader </param>
	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		string attribute = reader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
		if (attribute != null && XmlConvert.ToBoolean(attribute))
		{
			reader.ReadElementString();
			m_fNotNull = false;
			return;
		}
		DateTime value = XmlConvert.ToDateTime(reader.ReadElementString(), XmlDateTimeSerializationMode.RoundtripKind);
		if (value.Kind != DateTimeKind.Unspecified)
		{
			throw new SqlTypeException(SQLResource.TimeZoneSpecifiedMessage);
		}
		SqlDateTime sqlDateTime = FromDateTime(value);
		m_day = sqlDateTime.DayTicks;
		m_time = sqlDateTime.TimeTicks;
		m_fNotNull = true;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="writer">XmlWriter </param>
	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		if (IsNull)
		{
			writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
		}
		else
		{
			writer.WriteString(XmlConvert.ToString(Value, s_ISO8601_DateTimeFormat));
		}
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>A string value that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">A <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("dateTime", "http://www.w3.org/2001/XMLSchema");
	}
}
