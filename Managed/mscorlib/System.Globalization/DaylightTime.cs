namespace System.Globalization;

/// <summary>Defines the period of daylight saving time.</summary>
[Serializable]
public class DaylightTime
{
	private readonly DateTime _start;

	private readonly DateTime _end;

	private readonly TimeSpan _delta;

	/// <summary>Gets the object that represents the date and time when the daylight saving period begins.</summary>
	/// <returns>The object that represents the date and time when the daylight saving period begins. The value is in local time.</returns>
	public DateTime Start => _start;

	/// <summary>Gets the object that represents the date and time when the daylight saving period ends.</summary>
	/// <returns>The object that represents the date and time when the daylight saving period ends. The value is in local time.</returns>
	public DateTime End => _end;

	/// <summary>Gets the time interval that represents the difference between standard time and daylight saving time.</summary>
	/// <returns>The time interval that represents the difference between standard time and daylight saving time.</returns>
	public TimeSpan Delta => _delta;

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.DaylightTime" /> class with the specified start, end, and time difference information.</summary>
	/// <param name="start">The object that represents the date and time when daylight saving time begins. The value must be in local time. </param>
	/// <param name="end">The object that represents the date and time when daylight saving time ends. The value must be in local time. </param>
	/// <param name="delta">The object that represents the difference between standard time and daylight saving time, in ticks. </param>
	public DaylightTime(DateTime start, DateTime end, TimeSpan delta)
	{
		_start = start;
		_end = end;
		_delta = delta;
	}
}
