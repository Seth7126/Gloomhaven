using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Unity;

namespace System;

/// <summary>Represents any time zone in the world.</summary>
[Serializable]
[TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
public sealed class TimeZoneInfo : IEquatable<TimeZoneInfo>, ISerializable, IDeserializationCallback
{
	private sealed class CachedData
	{
		private volatile OffsetAndRule _oneYearLocalFromUtc;

		private volatile TimeZoneInfo _localTimeZone;

		public Dictionary<string, TimeZoneInfo> _systemTimeZones;

		public ReadOnlyCollection<TimeZoneInfo> _readOnlySystemTimeZones;

		public bool _allSystemTimeZonesRead;

		public TimeZoneInfo Local
		{
			get
			{
				TimeZoneInfo timeZoneInfo = _localTimeZone;
				if (timeZoneInfo == null)
				{
					timeZoneInfo = CreateLocal();
				}
				return timeZoneInfo;
			}
		}

		private static TimeZoneInfo GetCurrentOneYearLocal()
		{
			if (Interop.Kernel32.GetTimeZoneInformation(out var lpTimeZoneInformation) != uint.MaxValue)
			{
				return GetLocalTimeZoneFromWin32Data(in lpTimeZoneInformation, dstDisabled: false);
			}
			return CreateCustomTimeZone("Local", TimeSpan.Zero, "Local", "Local");
		}

		public OffsetAndRule GetOneYearLocalFromUtc(int year)
		{
			OffsetAndRule offsetAndRule = _oneYearLocalFromUtc;
			if (offsetAndRule == null || offsetAndRule.Year != year)
			{
				TimeZoneInfo currentOneYearLocal = GetCurrentOneYearLocal();
				AdjustmentRule rule = ((currentOneYearLocal._adjustmentRules == null) ? null : currentOneYearLocal._adjustmentRules[0]);
				offsetAndRule = (_oneYearLocalFromUtc = new OffsetAndRule(year, currentOneYearLocal.BaseUtcOffset, rule));
			}
			return offsetAndRule;
		}

		private TimeZoneInfo CreateLocal()
		{
			lock (this)
			{
				TimeZoneInfo timeZoneInfo = _localTimeZone;
				if (timeZoneInfo == null)
				{
					timeZoneInfo = GetLocalTimeZone(this);
					timeZoneInfo = (_localTimeZone = new TimeZoneInfo(timeZoneInfo._id, timeZoneInfo._baseUtcOffset, timeZoneInfo._displayName, timeZoneInfo._standardDisplayName, timeZoneInfo._daylightDisplayName, timeZoneInfo._adjustmentRules, disableDaylightSavingTime: false));
				}
				return timeZoneInfo;
			}
		}

		public DateTimeKind GetCorrespondingKind(TimeZoneInfo timeZone)
		{
			if (timeZone != s_utcTimeZone)
			{
				if (timeZone != _localTimeZone)
				{
					return DateTimeKind.Unspecified;
				}
				return DateTimeKind.Local;
			}
			return DateTimeKind.Utc;
		}
	}

	private sealed class OffsetAndRule
	{
		public readonly int Year;

		public readonly TimeSpan Offset;

		public readonly AdjustmentRule Rule;

		public OffsetAndRule(int year, TimeSpan offset, AdjustmentRule rule)
		{
			Year = year;
			Offset = offset;
			Rule = rule;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct DYNAMIC_TIME_ZONE_INFORMATION
	{
		internal Interop.Kernel32.TIME_ZONE_INFORMATION TZI;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		internal string TimeZoneKeyName;

		internal byte DynamicDaylightTimeDisabled;
	}

	private enum TimeZoneInfoResult
	{
		Success,
		TimeZoneNotFoundException,
		InvalidTimeZoneException,
		SecurityException
	}

	/// <summary>Provides information about a time zone adjustment, such as the transition to and from daylight saving time.</summary>
	/// <filterpriority>2</filterpriority>
	[Serializable]
	public sealed class AdjustmentRule : IEquatable<AdjustmentRule>, ISerializable, IDeserializationCallback
	{
		private readonly DateTime _dateStart;

		private readonly DateTime _dateEnd;

		private readonly TimeSpan _daylightDelta;

		private readonly TransitionTime _daylightTransitionStart;

		private readonly TransitionTime _daylightTransitionEnd;

		private readonly TimeSpan _baseUtcOffsetDelta;

		private readonly bool _noDaylightTransitions;

		/// <summary>Gets the date when the adjustment rule takes effect.</summary>
		/// <returns>A <see cref="T:System.DateTime" /> value that indicates when the adjustment rule takes effect.</returns>
		public DateTime DateStart => _dateStart;

		/// <summary>Gets the date when the adjustment rule ceases to be in effect.</summary>
		/// <returns>A <see cref="T:System.DateTime" /> value that indicates the end date of the adjustment rule.</returns>
		public DateTime DateEnd => _dateEnd;

		/// <summary>Gets the amount of time that is required to form the time zone's daylight saving time. This amount of time is added to the time zone's offset from Coordinated Universal Time (UTC).</summary>
		/// <returns>A <see cref="T:System.TimeSpan" /> object that indicates the amount of time to add to the standard time changes as a result of the adjustment rule.</returns>
		public TimeSpan DaylightDelta => _daylightDelta;

		/// <summary>Gets information about the annual transition from standard time to daylight saving time.</summary>
		/// <returns>A <see cref="T:System.TimeZoneInfo.TransitionTime" /> object that defines the annual transition from a time zone's standard time to daylight saving time.</returns>
		public TransitionTime DaylightTransitionStart => _daylightTransitionStart;

		/// <summary>Gets information about the annual transition from daylight saving time back to standard time.</summary>
		/// <returns>A <see cref="T:System.TimeZoneInfo.TransitionTime" /> object that defines the annual transition from daylight saving time back to the time zone's standard time.</returns>
		public TransitionTime DaylightTransitionEnd => _daylightTransitionEnd;

		internal TimeSpan BaseUtcOffsetDelta => _baseUtcOffsetDelta;

		internal bool NoDaylightTransitions => _noDaylightTransitions;

		internal bool HasDaylightSaving
		{
			get
			{
				if (!(DaylightDelta != TimeSpan.Zero) && (!(DaylightTransitionStart != default(TransitionTime)) || !(DaylightTransitionStart.TimeOfDay != DateTime.MinValue)))
				{
					if (DaylightTransitionEnd != default(TransitionTime))
					{
						return DaylightTransitionEnd.TimeOfDay != DateTime.MinValue.AddMilliseconds(1.0);
					}
					return false;
				}
				return true;
			}
		}

		/// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> object is equal to a second <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> object.</summary>
		/// <returns>true if both <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> objects have equal values; otherwise, false.</returns>
		/// <param name="other">The object to compare with the current object.</param>
		/// <filterpriority>2</filterpriority>
		public bool Equals(AdjustmentRule other)
		{
			if (other != null && _dateStart == other._dateStart && _dateEnd == other._dateEnd && _daylightDelta == other._daylightDelta && _baseUtcOffsetDelta == other._baseUtcOffsetDelta && _daylightTransitionEnd.Equals(other._daylightTransitionEnd))
			{
				return _daylightTransitionStart.Equals(other._daylightTransitionStart);
			}
			return false;
		}

		/// <summary>Serves as a hash function for hashing algorithms and data structures such as hash tables.</summary>
		/// <returns>A 32-bit signed integer that serves as the hash code for the current <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> object.</returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			return _dateStart.GetHashCode();
		}

		private AdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TransitionTime daylightTransitionStart, TransitionTime daylightTransitionEnd, TimeSpan baseUtcOffsetDelta, bool noDaylightTransitions)
		{
			ValidateAdjustmentRule(dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd, noDaylightTransitions);
			_dateStart = dateStart;
			_dateEnd = dateEnd;
			_daylightDelta = daylightDelta;
			_daylightTransitionStart = daylightTransitionStart;
			_daylightTransitionEnd = daylightTransitionEnd;
			_baseUtcOffsetDelta = baseUtcOffsetDelta;
			_noDaylightTransitions = noDaylightTransitions;
		}

		/// <summary>Creates a new adjustment rule for a particular time zone.</summary>
		/// <returns>An object that represents the new adjustment rule.</returns>
		/// <param name="dateStart">The effective date of the adjustment rule. If the value of the <paramref name="dateStart" /> parameter is DateTime.MinValue.Date, this is the first adjustment rule in effect for a time zone.   </param>
		/// <param name="dateEnd">The last date that the adjustment rule is in force. If the value of the <paramref name="dateEnd" /> parameter is DateTime.MaxValue.Date, the adjustment rule has no end date.</param>
		/// <param name="daylightDelta">The time change that results from the adjustment. This value is added to the time zone's <see cref="P:System.TimeZoneInfo.BaseUtcOffset" /> property to obtain the correct daylight offset from Coordinated Universal Time (UTC). This value can range from -14 to 14. </param>
		/// <param name="daylightTransitionStart">An object that defines the start of daylight saving time.</param>
		/// <param name="daylightTransitionEnd">An object that defines the end of daylight saving time.   </param>
		/// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateStart" /> or <paramref name="dateEnd" /> parameter does not equal <see cref="F:System.DateTimeKind.Unspecified" />.-or-The <paramref name="daylightTransitionStart" /> parameter is equal to the <paramref name="daylightTransitionEnd" /> parameter.-or-The <paramref name="dateStart" /> or <paramref name="dateEnd" /> parameter includes a time of day value.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="dateEnd" /> is earlier than <paramref name="dateStart" />.-or-<paramref name="daylightDelta" /> is less than -14 or greater than 14.-or-The <see cref="P:System.TimeSpan.Milliseconds" /> property of the <paramref name="daylightDelta" /> parameter is not equal to 0.-or-The <see cref="P:System.TimeSpan.Ticks" /> property of the <paramref name="daylightDelta" /> parameter does not equal a whole number of seconds.</exception>
		public static AdjustmentRule CreateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TransitionTime daylightTransitionStart, TransitionTime daylightTransitionEnd)
		{
			return new AdjustmentRule(dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd, TimeSpan.Zero, noDaylightTransitions: false);
		}

		internal static AdjustmentRule CreateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TransitionTime daylightTransitionStart, TransitionTime daylightTransitionEnd, TimeSpan baseUtcOffsetDelta, bool noDaylightTransitions)
		{
			return new AdjustmentRule(dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd, baseUtcOffsetDelta, noDaylightTransitions);
		}

		internal bool IsStartDateMarkerForBeginningOfYear()
		{
			if (!NoDaylightTransitions && DaylightTransitionStart.Month == 1 && DaylightTransitionStart.Day == 1 && DaylightTransitionStart.TimeOfDay.Hour == 0 && DaylightTransitionStart.TimeOfDay.Minute == 0 && DaylightTransitionStart.TimeOfDay.Second == 0)
			{
				return _dateStart.Year == _dateEnd.Year;
			}
			return false;
		}

		internal bool IsEndDateMarkerForEndOfYear()
		{
			if (!NoDaylightTransitions && DaylightTransitionEnd.Month == 1 && DaylightTransitionEnd.Day == 1 && DaylightTransitionEnd.TimeOfDay.Hour == 0 && DaylightTransitionEnd.TimeOfDay.Minute == 0 && DaylightTransitionEnd.TimeOfDay.Second == 0)
			{
				return _dateStart.Year == _dateEnd.Year;
			}
			return false;
		}

		private static void ValidateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TransitionTime daylightTransitionStart, TransitionTime daylightTransitionEnd, bool noDaylightTransitions)
		{
			if (dateStart.Kind != DateTimeKind.Unspecified && dateStart.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException("The supplied DateTime must have the Kind property set to DateTimeKind.Unspecified or DateTimeKind.Utc.", "dateStart");
			}
			if (dateEnd.Kind != DateTimeKind.Unspecified && dateEnd.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException("The supplied DateTime must have the Kind property set to DateTimeKind.Unspecified or DateTimeKind.Utc.", "dateEnd");
			}
			if (daylightTransitionStart.Equals(daylightTransitionEnd) && !noDaylightTransitions)
			{
				throw new ArgumentException("The DaylightTransitionStart property must not equal the DaylightTransitionEnd property.", "daylightTransitionEnd");
			}
			if (dateStart > dateEnd)
			{
				throw new ArgumentException("The DateStart property must come before the DateEnd property.", "dateStart");
			}
			if (daylightDelta.TotalHours < -23.0 || daylightDelta.TotalHours > 14.0)
			{
				throw new ArgumentOutOfRangeException("daylightDelta", daylightDelta, "The TimeSpan parameter must be within plus or minus 14.0 hours.");
			}
			if (daylightDelta.Ticks % 600000000 != 0L)
			{
				throw new ArgumentException("The TimeSpan parameter cannot be specified more precisely than whole minutes.", "daylightDelta");
			}
			if (dateStart != DateTime.MinValue && dateStart.Kind == DateTimeKind.Unspecified && dateStart.TimeOfDay != TimeSpan.Zero)
			{
				throw new ArgumentException("The supplied DateTime includes a TimeOfDay setting.   This is not supported.", "dateStart");
			}
			if (dateEnd != DateTime.MaxValue && dateEnd.Kind == DateTimeKind.Unspecified && dateEnd.TimeOfDay != TimeSpan.Zero)
			{
				throw new ArgumentException("The supplied DateTime includes a TimeOfDay setting.   This is not supported.", "dateEnd");
			}
		}

		/// <summary>Runs when the deserialization of a <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> object is completed.</summary>
		/// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.   </param>
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			try
			{
				ValidateAdjustmentRule(_dateStart, _dateEnd, _daylightDelta, _daylightTransitionStart, _daylightTransitionEnd, _noDaylightTransitions);
			}
			catch (ArgumentException innerException)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException);
			}
		}

		/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data that is required to serialize this object.</summary>
		/// <param name="info">The object to populate with data.</param>
		/// <param name="context">The destination for this serialization (see <see cref="T:System.Runtime.Serialization.StreamingContext" />).</param>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("DateStart", _dateStart);
			info.AddValue("DateEnd", _dateEnd);
			info.AddValue("DaylightDelta", _daylightDelta);
			info.AddValue("DaylightTransitionStart", _daylightTransitionStart);
			info.AddValue("DaylightTransitionEnd", _daylightTransitionEnd);
			info.AddValue("BaseUtcOffsetDelta", _baseUtcOffsetDelta);
			info.AddValue("NoDaylightTransitions", _noDaylightTransitions);
		}

		private AdjustmentRule(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			_dateStart = (DateTime)info.GetValue("DateStart", typeof(DateTime));
			_dateEnd = (DateTime)info.GetValue("DateEnd", typeof(DateTime));
			_daylightDelta = (TimeSpan)info.GetValue("DaylightDelta", typeof(TimeSpan));
			_daylightTransitionStart = (TransitionTime)info.GetValue("DaylightTransitionStart", typeof(TransitionTime));
			_daylightTransitionEnd = (TransitionTime)info.GetValue("DaylightTransitionEnd", typeof(TransitionTime));
			object valueNoThrow = info.GetValueNoThrow("BaseUtcOffsetDelta", typeof(TimeSpan));
			if (valueNoThrow != null)
			{
				_baseUtcOffsetDelta = (TimeSpan)valueNoThrow;
			}
			valueNoThrow = info.GetValueNoThrow("NoDaylightTransitions", typeof(bool));
			if (valueNoThrow != null)
			{
				_noDaylightTransitions = (bool)valueNoThrow;
			}
		}

		internal AdjustmentRule()
		{
			ThrowStub.ThrowNotSupportedException();
		}
	}

	private struct StringSerializer
	{
		private enum State
		{
			Escaped,
			NotEscaped,
			StartOfToken,
			EndOfLine
		}

		private readonly string _serializedText;

		private int _currentTokenStartIndex;

		private State _state;

		private const int InitialCapacityForString = 64;

		private const char Esc = '\\';

		private const char Sep = ';';

		private const char Lhs = '[';

		private const char Rhs = ']';

		private const string DateTimeFormat = "MM:dd:yyyy";

		private const string TimeOfDayFormat = "HH:mm:ss.FFF";

		public static string GetSerializedString(TimeZoneInfo zone)
		{
			StringBuilder stringBuilder = StringBuilderCache.Acquire();
			SerializeSubstitute(zone.Id, stringBuilder);
			stringBuilder.Append(';');
			stringBuilder.Append(zone.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append(';');
			SerializeSubstitute(zone.DisplayName, stringBuilder);
			stringBuilder.Append(';');
			SerializeSubstitute(zone.StandardName, stringBuilder);
			stringBuilder.Append(';');
			SerializeSubstitute(zone.DaylightName, stringBuilder);
			stringBuilder.Append(';');
			AdjustmentRule[] adjustmentRules = zone.GetAdjustmentRules();
			foreach (AdjustmentRule adjustmentRule in adjustmentRules)
			{
				stringBuilder.Append('[');
				stringBuilder.Append(adjustmentRule.DateStart.ToString("MM:dd:yyyy", DateTimeFormatInfo.InvariantInfo));
				stringBuilder.Append(';');
				stringBuilder.Append(adjustmentRule.DateEnd.ToString("MM:dd:yyyy", DateTimeFormatInfo.InvariantInfo));
				stringBuilder.Append(';');
				stringBuilder.Append(adjustmentRule.DaylightDelta.TotalMinutes.ToString(CultureInfo.InvariantCulture));
				stringBuilder.Append(';');
				SerializeTransitionTime(adjustmentRule.DaylightTransitionStart, stringBuilder);
				stringBuilder.Append(';');
				SerializeTransitionTime(adjustmentRule.DaylightTransitionEnd, stringBuilder);
				stringBuilder.Append(';');
				if (adjustmentRule.BaseUtcOffsetDelta != TimeSpan.Zero)
				{
					stringBuilder.Append(adjustmentRule.BaseUtcOffsetDelta.TotalMinutes.ToString(CultureInfo.InvariantCulture));
					stringBuilder.Append(';');
				}
				if (adjustmentRule.NoDaylightTransitions)
				{
					stringBuilder.Append('1');
					stringBuilder.Append(';');
				}
				stringBuilder.Append(']');
			}
			stringBuilder.Append(';');
			return StringBuilderCache.GetStringAndRelease(stringBuilder);
		}

		public static TimeZoneInfo GetDeserializedTimeZoneInfo(string source)
		{
			StringSerializer stringSerializer = new StringSerializer(source);
			string nextStringValue = stringSerializer.GetNextStringValue();
			TimeSpan nextTimeSpanValue = stringSerializer.GetNextTimeSpanValue();
			string nextStringValue2 = stringSerializer.GetNextStringValue();
			string nextStringValue3 = stringSerializer.GetNextStringValue();
			string nextStringValue4 = stringSerializer.GetNextStringValue();
			AdjustmentRule[] nextAdjustmentRuleArrayValue = stringSerializer.GetNextAdjustmentRuleArrayValue();
			try
			{
				return new TimeZoneInfo(nextStringValue, nextTimeSpanValue, nextStringValue2, nextStringValue3, nextStringValue4, nextAdjustmentRuleArrayValue, disableDaylightSavingTime: false);
			}
			catch (ArgumentException innerException)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException);
			}
			catch (InvalidTimeZoneException innerException2)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException2);
			}
		}

		private StringSerializer(string str)
		{
			_serializedText = str;
			_currentTokenStartIndex = 0;
			_state = State.StartOfToken;
		}

		private static void SerializeSubstitute(string text, StringBuilder serializedText)
		{
			foreach (char c in text)
			{
				if (c == '\\' || c == '[' || c == ']' || c == ';')
				{
					serializedText.Append('\\');
				}
				serializedText.Append(c);
			}
		}

		private static void SerializeTransitionTime(TransitionTime time, StringBuilder serializedText)
		{
			serializedText.Append('[');
			serializedText.Append(time.IsFixedDateRule ? '1' : '0');
			serializedText.Append(';');
			serializedText.Append(time.TimeOfDay.ToString("HH:mm:ss.FFF", DateTimeFormatInfo.InvariantInfo));
			serializedText.Append(';');
			serializedText.Append(time.Month.ToString(CultureInfo.InvariantCulture));
			serializedText.Append(';');
			if (time.IsFixedDateRule)
			{
				serializedText.Append(time.Day.ToString(CultureInfo.InvariantCulture));
				serializedText.Append(';');
			}
			else
			{
				serializedText.Append(time.Week.ToString(CultureInfo.InvariantCulture));
				serializedText.Append(';');
				serializedText.Append(((int)time.DayOfWeek).ToString(CultureInfo.InvariantCulture));
				serializedText.Append(';');
			}
			serializedText.Append(']');
		}

		private static void VerifyIsEscapableCharacter(char c)
		{
			if (c != '\\' && c != ';' && c != '[' && c != ']')
			{
				throw new SerializationException(SR.Format("The serialized data contained an invalid escape sequence '\\\\{0}'.", c));
			}
		}

		private void SkipVersionNextDataFields(int depth)
		{
			if (_currentTokenStartIndex < 0 || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			State state = State.NotEscaped;
			for (int i = _currentTokenStartIndex; i < _serializedText.Length; i++)
			{
				switch (state)
				{
				case State.Escaped:
					VerifyIsEscapableCharacter(_serializedText[i]);
					state = State.NotEscaped;
					break;
				case State.NotEscaped:
					switch (_serializedText[i])
					{
					case '\\':
						state = State.Escaped;
						break;
					case '[':
						depth++;
						break;
					case ']':
						depth--;
						if (depth == 0)
						{
							_currentTokenStartIndex = i + 1;
							if (_currentTokenStartIndex >= _serializedText.Length)
							{
								_state = State.EndOfLine;
							}
							else
							{
								_state = State.StartOfToken;
							}
							return;
						}
						break;
					case '\0':
						throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
					}
					break;
				}
			}
			throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
		}

		private string GetNextStringValue()
		{
			if (_state == State.EndOfLine)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_currentTokenStartIndex < 0 || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			State state = State.NotEscaped;
			StringBuilder stringBuilder = StringBuilderCache.Acquire(64);
			for (int i = _currentTokenStartIndex; i < _serializedText.Length; i++)
			{
				switch (state)
				{
				case State.Escaped:
					VerifyIsEscapableCharacter(_serializedText[i]);
					stringBuilder.Append(_serializedText[i]);
					state = State.NotEscaped;
					break;
				case State.NotEscaped:
					switch (_serializedText[i])
					{
					case '\\':
						state = State.Escaped;
						break;
					case '[':
						throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
					case ']':
						throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
					case ';':
						_currentTokenStartIndex = i + 1;
						if (_currentTokenStartIndex >= _serializedText.Length)
						{
							_state = State.EndOfLine;
						}
						else
						{
							_state = State.StartOfToken;
						}
						return StringBuilderCache.GetStringAndRelease(stringBuilder);
					case '\0':
						throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
					default:
						stringBuilder.Append(_serializedText[i]);
						break;
					}
					break;
				}
			}
			if (state == State.Escaped)
			{
				throw new SerializationException(SR.Format("The serialized data contained an invalid escape sequence '\\\\{0}'.", string.Empty));
			}
			throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
		}

		private DateTime GetNextDateTimeValue(string format)
		{
			if (!DateTime.TryParseExact(GetNextStringValue(), format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			return result;
		}

		private TimeSpan GetNextTimeSpanValue()
		{
			int nextInt32Value = GetNextInt32Value();
			try
			{
				return new TimeSpan(0, nextInt32Value, 0);
			}
			catch (ArgumentOutOfRangeException innerException)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException);
			}
		}

		private int GetNextInt32Value()
		{
			if (!int.TryParse(GetNextStringValue(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result))
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			return result;
		}

		private AdjustmentRule[] GetNextAdjustmentRuleArrayValue()
		{
			List<AdjustmentRule> list = new List<AdjustmentRule>(1);
			int num = 0;
			for (AdjustmentRule nextAdjustmentRuleValue = GetNextAdjustmentRuleValue(); nextAdjustmentRuleValue != null; nextAdjustmentRuleValue = GetNextAdjustmentRuleValue())
			{
				list.Add(nextAdjustmentRuleValue);
				num++;
			}
			if (_state == State.EndOfLine)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_currentTokenStartIndex < 0 || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (num == 0)
			{
				return null;
			}
			return list.ToArray();
		}

		private AdjustmentRule GetNextAdjustmentRuleValue()
		{
			if (_state == State.EndOfLine)
			{
				return null;
			}
			if (_currentTokenStartIndex < 0 || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_serializedText[_currentTokenStartIndex] == ';')
			{
				return null;
			}
			if (_serializedText[_currentTokenStartIndex] != '[')
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			_currentTokenStartIndex++;
			DateTime nextDateTimeValue = GetNextDateTimeValue("MM:dd:yyyy");
			DateTime nextDateTimeValue2 = GetNextDateTimeValue("MM:dd:yyyy");
			TimeSpan nextTimeSpanValue = GetNextTimeSpanValue();
			TransitionTime nextTransitionTimeValue = GetNextTransitionTimeValue();
			TransitionTime nextTransitionTimeValue2 = GetNextTransitionTimeValue();
			TimeSpan baseUtcOffsetDelta = TimeSpan.Zero;
			int num = 0;
			if (_state == State.EndOfLine || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if ((_serializedText[_currentTokenStartIndex] >= '0' && _serializedText[_currentTokenStartIndex] <= '9') || _serializedText[_currentTokenStartIndex] == '-' || _serializedText[_currentTokenStartIndex] == '+')
			{
				baseUtcOffsetDelta = GetNextTimeSpanValue();
			}
			if (_serializedText[_currentTokenStartIndex] >= '0' && _serializedText[_currentTokenStartIndex] <= '1')
			{
				num = GetNextInt32Value();
			}
			if (_state == State.EndOfLine || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_serializedText[_currentTokenStartIndex] != ']')
			{
				SkipVersionNextDataFields(1);
			}
			else
			{
				_currentTokenStartIndex++;
			}
			AdjustmentRule result;
			try
			{
				result = AdjustmentRule.CreateAdjustmentRule(nextDateTimeValue, nextDateTimeValue2, nextTimeSpanValue, nextTransitionTimeValue, nextTransitionTimeValue2, baseUtcOffsetDelta, num > 0);
			}
			catch (ArgumentException innerException)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException);
			}
			if (_currentTokenStartIndex >= _serializedText.Length)
			{
				_state = State.EndOfLine;
			}
			else
			{
				_state = State.StartOfToken;
			}
			return result;
		}

		private TransitionTime GetNextTransitionTimeValue()
		{
			if (_state == State.EndOfLine || (_currentTokenStartIndex < _serializedText.Length && _serializedText[_currentTokenStartIndex] == ']'))
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_currentTokenStartIndex < 0 || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_serializedText[_currentTokenStartIndex] != '[')
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			_currentTokenStartIndex++;
			int nextInt32Value = GetNextInt32Value();
			if (nextInt32Value != 0 && nextInt32Value != 1)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			DateTime nextDateTimeValue = GetNextDateTimeValue("HH:mm:ss.FFF");
			nextDateTimeValue = new DateTime(1, 1, 1, nextDateTimeValue.Hour, nextDateTimeValue.Minute, nextDateTimeValue.Second, nextDateTimeValue.Millisecond);
			int nextInt32Value2 = GetNextInt32Value();
			TransitionTime result;
			if (nextInt32Value == 1)
			{
				int nextInt32Value3 = GetNextInt32Value();
				try
				{
					result = TransitionTime.CreateFixedDateRule(nextDateTimeValue, nextInt32Value2, nextInt32Value3);
				}
				catch (ArgumentException innerException)
				{
					throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException);
				}
			}
			else
			{
				int nextInt32Value4 = GetNextInt32Value();
				int nextInt32Value5 = GetNextInt32Value();
				try
				{
					result = TransitionTime.CreateFloatingDateRule(nextDateTimeValue, nextInt32Value2, nextInt32Value4, (DayOfWeek)nextInt32Value5);
				}
				catch (ArgumentException innerException2)
				{
					throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException2);
				}
			}
			if (_state == State.EndOfLine || _currentTokenStartIndex >= _serializedText.Length)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_serializedText[_currentTokenStartIndex] != ']')
			{
				SkipVersionNextDataFields(1);
			}
			else
			{
				_currentTokenStartIndex++;
			}
			bool flag = false;
			if (_currentTokenStartIndex < _serializedText.Length && _serializedText[_currentTokenStartIndex] == ';')
			{
				_currentTokenStartIndex++;
				flag = true;
			}
			if (!flag)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.");
			}
			if (_currentTokenStartIndex >= _serializedText.Length)
			{
				_state = State.EndOfLine;
			}
			else
			{
				_state = State.StartOfToken;
			}
			return result;
		}
	}

	/// <summary>Provides information about a specific time change, such as the change from daylight saving time to standard time or vice versa, in a particular time zone.</summary>
	/// <filterpriority>2</filterpriority>
	[Serializable]
	public readonly struct TransitionTime : IEquatable<TransitionTime>, ISerializable, IDeserializationCallback
	{
		private readonly DateTime _timeOfDay;

		private readonly byte _month;

		private readonly byte _week;

		private readonly byte _day;

		private readonly DayOfWeek _dayOfWeek;

		private readonly bool _isFixedDateRule;

		/// <summary>Gets the hour, minute, and second at which the time change occurs.</summary>
		/// <returns>The time of day at which the time change occurs.</returns>
		public DateTime TimeOfDay => _timeOfDay;

		/// <summary>Gets the month in which the time change occurs.</summary>
		/// <returns>The month in which the time change occurs.</returns>
		public int Month => _month;

		/// <summary>Gets the week of the month in which a time change occurs.</summary>
		/// <returns>The week of the month in which the time change occurs.</returns>
		public int Week => _week;

		/// <summary>Gets the day on which the time change occurs.</summary>
		/// <returns>The day on which the time change occurs.</returns>
		public int Day => _day;

		/// <summary>Gets the day of the week on which the time change occurs.</summary>
		/// <returns>The day of the week on which the time change occurs.</returns>
		public DayOfWeek DayOfWeek => _dayOfWeek;

		/// <summary>Gets a value indicating whether the time change occurs at a fixed date and time (such as November 1) or a floating date and time (such as the last Sunday of October).</summary>
		/// <returns>true if the time change rule is fixed-date; false if the time change rule is floating-date.</returns>
		public bool IsFixedDateRule => _isFixedDateRule;

		/// <summary>Determines whether an object has identical values to the current <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.</summary>
		/// <returns>true if the two objects are equal; otherwise, false.</returns>
		/// <param name="obj">An object to compare with the current <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.   </param>
		/// <filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (obj is TransitionTime)
			{
				return Equals((TransitionTime)obj);
			}
			return false;
		}

		/// <summary>Determines whether two specified <see cref="T:System.TimeZoneInfo.TransitionTime" /> objects are equal.</summary>
		/// <returns>true if <paramref name="t1" /> and <paramref name="t2" /> have identical values; otherwise, false. </returns>
		/// <param name="t1">The first object to compare.</param>
		/// <param name="t2">The second object to compare.</param>
		public static bool operator ==(TransitionTime t1, TransitionTime t2)
		{
			return t1.Equals(t2);
		}

		/// <summary>Determines whether two specified <see cref="T:System.TimeZoneInfo.TransitionTime" /> objects are not equal.</summary>
		/// <returns>true if <paramref name="t1" /> and <paramref name="t2" /> have any different member values; otherwise, false.</returns>
		/// <param name="t1">The first object to compare.</param>
		/// <param name="t2">The second object to compare.</param>
		public static bool operator !=(TransitionTime t1, TransitionTime t2)
		{
			return !t1.Equals(t2);
		}

		/// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo.TransitionTime" /> object has identical values to a second <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.</summary>
		/// <returns>true if the two objects have identical property values; otherwise, false.</returns>
		/// <param name="other">An object to compare to the current instance. </param>
		/// <filterpriority>2</filterpriority>
		public bool Equals(TransitionTime other)
		{
			if (_isFixedDateRule == other._isFixedDateRule && _timeOfDay == other._timeOfDay && _month == other._month)
			{
				if (!other._isFixedDateRule)
				{
					if (_week == other._week)
					{
						return _dayOfWeek == other._dayOfWeek;
					}
					return false;
				}
				return _day == other._day;
			}
			return false;
		}

		/// <summary>Serves as a hash function for hashing algorithms and data structures such as hash tables.</summary>
		/// <returns>A 32-bit signed integer that serves as the hash code for this <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.</returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			return _month ^ (_week << 8);
		}

		private TransitionTime(DateTime timeOfDay, int month, int week, int day, DayOfWeek dayOfWeek, bool isFixedDateRule)
		{
			ValidateTransitionTime(timeOfDay, month, week, day, dayOfWeek);
			_timeOfDay = timeOfDay;
			_month = (byte)month;
			_week = (byte)week;
			_day = (byte)day;
			_dayOfWeek = dayOfWeek;
			_isFixedDateRule = isFixedDateRule;
		}

		/// <summary>Defines a time change that uses a fixed-date rule.</summary>
		/// <returns>Data about the time change.</returns>
		/// <param name="timeOfDay">The time at which the time change occurs.</param>
		/// <param name="month">The month in which the time change occurs.</param>
		/// <param name="day">The day of the month on which the time change occurs.</param>
		/// <exception cref="T:System.ArgumentException">The <paramref name="timeOfDay" /> parameter has a non-default date component.-or-The <paramref name="timeOfDay" /> parameter's <see cref="P:System.DateTime.Kind" /> property is not <see cref="F:System.DateTimeKind.Unspecified" />.-or-The <paramref name="timeOfDay" /> parameter does not represent a whole number of milliseconds.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="month" /> parameter is less than 1 or greater than 12.-or-The <paramref name="day" /> parameter is less than 1 or greater than 31.</exception>
		public static TransitionTime CreateFixedDateRule(DateTime timeOfDay, int month, int day)
		{
			return new TransitionTime(timeOfDay, month, 1, day, DayOfWeek.Sunday, isFixedDateRule: true);
		}

		/// <summary>Defines a time change that uses a floating-date rule.</summary>
		/// <returns>Data about the time change.</returns>
		/// <param name="timeOfDay">The time at which the time change occurs.</param>
		/// <param name="month">The month in which the time change occurs.</param>
		/// <param name="week">The week of the month in which the time change occurs.</param>
		/// <param name="dayOfWeek">The day of the week on which the time change occurs.</param>
		/// <exception cref="T:System.ArgumentException">The <paramref name="timeOfDay" /> parameter has a non-default date component.-or-The <paramref name="timeOfDay" /> parameter does not represent a whole number of milliseconds.-or-The <paramref name="timeOfDay" /> parameter's <see cref="P:System.DateTime.Kind" /> property is not <see cref="F:System.DateTimeKind.Unspecified" />.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="month" /> is less than 1 or greater than 12.-or-<paramref name="week" /> is less than 1 or greater than 5.-or-The <paramref name="dayOfWeek" /> parameter is not a member of the <see cref="T:System.DayOfWeek" /> enumeration.</exception>
		public static TransitionTime CreateFloatingDateRule(DateTime timeOfDay, int month, int week, DayOfWeek dayOfWeek)
		{
			return new TransitionTime(timeOfDay, month, week, 1, dayOfWeek, isFixedDateRule: false);
		}

		private static void ValidateTransitionTime(DateTime timeOfDay, int month, int week, int day, DayOfWeek dayOfWeek)
		{
			if (timeOfDay.Kind != DateTimeKind.Unspecified)
			{
				throw new ArgumentException("The supplied DateTime must have the Kind property set to DateTimeKind.Unspecified.", "timeOfDay");
			}
			if (month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException("month", "The Month parameter must be in the range 1 through 12.");
			}
			if (day < 1 || day > 31)
			{
				throw new ArgumentOutOfRangeException("day", "The Day parameter must be in the range 1 through 31.");
			}
			if (week < 1 || week > 5)
			{
				throw new ArgumentOutOfRangeException("week", "The Week parameter must be in the range 1 through 5.");
			}
			if (dayOfWeek < DayOfWeek.Sunday || dayOfWeek > DayOfWeek.Saturday)
			{
				throw new ArgumentOutOfRangeException("dayOfWeek", "The DayOfWeek enumeration must be in the range 0 through 6.");
			}
			timeOfDay.GetDatePart(out var year, out var month2, out var day2);
			if (year != 1 || month2 != 1 || day2 != 1 || timeOfDay.Ticks % 10000 != 0L)
			{
				throw new ArgumentException("The supplied DateTime must have the Year, Month, and Day properties set to 1.  The time cannot be specified more precisely than whole milliseconds.", "timeOfDay");
			}
		}

		/// <summary>Runs when the deserialization of an object has been completed.</summary>
		/// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.   </param>
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			try
			{
				ValidateTransitionTime(_timeOfDay, _month, _week, _day, _dayOfWeek);
			}
			catch (ArgumentException innerException)
			{
				throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException);
			}
		}

		/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data that is required to serialize this object.</summary>
		/// <param name="info">The object to populate with data.</param>
		/// <param name="context">The destination for this serialization (see <see cref="T:System.Runtime.Serialization.StreamingContext" />).</param>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("TimeOfDay", _timeOfDay);
			info.AddValue("Month", _month);
			info.AddValue("Week", _week);
			info.AddValue("Day", _day);
			info.AddValue("DayOfWeek", _dayOfWeek);
			info.AddValue("IsFixedDateRule", _isFixedDateRule);
		}

		private TransitionTime(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			_timeOfDay = (DateTime)info.GetValue("TimeOfDay", typeof(DateTime));
			_month = (byte)info.GetValue("Month", typeof(byte));
			_week = (byte)info.GetValue("Week", typeof(byte));
			_day = (byte)info.GetValue("Day", typeof(byte));
			_dayOfWeek = (DayOfWeek)info.GetValue("DayOfWeek", typeof(DayOfWeek));
			_isFixedDateRule = (bool)info.GetValue("IsFixedDateRule", typeof(bool));
		}
	}

	private const string TimeZonesRegistryHive = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones";

	private const string DisplayValue = "Display";

	private const string DaylightValue = "Dlt";

	private const string StandardValue = "Std";

	private const string MuiDisplayValue = "MUI_Display";

	private const string MuiDaylightValue = "MUI_Dlt";

	private const string MuiStandardValue = "MUI_Std";

	private const string TimeZoneInfoValue = "TZI";

	private const string FirstEntryValue = "FirstEntry";

	private const string LastEntryValue = "LastEntry";

	private const int MaxKeyLength = 255;

	private static Lazy<bool> lazyHaveRegistry = new Lazy<bool>(delegate
	{
		try
		{
			using (Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\TimeZoneInformation", writable: false))
			{
				return true;
			}
		}
		catch
		{
			return false;
		}
	});

	internal const uint TIME_ZONE_ID_INVALID = uint.MaxValue;

	internal const uint ERROR_NO_MORE_ITEMS = 259u;

	private readonly string _id;

	private readonly string _displayName;

	private readonly string _standardDisplayName;

	private readonly string _daylightDisplayName;

	private readonly TimeSpan _baseUtcOffset;

	private readonly bool _supportsDaylightSavingTime;

	private readonly AdjustmentRule[] _adjustmentRules;

	private const string UtcId = "UTC";

	private const string LocalId = "Local";

	private static readonly TimeZoneInfo s_utcTimeZone = CreateCustomTimeZone("UTC", TimeSpan.Zero, "UTC", "UTC");

	private static CachedData s_cachedData = new CachedData();

	private static readonly DateTime s_maxDateOnly = new DateTime(9999, 12, 31);

	private static readonly DateTime s_minDateOnly = new DateTime(1, 1, 2);

	private static readonly TimeSpan MaxOffset = TimeSpan.FromHours(14.0);

	private static readonly TimeSpan MinOffset = -MaxOffset;

	private static bool HaveRegistry => lazyHaveRegistry.Value;

	/// <summary>Gets the time zone identifier.</summary>
	/// <returns>The time zone identifier.</returns>
	public string Id => _id;

	/// <summary>Gets the general display name that represents the time zone.</summary>
	/// <returns>The time zone's general display name.</returns>
	public string DisplayName => _displayName ?? string.Empty;

	/// <summary>Gets the display name for the time zone's standard time.</summary>
	/// <returns>The display name of the time zone's standard time.</returns>
	public string StandardName => _standardDisplayName ?? string.Empty;

	/// <summary>Gets the display name for the current time zone's daylight saving time.</summary>
	/// <returns>The display name for the time zone's daylight saving time.</returns>
	public string DaylightName => _daylightDisplayName ?? string.Empty;

	/// <summary>Gets the time difference between the current time zone's standard time and Coordinated Universal Time (UTC).</summary>
	/// <returns>An object that indicates the time difference between the current time zone's standard time and Coordinated Universal Time (UTC).</returns>
	public TimeSpan BaseUtcOffset => _baseUtcOffset;

	/// <summary>Gets a value indicating whether the time zone has any daylight saving time rules.</summary>
	/// <returns>true if the time zone supports daylight saving time; otherwise, false.</returns>
	public bool SupportsDaylightSavingTime => _supportsDaylightSavingTime;

	/// <summary>Gets a <see cref="T:System.TimeZoneInfo" /> object that represents the local time zone.</summary>
	/// <returns>An object that represents the local time zone.</returns>
	public static TimeZoneInfo Local => s_cachedData.Local;

	/// <summary>Gets a <see cref="T:System.TimeZoneInfo" /> object that represents the Coordinated Universal Time (UTC) zone.</summary>
	/// <returns>An object that represents the Coordinated Universal Time (UTC) zone.</returns>
	public static TimeZoneInfo Utc => s_utcTimeZone;

	/// <summary>Retrieves an array of <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> objects that apply to the current <see cref="T:System.TimeZoneInfo" /> object.</summary>
	/// <returns>An array of objects for this time zone.</returns>
	/// <exception cref="T:System.OutOfMemoryException">The system does not have enough memory to make an in-memory copy of the adjustment rules.</exception>
	/// <filterpriority>2</filterpriority>
	public AdjustmentRule[] GetAdjustmentRules()
	{
		if (_adjustmentRules == null)
		{
			return Array.Empty<AdjustmentRule>();
		}
		return (AdjustmentRule[])_adjustmentRules.Clone();
	}

	private static void PopulateAllSystemTimeZones(CachedData cachedData)
	{
		if (HaveRegistry)
		{
			PopulateAllSystemTimeZonesFromRegistry(cachedData);
		}
		else
		{
			GetSystemTimeZonesWinRTFallback(cachedData);
		}
	}

	private static void PopulateAllSystemTimeZonesFromRegistry(CachedData cachedData)
	{
		using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", writable: false);
		if (registryKey != null)
		{
			string[] subKeyNames = registryKey.GetSubKeyNames();
			for (int i = 0; i < subKeyNames.Length; i++)
			{
				TryGetTimeZone(subKeyNames[i], dstDisabled: false, out var _, out var _, cachedData);
			}
		}
	}

	private TimeZoneInfo(in Interop.Kernel32.TIME_ZONE_INFORMATION zone, bool dstDisabled)
	{
		string standardName = zone.GetStandardName();
		if (standardName.Length == 0)
		{
			_id = "Local";
		}
		else
		{
			_id = standardName;
		}
		_baseUtcOffset = new TimeSpan(0, -zone.Bias, 0);
		if (!dstDisabled)
		{
			AdjustmentRule adjustmentRule = CreateAdjustmentRuleFromTimeZoneInformation(new Interop.Kernel32.REG_TZI_FORMAT(in zone), DateTime.MinValue.Date, DateTime.MaxValue.Date, zone.Bias);
			if (adjustmentRule != null)
			{
				_adjustmentRules = new AdjustmentRule[1] { adjustmentRule };
			}
		}
		ValidateTimeZoneInfo(_id, _baseUtcOffset, _adjustmentRules, out _supportsDaylightSavingTime);
		_displayName = standardName;
		_standardDisplayName = standardName;
		_daylightDisplayName = zone.GetDaylightName();
	}

	private static bool CheckDaylightSavingTimeNotSupported(in Interop.Kernel32.TIME_ZONE_INFORMATION timeZone)
	{
		return timeZone.DaylightDate.Equals(in timeZone.StandardDate);
	}

	private static AdjustmentRule CreateAdjustmentRuleFromTimeZoneInformation(in Interop.Kernel32.REG_TZI_FORMAT timeZoneInformation, DateTime startDate, DateTime endDate, int defaultBaseUtcOffset)
	{
		if (timeZoneInformation.StandardDate.Month == 0)
		{
			if (timeZoneInformation.Bias == defaultBaseUtcOffset)
			{
				return null;
			}
			return AdjustmentRule.CreateAdjustmentRule(startDate, endDate, TimeSpan.Zero, TransitionTime.CreateFixedDateRule(DateTime.MinValue, 1, 1), TransitionTime.CreateFixedDateRule(DateTime.MinValue.AddMilliseconds(1.0), 1, 1), new TimeSpan(0, defaultBaseUtcOffset - timeZoneInformation.Bias, 0), noDaylightTransitions: false);
		}
		if (!TransitionTimeFromTimeZoneInformation(in timeZoneInformation, out var transitionTime, readStartDate: true))
		{
			return null;
		}
		if (!TransitionTimeFromTimeZoneInformation(in timeZoneInformation, out var transitionTime2, readStartDate: false))
		{
			return null;
		}
		if (transitionTime.Equals(transitionTime2))
		{
			return null;
		}
		return AdjustmentRule.CreateAdjustmentRule(startDate, endDate, new TimeSpan(0, -timeZoneInformation.DaylightBias, 0), transitionTime, transitionTime2, new TimeSpan(0, defaultBaseUtcOffset - timeZoneInformation.Bias, 0), noDaylightTransitions: false);
	}

	private static string FindIdFromTimeZoneInformation(in Interop.Kernel32.TIME_ZONE_INFORMATION timeZone, out bool dstDisabled)
	{
		dstDisabled = false;
		using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", writable: false))
		{
			if (registryKey == null)
			{
				return null;
			}
			string[] subKeyNames = registryKey.GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				if (TryCompareTimeZoneInformationToRegistry(in timeZone, text, out dstDisabled))
				{
					return text;
				}
			}
		}
		return null;
	}

	private static TimeZoneInfo GetLocalTimeZone(CachedData cachedData)
	{
		if (!HaveRegistry)
		{
			return GetLocalTimeZoneInfoWinRTFallback();
		}
		Interop.Kernel32.TIME_DYNAMIC_ZONE_INFORMATION pTimeZoneInformation = default(Interop.Kernel32.TIME_DYNAMIC_ZONE_INFORMATION);
		if (Interop.Kernel32.GetDynamicTimeZoneInformation(out pTimeZoneInformation) == uint.MaxValue)
		{
			return CreateCustomTimeZone("Local", TimeSpan.Zero, "Local", "Local");
		}
		string timeZoneKeyName = pTimeZoneInformation.GetTimeZoneKeyName();
		if (timeZoneKeyName.Length != 0 && TryGetTimeZone(timeZoneKeyName, pTimeZoneInformation.DynamicDaylightTimeDisabled != 0, out var value, out var _, cachedData) == TimeZoneInfoResult.Success)
		{
			return value;
		}
		Interop.Kernel32.TIME_ZONE_INFORMATION timeZone = new Interop.Kernel32.TIME_ZONE_INFORMATION(in pTimeZoneInformation);
		bool dstDisabled;
		string text = FindIdFromTimeZoneInformation(in timeZone, out dstDisabled);
		if (text != null && TryGetTimeZone(text, dstDisabled, out var value2, out var _, cachedData) == TimeZoneInfoResult.Success)
		{
			return value2;
		}
		return GetLocalTimeZoneFromWin32Data(in timeZone, dstDisabled);
	}

	private static TimeZoneInfo GetLocalTimeZoneFromWin32Data(in Interop.Kernel32.TIME_ZONE_INFORMATION timeZoneInformation, bool dstDisabled)
	{
		try
		{
			return new TimeZoneInfo(in timeZoneInformation, dstDisabled);
		}
		catch (ArgumentException)
		{
		}
		catch (InvalidTimeZoneException)
		{
		}
		if (!dstDisabled)
		{
			try
			{
				return new TimeZoneInfo(in timeZoneInformation, dstDisabled: true);
			}
			catch (ArgumentException)
			{
			}
			catch (InvalidTimeZoneException)
			{
			}
		}
		return CreateCustomTimeZone("Local", TimeSpan.Zero, "Local", "Local");
	}

	/// <summary>Retrieves a <see cref="T:System.TimeZoneInfo" /> object from the registry based on its identifier.</summary>
	/// <returns>An object whose identifier is the value of the <paramref name="id" /> parameter.</returns>
	/// <param name="id">The time zone identifier, which corresponds to the <see cref="P:System.TimeZoneInfo.Id" /> property.      </param>
	/// <exception cref="T:System.OutOfMemoryException">The system does not have enough memory to hold information about the time zone.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
	/// <exception cref="T:System.TimeZoneNotFoundException">The time zone identifier specified by <paramref name="id" /> was not found. This means that a registry key whose name matches <paramref name="id" /> does not exist, or that the key exists but does not contain any time zone data.</exception>
	/// <exception cref="T:System.Security.SecurityException">The process does not have the permissions required to read from the registry key that contains the time zone information.</exception>
	/// <exception cref="T:System.InvalidTimeZoneException">The time zone identifier was found, but the registry data is corrupted.</exception>
	public static TimeZoneInfo FindSystemTimeZoneById(string id)
	{
		if (string.Equals(id, "UTC", StringComparison.OrdinalIgnoreCase))
		{
			return Utc;
		}
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (id.Length == 0 || id.Length > 255 || id.Contains('\0'))
		{
			throw new TimeZoneNotFoundException(SR.Format("The time zone ID '{0}' was not found on the local computer.", id));
		}
		CachedData cachedData = s_cachedData;
		TimeZoneInfoResult timeZoneInfoResult;
		TimeZoneInfo value;
		Exception e;
		lock (cachedData)
		{
			timeZoneInfoResult = TryGetTimeZone(id, dstDisabled: false, out value, out e, cachedData);
		}
		return timeZoneInfoResult switch
		{
			TimeZoneInfoResult.Success => value, 
			TimeZoneInfoResult.InvalidTimeZoneException => throw new InvalidTimeZoneException(SR.Format("The time zone ID '{0}' was found on the local computer, but the registry information was corrupt.", id), e), 
			TimeZoneInfoResult.SecurityException => throw new SecurityException(SR.Format("The time zone ID '{0}' was found on the local computer, but the application does not have permission to read the registry information.", id), e), 
			_ => throw new TimeZoneNotFoundException(SR.Format("The time zone ID '{0}' was not found on the local computer.", id), e), 
		};
	}

	internal static TimeSpan GetDateTimeNowUtcOffsetFromUtc(DateTime time, out bool isAmbiguousLocalDst)
	{
		bool flag = false;
		isAmbiguousLocalDst = false;
		int year = time.Year;
		OffsetAndRule oneYearLocalFromUtc = s_cachedData.GetOneYearLocalFromUtc(year);
		TimeSpan offset = oneYearLocalFromUtc.Offset;
		if (oneYearLocalFromUtc.Rule != null)
		{
			offset += oneYearLocalFromUtc.Rule.BaseUtcOffsetDelta;
			if (oneYearLocalFromUtc.Rule.HasDaylightSaving)
			{
				flag = GetIsDaylightSavingsFromUtc(time, year, oneYearLocalFromUtc.Offset, oneYearLocalFromUtc.Rule, null, out isAmbiguousLocalDst, Local);
				offset += (flag ? oneYearLocalFromUtc.Rule.DaylightDelta : TimeSpan.Zero);
			}
		}
		return offset;
	}

	private static bool TransitionTimeFromTimeZoneInformation(in Interop.Kernel32.REG_TZI_FORMAT timeZoneInformation, out TransitionTime transitionTime, bool readStartDate)
	{
		if (timeZoneInformation.StandardDate.Month == 0)
		{
			transitionTime = default(TransitionTime);
			return false;
		}
		if (readStartDate)
		{
			if (timeZoneInformation.DaylightDate.Year == 0)
			{
				transitionTime = TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, timeZoneInformation.DaylightDate.Hour, timeZoneInformation.DaylightDate.Minute, timeZoneInformation.DaylightDate.Second, timeZoneInformation.DaylightDate.Milliseconds), timeZoneInformation.DaylightDate.Month, timeZoneInformation.DaylightDate.Day, (DayOfWeek)timeZoneInformation.DaylightDate.DayOfWeek);
			}
			else
			{
				transitionTime = TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, timeZoneInformation.DaylightDate.Hour, timeZoneInformation.DaylightDate.Minute, timeZoneInformation.DaylightDate.Second, timeZoneInformation.DaylightDate.Milliseconds), timeZoneInformation.DaylightDate.Month, timeZoneInformation.DaylightDate.Day);
			}
		}
		else if (timeZoneInformation.StandardDate.Year == 0)
		{
			transitionTime = TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, timeZoneInformation.StandardDate.Hour, timeZoneInformation.StandardDate.Minute, timeZoneInformation.StandardDate.Second, timeZoneInformation.StandardDate.Milliseconds), timeZoneInformation.StandardDate.Month, timeZoneInformation.StandardDate.Day, (DayOfWeek)timeZoneInformation.StandardDate.DayOfWeek);
		}
		else
		{
			transitionTime = TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, timeZoneInformation.StandardDate.Hour, timeZoneInformation.StandardDate.Minute, timeZoneInformation.StandardDate.Second, timeZoneInformation.StandardDate.Milliseconds), timeZoneInformation.StandardDate.Month, timeZoneInformation.StandardDate.Day);
		}
		return true;
	}

	private static bool TryCreateAdjustmentRules(string id, in Interop.Kernel32.REG_TZI_FORMAT defaultTimeZoneInformation, out AdjustmentRule[] rules, out Exception e, int defaultBaseUtcOffset)
	{
		rules = null;
		e = null;
		try
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\" + id + "\\Dynamic DST", writable: false);
			if (registryKey == null)
			{
				AdjustmentRule adjustmentRule = CreateAdjustmentRuleFromTimeZoneInformation(in defaultTimeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date, defaultBaseUtcOffset);
				if (adjustmentRule != null)
				{
					rules = new AdjustmentRule[1] { adjustmentRule };
				}
				return true;
			}
			int num = (int)registryKey.GetValue("FirstEntry", -1, RegistryValueOptions.None);
			int num2 = (int)registryKey.GetValue("LastEntry", -1, RegistryValueOptions.None);
			if (num == -1 || num2 == -1 || num > num2)
			{
				return false;
			}
			if (!TryGetTimeZoneEntryFromRegistry(registryKey, num.ToString(CultureInfo.InvariantCulture), out var dtzi))
			{
				return false;
			}
			if (num == num2)
			{
				AdjustmentRule adjustmentRule2 = CreateAdjustmentRuleFromTimeZoneInformation(in dtzi, DateTime.MinValue.Date, DateTime.MaxValue.Date, defaultBaseUtcOffset);
				if (adjustmentRule2 != null)
				{
					rules = new AdjustmentRule[1] { adjustmentRule2 };
				}
				return true;
			}
			List<AdjustmentRule> list = new List<AdjustmentRule>(1);
			AdjustmentRule adjustmentRule3 = CreateAdjustmentRuleFromTimeZoneInformation(in dtzi, DateTime.MinValue.Date, new DateTime(num, 12, 31), defaultBaseUtcOffset);
			if (adjustmentRule3 != null)
			{
				list.Add(adjustmentRule3);
			}
			for (int i = num + 1; i < num2; i++)
			{
				if (!TryGetTimeZoneEntryFromRegistry(registryKey, i.ToString(CultureInfo.InvariantCulture), out dtzi))
				{
					return false;
				}
				AdjustmentRule adjustmentRule4 = CreateAdjustmentRuleFromTimeZoneInformation(in dtzi, new DateTime(i, 1, 1), new DateTime(i, 12, 31), defaultBaseUtcOffset);
				if (adjustmentRule4 != null)
				{
					list.Add(adjustmentRule4);
				}
			}
			if (!TryGetTimeZoneEntryFromRegistry(registryKey, num2.ToString(CultureInfo.InvariantCulture), out dtzi))
			{
				return false;
			}
			AdjustmentRule adjustmentRule5 = CreateAdjustmentRuleFromTimeZoneInformation(in dtzi, new DateTime(num2, 1, 1), DateTime.MaxValue.Date, defaultBaseUtcOffset);
			if (adjustmentRule5 != null)
			{
				list.Add(adjustmentRule5);
			}
			if (list.Count != 0)
			{
				rules = list.ToArray();
			}
		}
		catch (InvalidCastException ex)
		{
			e = ex;
			return false;
		}
		catch (ArgumentOutOfRangeException ex2)
		{
			e = ex2;
			return false;
		}
		catch (ArgumentException ex3)
		{
			e = ex3;
			return false;
		}
		return true;
	}

	private unsafe static bool TryGetTimeZoneEntryFromRegistry(RegistryKey key, string name, out Interop.Kernel32.REG_TZI_FORMAT dtzi)
	{
		if (!(key.GetValue(name, null, RegistryValueOptions.None) is byte[] array) || array.Length != sizeof(Interop.Kernel32.REG_TZI_FORMAT))
		{
			dtzi = default(Interop.Kernel32.REG_TZI_FORMAT);
			return false;
		}
		fixed (byte* ptr = &array[0])
		{
			dtzi = *(Interop.Kernel32.REG_TZI_FORMAT*)ptr;
		}
		return true;
	}

	private static bool TryCompareStandardDate(in Interop.Kernel32.TIME_ZONE_INFORMATION timeZone, in Interop.Kernel32.REG_TZI_FORMAT registryTimeZoneInfo)
	{
		if (timeZone.Bias == registryTimeZoneInfo.Bias && timeZone.StandardBias == registryTimeZoneInfo.StandardBias)
		{
			return timeZone.StandardDate.Equals(in registryTimeZoneInfo.StandardDate);
		}
		return false;
	}

	private static bool TryCompareTimeZoneInformationToRegistry(in Interop.Kernel32.TIME_ZONE_INFORMATION timeZone, string id, out bool dstDisabled)
	{
		dstDisabled = false;
		using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\" + id, writable: false);
		if (registryKey == null)
		{
			return false;
		}
		if (!TryGetTimeZoneEntryFromRegistry(registryKey, "TZI", out var dtzi))
		{
			return false;
		}
		if (!TryCompareStandardDate(in timeZone, in dtzi))
		{
			return false;
		}
		bool flag = dstDisabled || CheckDaylightSavingTimeNotSupported(in timeZone) || (timeZone.DaylightBias == dtzi.DaylightBias && timeZone.DaylightDate.Equals(in dtzi.DaylightDate));
		if (flag)
		{
			flag = string.Equals(registryKey.GetValue("Std", string.Empty, RegistryValueOptions.None) as string, timeZone.GetStandardName(), StringComparison.Ordinal);
		}
		return flag;
	}

	private static string TryGetLocalizedNameByMuiNativeResource(string resource)
	{
		if (string.IsNullOrEmpty(resource))
		{
			return string.Empty;
		}
		string[] array = resource.Split(',');
		if (array.Length != 2)
		{
			return string.Empty;
		}
		string systemDirectory = Environment.SystemDirectory;
		string path = array[0].TrimStart('@');
		string filePath;
		try
		{
			filePath = Path.Combine(systemDirectory, path);
		}
		catch (ArgumentException)
		{
			return string.Empty;
		}
		if (!int.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return string.Empty;
		}
		result = -result;
		try
		{
			StringBuilder stringBuilder = StringBuilderCache.Acquire(260);
			stringBuilder.Length = 260;
			int fileMuiPathLength = 260;
			int languageLength = 0;
			long enumerator = 0L;
			if (!Interop.Kernel32.GetFileMUIPath(16u, filePath, null, ref languageLength, stringBuilder, ref fileMuiPathLength, ref enumerator))
			{
				StringBuilderCache.Release(stringBuilder);
				return string.Empty;
			}
			return TryGetLocalizedNameByNativeResource(StringBuilderCache.GetStringAndRelease(stringBuilder), result);
		}
		catch (EntryPointNotFoundException)
		{
			return string.Empty;
		}
	}

	private static string TryGetLocalizedNameByNativeResource(string filePath, int resource)
	{
		using (SafeLibraryHandle safeLibraryHandle = Interop.Kernel32.LoadLibraryEx(filePath, IntPtr.Zero, 2))
		{
			if (!safeLibraryHandle.IsInvalid)
			{
				StringBuilder stringBuilder = StringBuilderCache.Acquire(500);
				if (Interop.User32.LoadString(safeLibraryHandle, resource, stringBuilder, 500) != 0)
				{
					return StringBuilderCache.GetStringAndRelease(stringBuilder);
				}
			}
		}
		return string.Empty;
	}

	private static void GetLocalizedNamesByRegistryKey(RegistryKey key, out string displayName, out string standardName, out string daylightName)
	{
		displayName = string.Empty;
		standardName = string.Empty;
		daylightName = string.Empty;
		string text = key.GetValue("MUI_Display", string.Empty, RegistryValueOptions.None) as string;
		string text2 = key.GetValue("MUI_Std", string.Empty, RegistryValueOptions.None) as string;
		string text3 = key.GetValue("MUI_Dlt", string.Empty, RegistryValueOptions.None) as string;
		if (!string.IsNullOrEmpty(text))
		{
			displayName = TryGetLocalizedNameByMuiNativeResource(text);
		}
		if (!string.IsNullOrEmpty(text2))
		{
			standardName = TryGetLocalizedNameByMuiNativeResource(text2);
		}
		if (!string.IsNullOrEmpty(text3))
		{
			daylightName = TryGetLocalizedNameByMuiNativeResource(text3);
		}
		if (string.IsNullOrEmpty(displayName))
		{
			displayName = key.GetValue("Display", string.Empty, RegistryValueOptions.None) as string;
		}
		if (string.IsNullOrEmpty(standardName))
		{
			standardName = key.GetValue("Std", string.Empty, RegistryValueOptions.None) as string;
		}
		if (string.IsNullOrEmpty(daylightName))
		{
			daylightName = key.GetValue("Dlt", string.Empty, RegistryValueOptions.None) as string;
		}
	}

	private static TimeZoneInfoResult TryGetTimeZoneFromLocalMachine(string id, out TimeZoneInfo value, out Exception e)
	{
		if (HaveRegistry)
		{
			return TryGetTimeZoneFromLocalRegistry(id, out value, out e);
		}
		e = null;
		value = FindSystemTimeZoneByIdWinRTFallback(id);
		return TimeZoneInfoResult.Success;
	}

	private static TimeZoneInfoResult TryGetTimeZoneFromLocalRegistry(string id, out TimeZoneInfo value, out Exception e)
	{
		e = null;
		using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\" + id, writable: false);
		if (registryKey == null)
		{
			value = null;
			return TimeZoneInfoResult.TimeZoneNotFoundException;
		}
		if (!TryGetTimeZoneEntryFromRegistry(registryKey, "TZI", out var dtzi))
		{
			value = null;
			return TimeZoneInfoResult.InvalidTimeZoneException;
		}
		if (!TryCreateAdjustmentRules(id, in dtzi, out var rules, out e, dtzi.Bias))
		{
			value = null;
			return TimeZoneInfoResult.InvalidTimeZoneException;
		}
		GetLocalizedNamesByRegistryKey(registryKey, out var displayName, out var standardName, out var daylightName);
		try
		{
			value = new TimeZoneInfo(id, new TimeSpan(0, -dtzi.Bias, 0), displayName, standardName, daylightName, rules, disableDaylightSavingTime: false);
			return TimeZoneInfoResult.Success;
		}
		catch (ArgumentException ex)
		{
			value = null;
			e = ex;
			return TimeZoneInfoResult.InvalidTimeZoneException;
		}
		catch (InvalidTimeZoneException ex2)
		{
			value = null;
			e = ex2;
			return TimeZoneInfoResult.InvalidTimeZoneException;
		}
	}

	[DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
	internal static extern uint EnumDynamicTimeZoneInformation(uint dwIndex, out DYNAMIC_TIME_ZONE_INFORMATION lpTimeZoneInformation);

	[DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
	internal static extern uint GetDynamicTimeZoneInformation(out DYNAMIC_TIME_ZONE_INFORMATION pTimeZoneInformation);

	[DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
	internal static extern uint GetDynamicTimeZoneInformationEffectiveYears(ref DYNAMIC_TIME_ZONE_INFORMATION lpTimeZoneInformation, out uint FirstYear, out uint LastYear);

	[DllImport("api-ms-win-core-timezone-l1-1-0.dll")]
	internal static extern bool GetTimeZoneInformationForYear(ushort wYear, ref DYNAMIC_TIME_ZONE_INFORMATION pdtzi, out Interop.Kernel32.TIME_ZONE_INFORMATION ptzi);

	internal static AdjustmentRule CreateAdjustmentRuleFromTimeZoneInformation(ref DYNAMIC_TIME_ZONE_INFORMATION timeZoneInformation, DateTime startDate, DateTime endDate, int defaultBaseUtcOffset)
	{
		if (timeZoneInformation.TZI.StandardDate.Month == 0)
		{
			if (timeZoneInformation.TZI.Bias == defaultBaseUtcOffset)
			{
				return null;
			}
			return AdjustmentRule.CreateAdjustmentRule(startDate, endDate, TimeSpan.Zero, TransitionTime.CreateFixedDateRule(DateTime.MinValue, 1, 1), TransitionTime.CreateFixedDateRule(DateTime.MinValue.AddMilliseconds(1.0), 1, 1), new TimeSpan(0, defaultBaseUtcOffset - timeZoneInformation.TZI.Bias, 0), noDaylightTransitions: false);
		}
		if (!TransitionTimeFromTimeZoneInformation(timeZoneInformation, out var transitionTime, readStartDate: true))
		{
			return null;
		}
		if (!TransitionTimeFromTimeZoneInformation(timeZoneInformation, out var transitionTime2, readStartDate: false))
		{
			return null;
		}
		if (transitionTime.Equals(transitionTime2))
		{
			return null;
		}
		return AdjustmentRule.CreateAdjustmentRule(startDate, endDate, new TimeSpan(0, -timeZoneInformation.TZI.DaylightBias, 0), transitionTime, transitionTime2, new TimeSpan(0, defaultBaseUtcOffset - timeZoneInformation.TZI.Bias, 0), noDaylightTransitions: false);
	}

	private static bool TransitionTimeFromTimeZoneInformation(DYNAMIC_TIME_ZONE_INFORMATION timeZoneInformation, out TransitionTime transitionTime, bool readStartDate)
	{
		if (timeZoneInformation.TZI.StandardDate.Month == 0)
		{
			transitionTime = default(TransitionTime);
			return false;
		}
		if (readStartDate)
		{
			if (timeZoneInformation.TZI.DaylightDate.Year == 0)
			{
				transitionTime = TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, timeZoneInformation.TZI.DaylightDate.Hour, timeZoneInformation.TZI.DaylightDate.Minute, timeZoneInformation.TZI.DaylightDate.Second, timeZoneInformation.TZI.DaylightDate.Milliseconds), timeZoneInformation.TZI.DaylightDate.Month, timeZoneInformation.TZI.DaylightDate.Day, (DayOfWeek)timeZoneInformation.TZI.DaylightDate.DayOfWeek);
			}
			else
			{
				transitionTime = TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, timeZoneInformation.TZI.DaylightDate.Hour, timeZoneInformation.TZI.DaylightDate.Minute, timeZoneInformation.TZI.DaylightDate.Second, timeZoneInformation.TZI.DaylightDate.Milliseconds), timeZoneInformation.TZI.DaylightDate.Month, timeZoneInformation.TZI.DaylightDate.Day);
			}
		}
		else if (timeZoneInformation.TZI.StandardDate.Year == 0)
		{
			transitionTime = TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, timeZoneInformation.TZI.StandardDate.Hour, timeZoneInformation.TZI.StandardDate.Minute, timeZoneInformation.TZI.StandardDate.Second, timeZoneInformation.TZI.StandardDate.Milliseconds), timeZoneInformation.TZI.StandardDate.Month, timeZoneInformation.TZI.StandardDate.Day, (DayOfWeek)timeZoneInformation.TZI.StandardDate.DayOfWeek);
		}
		else
		{
			transitionTime = TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, timeZoneInformation.TZI.StandardDate.Hour, timeZoneInformation.TZI.StandardDate.Minute, timeZoneInformation.TZI.StandardDate.Second, timeZoneInformation.TZI.StandardDate.Milliseconds), timeZoneInformation.TZI.StandardDate.Month, timeZoneInformation.TZI.StandardDate.Day);
		}
		return true;
	}

	internal static TimeZoneInfo TryCreateTimeZone(DYNAMIC_TIME_ZONE_INFORMATION timeZoneInformation)
	{
		uint FirstYear = 0u;
		uint LastYear = 0u;
		AdjustmentRule[] adjustmentRules = null;
		int bias = timeZoneInformation.TZI.Bias;
		if (string.IsNullOrEmpty(timeZoneInformation.TimeZoneKeyName))
		{
			return null;
		}
		try
		{
			if (GetDynamicTimeZoneInformationEffectiveYears(ref timeZoneInformation, out FirstYear, out LastYear) != 0)
			{
				FirstYear = (LastYear = 0u);
			}
		}
		catch
		{
			FirstYear = (LastYear = 0u);
		}
		if (FirstYear == LastYear)
		{
			AdjustmentRule adjustmentRule = CreateAdjustmentRuleFromTimeZoneInformation(ref timeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date, bias);
			if (adjustmentRule != null)
			{
				adjustmentRules = new AdjustmentRule[1] { adjustmentRule };
			}
		}
		else
		{
			DYNAMIC_TIME_ZONE_INFORMATION timeZoneInformation2 = default(DYNAMIC_TIME_ZONE_INFORMATION);
			List<AdjustmentRule> list = new List<AdjustmentRule>();
			if (!GetTimeZoneInformationForYear((ushort)FirstYear, ref timeZoneInformation, out timeZoneInformation2.TZI))
			{
				return null;
			}
			AdjustmentRule adjustmentRule = CreateAdjustmentRuleFromTimeZoneInformation(ref timeZoneInformation2, DateTime.MinValue.Date, new DateTime((int)FirstYear, 12, 31), bias);
			if (adjustmentRule != null)
			{
				list.Add(adjustmentRule);
			}
			for (uint num = FirstYear + 1; num < LastYear; num++)
			{
				if (!GetTimeZoneInformationForYear((ushort)num, ref timeZoneInformation, out timeZoneInformation2.TZI))
				{
					return null;
				}
				adjustmentRule = CreateAdjustmentRuleFromTimeZoneInformation(ref timeZoneInformation2, new DateTime((int)num, 1, 1), new DateTime((int)num, 12, 31), bias);
				if (adjustmentRule != null)
				{
					list.Add(adjustmentRule);
				}
			}
			if (!GetTimeZoneInformationForYear((ushort)LastYear, ref timeZoneInformation, out timeZoneInformation2.TZI))
			{
				return null;
			}
			adjustmentRule = CreateAdjustmentRuleFromTimeZoneInformation(ref timeZoneInformation2, new DateTime((int)LastYear, 1, 1), DateTime.MaxValue.Date, bias);
			if (adjustmentRule != null)
			{
				list.Add(adjustmentRule);
			}
			if (list.Count > 0)
			{
				adjustmentRules = list.ToArray();
			}
		}
		return new TimeZoneInfo(timeZoneInformation.TimeZoneKeyName, new TimeSpan(0, -timeZoneInformation.TZI.Bias, 0), timeZoneInformation.TZI.GetStandardName(), timeZoneInformation.TZI.GetStandardName(), timeZoneInformation.TZI.GetDaylightName(), adjustmentRules, disableDaylightSavingTime: false);
	}

	internal static TimeZoneInfo GetLocalTimeZoneInfoWinRTFallback()
	{
		try
		{
			if (GetDynamicTimeZoneInformation(out var pTimeZoneInformation) == uint.MaxValue)
			{
				return Utc;
			}
			TimeZoneInfo timeZoneInfo = TryCreateTimeZone(pTimeZoneInformation);
			return (timeZoneInfo != null) ? timeZoneInfo : Utc;
		}
		catch
		{
			return Utc;
		}
	}

	internal static TimeZoneInfo FindSystemTimeZoneByIdWinRTFallback(string id)
	{
		foreach (TimeZoneInfo systemTimeZone in GetSystemTimeZones())
		{
			if (string.Compare(id, systemTimeZone.Id, StringComparison.Ordinal) == 0)
			{
				return systemTimeZone;
			}
		}
		throw new TimeZoneNotFoundException();
	}

	private static void GetSystemTimeZonesWinRTFallback(CachedData cachedData)
	{
		List<TimeZoneInfo> list = new List<TimeZoneInfo>();
		try
		{
			uint num = 0u;
			DYNAMIC_TIME_ZONE_INFORMATION lpTimeZoneInformation;
			while (EnumDynamicTimeZoneInformation(num++, out lpTimeZoneInformation) != 259)
			{
				TimeZoneInfo timeZoneInfo = TryCreateTimeZone(lpTimeZoneInformation);
				if (timeZoneInfo != null)
				{
					list.Add(timeZoneInfo);
				}
			}
		}
		catch
		{
		}
		if (list.Count == 0)
		{
			list.Add(Local);
			list.Add(Utc);
		}
		list.Sort(delegate(TimeZoneInfo x, TimeZoneInfo y)
		{
			int num2 = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
			return (num2 != 0) ? num2 : string.CompareOrdinal(x.DisplayName, y.DisplayName);
		});
		foreach (TimeZoneInfo item in list)
		{
			if (cachedData._systemTimeZones == null)
			{
				cachedData._systemTimeZones = new Dictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase);
			}
			cachedData._systemTimeZones.Add(item.Id, item);
		}
	}

	/// <summary>Returns information about the possible dates and times that an ambiguous date and time can be mapped to.</summary>
	/// <returns>An array of objects that represents possible Coordinated Universal Time (UTC) offsets that a particular date and time can be mapped to.</returns>
	/// <param name="dateTimeOffset">A date and time.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dateTime" /> is not an ambiguous time.</exception>
	/// <filterpriority>2</filterpriority>
	public TimeSpan[] GetAmbiguousTimeOffsets(DateTimeOffset dateTimeOffset)
	{
		if (!SupportsDaylightSavingTime)
		{
			throw new ArgumentException("The supplied DateTimeOffset is not in an ambiguous time range.", "dateTimeOffset");
		}
		DateTime dateTime = ConvertTime(dateTimeOffset, this).DateTime;
		bool flag = false;
		int? ruleIndex;
		AdjustmentRule adjustmentRuleForAmbiguousOffsets = GetAdjustmentRuleForAmbiguousOffsets(dateTime, out ruleIndex);
		if (adjustmentRuleForAmbiguousOffsets != null && adjustmentRuleForAmbiguousOffsets.HasDaylightSaving)
		{
			DaylightTimeStruct daylightTime = GetDaylightTime(dateTime.Year, adjustmentRuleForAmbiguousOffsets, ruleIndex);
			flag = GetIsAmbiguousTime(dateTime, adjustmentRuleForAmbiguousOffsets, daylightTime);
		}
		if (!flag)
		{
			throw new ArgumentException("The supplied DateTimeOffset is not in an ambiguous time range.", "dateTimeOffset");
		}
		TimeSpan[] array = new TimeSpan[2];
		TimeSpan timeSpan = _baseUtcOffset + adjustmentRuleForAmbiguousOffsets.BaseUtcOffsetDelta;
		if (adjustmentRuleForAmbiguousOffsets.DaylightDelta > TimeSpan.Zero)
		{
			array[0] = timeSpan;
			array[1] = timeSpan + adjustmentRuleForAmbiguousOffsets.DaylightDelta;
		}
		else
		{
			array[0] = timeSpan + adjustmentRuleForAmbiguousOffsets.DaylightDelta;
			array[1] = timeSpan;
		}
		return array;
	}

	/// <summary>Returns information about the possible dates and times that an ambiguous date and time can be mapped to.</summary>
	/// <returns>An array of objects that represents possible Coordinated Universal Time (UTC) offsets that a particular date and time can be mapped to.</returns>
	/// <param name="dateTime">A date and time.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dateTime" /> is not an ambiguous time.</exception>
	/// <filterpriority>2</filterpriority>
	public TimeSpan[] GetAmbiguousTimeOffsets(DateTime dateTime)
	{
		if (!SupportsDaylightSavingTime)
		{
			throw new ArgumentException("The supplied DateTime is not in an ambiguous time range.", "dateTime");
		}
		DateTime dateTime2;
		if (dateTime.Kind == DateTimeKind.Local)
		{
			CachedData cachedData = s_cachedData;
			dateTime2 = ConvertTime(dateTime, cachedData.Local, this, TimeZoneInfoOptions.None, cachedData);
		}
		else if (dateTime.Kind == DateTimeKind.Utc)
		{
			CachedData cachedData2 = s_cachedData;
			dateTime2 = ConvertTime(dateTime, s_utcTimeZone, this, TimeZoneInfoOptions.None, cachedData2);
		}
		else
		{
			dateTime2 = dateTime;
		}
		bool flag = false;
		int? ruleIndex;
		AdjustmentRule adjustmentRuleForAmbiguousOffsets = GetAdjustmentRuleForAmbiguousOffsets(dateTime2, out ruleIndex);
		if (adjustmentRuleForAmbiguousOffsets != null && adjustmentRuleForAmbiguousOffsets.HasDaylightSaving)
		{
			DaylightTimeStruct daylightTime = GetDaylightTime(dateTime2.Year, adjustmentRuleForAmbiguousOffsets, ruleIndex);
			flag = GetIsAmbiguousTime(dateTime2, adjustmentRuleForAmbiguousOffsets, daylightTime);
		}
		if (!flag)
		{
			throw new ArgumentException("The supplied DateTime is not in an ambiguous time range.", "dateTime");
		}
		TimeSpan[] array = new TimeSpan[2];
		TimeSpan timeSpan = _baseUtcOffset + adjustmentRuleForAmbiguousOffsets.BaseUtcOffsetDelta;
		if (adjustmentRuleForAmbiguousOffsets.DaylightDelta > TimeSpan.Zero)
		{
			array[0] = timeSpan;
			array[1] = timeSpan + adjustmentRuleForAmbiguousOffsets.DaylightDelta;
		}
		else
		{
			array[0] = timeSpan + adjustmentRuleForAmbiguousOffsets.DaylightDelta;
			array[1] = timeSpan;
		}
		return array;
	}

	private AdjustmentRule GetAdjustmentRuleForAmbiguousOffsets(DateTime adjustedTime, out int? ruleIndex)
	{
		AdjustmentRule adjustmentRuleForTime = GetAdjustmentRuleForTime(adjustedTime, out ruleIndex);
		if (adjustmentRuleForTime != null && adjustmentRuleForTime.NoDaylightTransitions && !adjustmentRuleForTime.HasDaylightSaving)
		{
			return GetPreviousAdjustmentRule(adjustmentRuleForTime, ruleIndex);
		}
		return adjustmentRuleForTime;
	}

	private AdjustmentRule GetPreviousAdjustmentRule(AdjustmentRule rule, int? ruleIndex)
	{
		if (ruleIndex.HasValue && 0 < ruleIndex.Value && ruleIndex.Value < _adjustmentRules.Length)
		{
			return _adjustmentRules[ruleIndex.Value - 1];
		}
		AdjustmentRule result = rule;
		for (int i = 1; i < _adjustmentRules.Length; i++)
		{
			if (rule == _adjustmentRules[i])
			{
				result = _adjustmentRules[i - 1];
				break;
			}
		}
		return result;
	}

	/// <summary>Calculates the offset or difference between the time in this time zone and Coordinated Universal Time (UTC) for a particular date and time.</summary>
	/// <returns>An object that indicates the time difference between Coordinated Universal Time (UTC) and the current time zone.</returns>
	/// <param name="dateTimeOffset">The date and time to determine the offset for.</param>
	public TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset)
	{
		return GetUtcOffsetFromUtc(dateTimeOffset.UtcDateTime, this);
	}

	/// <summary>Calculates the offset or difference between the time in this time zone and Coordinated Universal Time (UTC) for a particular date and time.</summary>
	/// <returns>An object that indicates the time difference between the two time zones.</returns>
	/// <param name="dateTime">The date and time to determine the offset for.   </param>
	public TimeSpan GetUtcOffset(DateTime dateTime)
	{
		return GetUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime, s_cachedData);
	}

	internal static TimeSpan GetLocalUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags)
	{
		CachedData cachedData = s_cachedData;
		return cachedData.Local.GetUtcOffset(dateTime, flags, cachedData);
	}

	internal TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags)
	{
		return GetUtcOffset(dateTime, flags, s_cachedData);
	}

	private TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags, CachedData cachedData)
	{
		if (dateTime.Kind == DateTimeKind.Local)
		{
			if (cachedData.GetCorrespondingKind(this) != DateTimeKind.Local)
			{
				return GetUtcOffsetFromUtc(ConvertTime(dateTime, cachedData.Local, s_utcTimeZone, flags), this);
			}
		}
		else if (dateTime.Kind == DateTimeKind.Utc)
		{
			if (cachedData.GetCorrespondingKind(this) == DateTimeKind.Utc)
			{
				return _baseUtcOffset;
			}
			return GetUtcOffsetFromUtc(dateTime, this);
		}
		return GetUtcOffset(dateTime, this, flags);
	}

	/// <summary>Determines whether a particular date and time in a particular time zone is ambiguous and can be mapped to two or more Coordinated Universal Time (UTC) times.</summary>
	/// <returns>true if the <paramref name="dateTimeOffset" /> parameter is ambiguous in the current time zone; otherwise, false.</returns>
	/// <param name="dateTimeOffset">A date and time.</param>
	/// <filterpriority>2</filterpriority>
	public bool IsAmbiguousTime(DateTimeOffset dateTimeOffset)
	{
		if (!_supportsDaylightSavingTime)
		{
			return false;
		}
		return IsAmbiguousTime(ConvertTime(dateTimeOffset, this).DateTime);
	}

	/// <summary>Determines whether a particular date and time in a particular time zone is ambiguous and can be mapped to two or more Coordinated Universal Time (UTC) times.</summary>
	/// <returns>true if the <paramref name="dateTime" /> parameter is ambiguous; otherwise, false.</returns>
	/// <param name="dateTime">A date and time value.   </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateTime" /> value is <see cref="F:System.DateTimeKind.Local" /> and <paramref name="dateTime" /> is an invalid time.</exception>
	/// <filterpriority>2</filterpriority>
	public bool IsAmbiguousTime(DateTime dateTime)
	{
		return IsAmbiguousTime(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
	}

	internal bool IsAmbiguousTime(DateTime dateTime, TimeZoneInfoOptions flags)
	{
		if (!_supportsDaylightSavingTime)
		{
			return false;
		}
		CachedData cachedData = s_cachedData;
		DateTime dateTime2 = ((dateTime.Kind == DateTimeKind.Local) ? ConvertTime(dateTime, cachedData.Local, this, flags, cachedData) : ((dateTime.Kind == DateTimeKind.Utc) ? ConvertTime(dateTime, s_utcTimeZone, this, flags, cachedData) : dateTime));
		int? ruleIndex;
		AdjustmentRule adjustmentRuleForTime = GetAdjustmentRuleForTime(dateTime2, out ruleIndex);
		if (adjustmentRuleForTime != null && adjustmentRuleForTime.HasDaylightSaving)
		{
			DaylightTimeStruct daylightTime = GetDaylightTime(dateTime2.Year, adjustmentRuleForTime, ruleIndex);
			return GetIsAmbiguousTime(dateTime2, adjustmentRuleForTime, daylightTime);
		}
		return false;
	}

	/// <summary>Indicates whether a specified date and time falls in the range of daylight saving time for the time zone of the current <see cref="T:System.TimeZoneInfo" /> object.</summary>
	/// <returns>true if the <paramref name="dateTimeOffset" /> parameter is a daylight saving time; otherwise, false.</returns>
	/// <param name="dateTimeOffset">A date and time value.</param>
	public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
	{
		GetUtcOffsetFromUtc(dateTimeOffset.UtcDateTime, this, out var isDaylightSavings);
		return isDaylightSavings;
	}

	/// <summary>Indicates whether a specified date and time falls in the range of daylight saving time for the time zone of the current <see cref="T:System.TimeZoneInfo" /> object.</summary>
	/// <returns>true if the <paramref name="dateTime" /> parameter is a daylight saving time; otherwise, false.</returns>
	/// <param name="dateTime">A date and time value.   </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateTime" /> value is <see cref="F:System.DateTimeKind.Local" /> and <paramref name="dateTime" /> is an invalid time.</exception>
	public bool IsDaylightSavingTime(DateTime dateTime)
	{
		return IsDaylightSavingTime(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime, s_cachedData);
	}

	internal bool IsDaylightSavingTime(DateTime dateTime, TimeZoneInfoOptions flags)
	{
		return IsDaylightSavingTime(dateTime, flags, s_cachedData);
	}

	private bool IsDaylightSavingTime(DateTime dateTime, TimeZoneInfoOptions flags, CachedData cachedData)
	{
		if (!_supportsDaylightSavingTime || _adjustmentRules == null)
		{
			return false;
		}
		DateTime dateTime2;
		if (dateTime.Kind == DateTimeKind.Local)
		{
			dateTime2 = ConvertTime(dateTime, cachedData.Local, this, flags, cachedData);
		}
		else
		{
			if (dateTime.Kind == DateTimeKind.Utc)
			{
				if (cachedData.GetCorrespondingKind(this) == DateTimeKind.Utc)
				{
					return false;
				}
				GetUtcOffsetFromUtc(dateTime, this, out var isDaylightSavings);
				return isDaylightSavings;
			}
			dateTime2 = dateTime;
		}
		int? ruleIndex;
		AdjustmentRule adjustmentRuleForTime = GetAdjustmentRuleForTime(dateTime2, out ruleIndex);
		if (adjustmentRuleForTime != null && adjustmentRuleForTime.HasDaylightSaving)
		{
			DaylightTimeStruct daylightTime = GetDaylightTime(dateTime2.Year, adjustmentRuleForTime, ruleIndex);
			return GetIsDaylightSavings(dateTime2, adjustmentRuleForTime, daylightTime, flags);
		}
		return false;
	}

	/// <summary>Indicates whether a particular date and time is invalid.</summary>
	/// <returns>true if <paramref name="dateTime" /> is invalid; otherwise, false.</returns>
	/// <param name="dateTime">A date and time value.   </param>
	/// <filterpriority>2</filterpriority>
	public bool IsInvalidTime(DateTime dateTime)
	{
		bool result = false;
		if (dateTime.Kind == DateTimeKind.Unspecified || (dateTime.Kind == DateTimeKind.Local && s_cachedData.GetCorrespondingKind(this) == DateTimeKind.Local))
		{
			int? ruleIndex;
			AdjustmentRule adjustmentRuleForTime = GetAdjustmentRuleForTime(dateTime, out ruleIndex);
			if (adjustmentRuleForTime != null && adjustmentRuleForTime.HasDaylightSaving)
			{
				DaylightTimeStruct daylightTime = GetDaylightTime(dateTime.Year, adjustmentRuleForTime, ruleIndex);
				result = GetIsInvalidTime(dateTime, adjustmentRuleForTime, daylightTime);
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	/// <summary>Clears cached time zone data.</summary>
	/// <filterpriority>2</filterpriority>
	public static void ClearCachedData()
	{
		s_cachedData = new CachedData();
	}

	/// <summary>Converts a time to the time in another time zone based on the time zone's identifier.</summary>
	/// <returns>The date and time in the destination time zone.</returns>
	/// <param name="dateTimeOffset">The date and time to convert.</param>
	/// <param name="destinationTimeZoneId">The identifier of the destination time zone.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationTimeZoneId" /> is null.</exception>
	/// <exception cref="T:System.InvalidTimeZoneException">The time zone identifier was found but the registry data is corrupted.</exception>
	/// <exception cref="T:System.Security.SecurityException">The process does not have the permissions required to read from the registry key that contains the time zone information.</exception>
	/// <exception cref="T:System.TimeZoneNotFoundException">The <paramref name="destinationTimeZoneId" /> identifier was not found on the local system.</exception>
	public static DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
	{
		return ConvertTime(dateTimeOffset, FindSystemTimeZoneById(destinationTimeZoneId));
	}

	/// <summary>Converts a time to the time in another time zone based on the time zone's identifier.</summary>
	/// <returns>The date and time in the destination time zone.</returns>
	/// <param name="dateTime">The date and time to convert.</param>
	/// <param name="destinationTimeZoneId">The identifier of the destination time zone.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationTimeZoneId" /> is null.</exception>
	/// <exception cref="T:System.InvalidTimeZoneException">The time zone identifier was found, but the registry data is corrupted.</exception>
	/// <exception cref="T:System.Security.SecurityException">The process does not have the permissions required to read from the registry key that contains the time zone information.</exception>
	/// <exception cref="T:System.TimeZoneNotFoundException">The <paramref name="destinationTimeZoneId" /> identifier was not found on the local system.</exception>
	public static DateTime ConvertTimeBySystemTimeZoneId(DateTime dateTime, string destinationTimeZoneId)
	{
		return ConvertTime(dateTime, FindSystemTimeZoneById(destinationTimeZoneId));
	}

	/// <summary>Converts a time from one time zone to another based on time zone identifiers.</summary>
	/// <returns>The date and time in the destination time zone that corresponds to the <paramref name="dateTime" /> parameter in the source time zone.</returns>
	/// <param name="dateTime">The date and time to convert.</param>
	/// <param name="sourceTimeZoneId">The identifier of the source time zone. </param>
	/// <param name="destinationTimeZoneId">The identifier of the destination time zone.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateTime" /> parameter does not correspond to the source time zone.-or-<paramref name="dateTime" /> is an invalid time in the source time zone.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceTimeZoneId" /> is null.-or-<paramref name="destinationTimeZoneId" /> is null.</exception>
	/// <exception cref="T:System.InvalidTimeZoneException">The time zone identifiers were found, but the registry data is corrupted.</exception>
	/// <exception cref="T:System.Security.SecurityException">The process does not have the permissions required to read from the registry key that contains the time zone information.</exception>
	/// <exception cref="T:System.TimeZoneNotFoundException">The <paramref name="sourceTimeZoneId" /> identifier was not found on the local system.-or-The <paramref name="destinationTimeZoneId" /> identifier was not found on the local system.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry keys that hold time zone data.</exception>
	public static DateTime ConvertTimeBySystemTimeZoneId(DateTime dateTime, string sourceTimeZoneId, string destinationTimeZoneId)
	{
		if (dateTime.Kind == DateTimeKind.Local && string.Equals(sourceTimeZoneId, Local.Id, StringComparison.OrdinalIgnoreCase))
		{
			CachedData cachedData = s_cachedData;
			return ConvertTime(dateTime, cachedData.Local, FindSystemTimeZoneById(destinationTimeZoneId), TimeZoneInfoOptions.None, cachedData);
		}
		if (dateTime.Kind == DateTimeKind.Utc && string.Equals(sourceTimeZoneId, Utc.Id, StringComparison.OrdinalIgnoreCase))
		{
			return ConvertTime(dateTime, s_utcTimeZone, FindSystemTimeZoneById(destinationTimeZoneId), TimeZoneInfoOptions.None, s_cachedData);
		}
		return ConvertTime(dateTime, FindSystemTimeZoneById(sourceTimeZoneId), FindSystemTimeZoneById(destinationTimeZoneId));
	}

	/// <summary>Converts a time to the time in a particular time zone.</summary>
	/// <returns>The date and time in the destination time zone.</returns>
	/// <param name="dateTimeOffset">The date and time to convert.   </param>
	/// <param name="destinationTimeZone">The time zone to convert <paramref name="dateTime" /> to.</param>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="destinationTimeZone" /> parameter is null.</exception>
	public static DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo destinationTimeZone)
	{
		if (destinationTimeZone == null)
		{
			throw new ArgumentNullException("destinationTimeZone");
		}
		DateTime utcDateTime = dateTimeOffset.UtcDateTime;
		TimeSpan utcOffsetFromUtc = GetUtcOffsetFromUtc(utcDateTime, destinationTimeZone);
		long num = utcDateTime.Ticks + utcOffsetFromUtc.Ticks;
		if (num <= DateTimeOffset.MaxValue.Ticks)
		{
			if (num >= DateTimeOffset.MinValue.Ticks)
			{
				return new DateTimeOffset(num, utcOffsetFromUtc);
			}
			return DateTimeOffset.MinValue;
		}
		return DateTimeOffset.MaxValue;
	}

	/// <summary>Converts a time to the time in a particular time zone.</summary>
	/// <returns>The date and time in the destination time zone.</returns>
	/// <param name="dateTime">The date and time to convert.   </param>
	/// <param name="destinationTimeZone">The time zone to convert <paramref name="dateTime" /> to.</param>
	/// <exception cref="T:System.ArgumentException">The value of the <paramref name="dateTime" /> parameter represents an invalid time.</exception>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="destinationTimeZone" /> parameter is null.</exception>
	public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo destinationTimeZone)
	{
		if (destinationTimeZone == null)
		{
			throw new ArgumentNullException("destinationTimeZone");
		}
		if (dateTime.Ticks == 0L)
		{
			ClearCachedData();
		}
		CachedData cachedData = s_cachedData;
		TimeZoneInfo sourceTimeZone = ((dateTime.Kind == DateTimeKind.Utc) ? s_utcTimeZone : cachedData.Local);
		return ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, TimeZoneInfoOptions.None, cachedData);
	}

	/// <summary>Converts a time from one time zone to another.</summary>
	/// <returns>The date and time in the destination time zone that corresponds to the <paramref name="dateTime" /> parameter in the source time zone.</returns>
	/// <param name="dateTime">The date and time to convert.</param>
	/// <param name="sourceTimeZone">The time zone of <paramref name="dateTime" />.</param>
	/// <param name="destinationTimeZone">The time zone to convert <paramref name="dateTime" /> to.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateTime" /> parameter is <see cref="F:System.DateTimeKind.Local" />, but the <paramref name="sourceTimeZone" /> parameter does not equal <see cref="F:System.DateTimeKind.Local" />. For more information, see the Remarks section. -or-The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateTime" /> parameter is <see cref="F:System.DateTimeKind.Utc" />, but the <paramref name="sourceTimeZone" /> parameter does not equal <see cref="P:System.TimeZoneInfo.Utc" />.-or-The <paramref name="dateTime" /> parameter is an invalid time (that is, it represents a time that does not exist because of a time zone's adjustment rules).</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="sourceTimeZone" /> parameter is null.-or-The <paramref name="destinationTimeZone" /> parameter is null.</exception>
	public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
	{
		return ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, TimeZoneInfoOptions.None, s_cachedData);
	}

	internal static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone, TimeZoneInfoOptions flags)
	{
		return ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, flags, s_cachedData);
	}

	private static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone, TimeZoneInfoOptions flags, CachedData cachedData)
	{
		if (sourceTimeZone == null)
		{
			throw new ArgumentNullException("sourceTimeZone");
		}
		if (destinationTimeZone == null)
		{
			throw new ArgumentNullException("destinationTimeZone");
		}
		DateTimeKind correspondingKind = cachedData.GetCorrespondingKind(sourceTimeZone);
		if ((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == 0 && dateTime.Kind != DateTimeKind.Unspecified && dateTime.Kind != correspondingKind)
		{
			throw new ArgumentException("The conversion could not be completed because the supplied DateTime did not have the Kind property set correctly.  For example, when the Kind property is DateTimeKind.Local, the source time zone must be TimeZoneInfo.Local.", "sourceTimeZone");
		}
		int? ruleIndex;
		AdjustmentRule adjustmentRuleForTime = sourceTimeZone.GetAdjustmentRuleForTime(dateTime, out ruleIndex);
		TimeSpan baseUtcOffset = sourceTimeZone.BaseUtcOffset;
		if (adjustmentRuleForTime != null)
		{
			baseUtcOffset += adjustmentRuleForTime.BaseUtcOffsetDelta;
			if (adjustmentRuleForTime.HasDaylightSaving)
			{
				bool flag = false;
				DaylightTimeStruct daylightTime = sourceTimeZone.GetDaylightTime(dateTime.Year, adjustmentRuleForTime, ruleIndex);
				if ((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == 0 && GetIsInvalidTime(dateTime, adjustmentRuleForTime, daylightTime))
				{
					throw new ArgumentException("The supplied DateTime represents an invalid time.  For example, when the clock is adjusted forward, any time in the period that is skipped is invalid.", "dateTime");
				}
				flag = GetIsDaylightSavings(dateTime, adjustmentRuleForTime, daylightTime, flags);
				baseUtcOffset += (flag ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero);
			}
		}
		DateTimeKind correspondingKind2 = cachedData.GetCorrespondingKind(destinationTimeZone);
		if (dateTime.Kind != DateTimeKind.Unspecified && correspondingKind != DateTimeKind.Unspecified && correspondingKind == correspondingKind2)
		{
			return dateTime;
		}
		bool isAmbiguousLocalDst;
		DateTime dateTime2 = ConvertUtcToTimeZone(dateTime.Ticks - baseUtcOffset.Ticks, destinationTimeZone, out isAmbiguousLocalDst);
		if (correspondingKind2 == DateTimeKind.Local)
		{
			return new DateTime(dateTime2.Ticks, DateTimeKind.Local, isAmbiguousLocalDst);
		}
		return new DateTime(dateTime2.Ticks, correspondingKind2);
	}

	/// <summary>Converts a Coordinated Universal Time (UTC) to the time in a specified time zone.</summary>
	/// <returns>The date and time in the destination time zone. Its <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Utc" /> if <paramref name="destinationTimeZone" /> is <see cref="P:System.TimeZoneInfo.Utc" />; otherwise, its <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Unspecified" />.</returns>
	/// <param name="dateTime">The Coordinated Universal Time (UTC).</param>
	/// <param name="destinationTimeZone">The time zone to convert <paramref name="dateTime" /> to.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of <paramref name="dateTime" /> is <see cref="F:System.DateTimeKind.Local" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationTimeZone" /> is null.</exception>
	public static DateTime ConvertTimeFromUtc(DateTime dateTime, TimeZoneInfo destinationTimeZone)
	{
		return ConvertTime(dateTime, s_utcTimeZone, destinationTimeZone, TimeZoneInfoOptions.None, s_cachedData);
	}

	/// <summary>Converts the current date and time to Coordinated Universal Time (UTC).</summary>
	/// <returns>The Coordinated Universal Time (UTC) that corresponds to the <paramref name="dateTime" /> parameter. The <see cref="T:System.DateTime" /> value's <see cref="P:System.DateTime.Kind" /> property is always set to <see cref="F:System.DateTimeKind.Utc" />.</returns>
	/// <param name="dateTime">The date and time to convert.</param>
	/// <exception cref="T:System.ArgumentException">TimeZoneInfo.Local.IsInvalidDateTime(<paramref name="dateTime" />) returns true.</exception>
	public static DateTime ConvertTimeToUtc(DateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKind.Utc)
		{
			return dateTime;
		}
		CachedData cachedData = s_cachedData;
		return ConvertTime(dateTime, cachedData.Local, s_utcTimeZone, TimeZoneInfoOptions.None, cachedData);
	}

	internal static DateTime ConvertTimeToUtc(DateTime dateTime, TimeZoneInfoOptions flags)
	{
		if (dateTime.Kind == DateTimeKind.Utc)
		{
			return dateTime;
		}
		CachedData cachedData = s_cachedData;
		return ConvertTime(dateTime, cachedData.Local, s_utcTimeZone, flags, cachedData);
	}

	/// <summary>Converts the time in a specified time zone to Coordinated Universal Time (UTC).</summary>
	/// <returns>The Coordinated Universal Time (UTC) that corresponds to the <paramref name="dateTime" /> parameter. The <see cref="T:System.DateTime" /> object's <see cref="P:System.DateTime.Kind" /> property is always set to <see cref="F:System.DateTimeKind.Utc" />.</returns>
	/// <param name="dateTime">The date and time to convert.</param>
	/// <param name="sourceTimeZone">The time zone of <paramref name="dateTime" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dateTime" />.Kind is <see cref="F:System.DateTimeKind.Utc" /> and <paramref name="sourceTimeZone" /> does not equal <see cref="P:System.TimeZoneInfo.Utc" />.-or-<paramref name="dateTime" />.Kind is <see cref="F:System.DateTimeKind.Local" /> and <paramref name="sourceTimeZone" /> does not equal <see cref="P:System.TimeZoneInfo.Local" />.-or-<paramref name="sourceTimeZone" />.IsInvalidDateTime(<paramref name="dateTime" />) returns true.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceTimeZone" /> is null.</exception>
	public static DateTime ConvertTimeToUtc(DateTime dateTime, TimeZoneInfo sourceTimeZone)
	{
		return ConvertTime(dateTime, sourceTimeZone, s_utcTimeZone, TimeZoneInfoOptions.None, s_cachedData);
	}

	/// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo" /> object and another <see cref="T:System.TimeZoneInfo" /> object are equal.</summary>
	/// <returns>true if the two <see cref="T:System.TimeZoneInfo" /> objects are equal; otherwise, false.</returns>
	/// <param name="other">A second object to compare with the current object.  </param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(TimeZoneInfo other)
	{
		if (other != null && string.Equals(_id, other._id, StringComparison.OrdinalIgnoreCase))
		{
			return HasSameRules(other);
		}
		return false;
	}

	/// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo" /> object and another object are equal.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.TimeZoneInfo" /> object that is equal to the current instance; otherwise, false.</returns>
	/// <param name="obj">A second object to compare with the current object.  </param>
	public override bool Equals(object obj)
	{
		return Equals(obj as TimeZoneInfo);
	}

	/// <summary>Deserializes a string to re-create an original serialized <see cref="T:System.TimeZoneInfo" /> object.</summary>
	/// <returns>The original serialized object.</returns>
	/// <param name="source">The string representation of the serialized <see cref="T:System.TimeZoneInfo" /> object.   </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> parameter is <see cref="F:System.String.Empty" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="source" /> parameter is a null string.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The source parameter cannot be deserialized back into a <see cref="T:System.TimeZoneInfo" /> object.</exception>
	public static TimeZoneInfo FromSerializedString(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.Length == 0)
		{
			throw new ArgumentException(SR.Format("The specified serialized string '{0}' is not supported.", source), "source");
		}
		return StringSerializer.GetDeserializedTimeZoneInfo(source);
	}

	/// <summary>Serves as a hash function for hashing algorithms and data structures such as hash tables.</summary>
	/// <returns>A 32-bit signed integer that serves as the hash code for this <see cref="T:System.TimeZoneInfo" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(_id);
	}

	/// <summary>Returns a sorted collection of all the time zones about which information is available on the local system.</summary>
	/// <returns>A read-only collection of <see cref="T:System.TimeZoneInfo" /> objects.</returns>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to store all time zone information.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to read from the registry keys that contain time zone information.</exception>
	public static ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
	{
		CachedData cachedData = s_cachedData;
		lock (cachedData)
		{
			if (cachedData._readOnlySystemTimeZones == null)
			{
				PopulateAllSystemTimeZones(cachedData);
				cachedData._allSystemTimeZonesRead = true;
				List<TimeZoneInfo> list = ((cachedData._systemTimeZones == null) ? new List<TimeZoneInfo>() : new List<TimeZoneInfo>(cachedData._systemTimeZones.Values));
				list.Sort(delegate(TimeZoneInfo x, TimeZoneInfo y)
				{
					int num = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
					return (num != 0) ? num : string.CompareOrdinal(x.DisplayName, y.DisplayName);
				});
				cachedData._readOnlySystemTimeZones = new ReadOnlyCollection<TimeZoneInfo>(list);
			}
		}
		return cachedData._readOnlySystemTimeZones;
	}

	/// <summary>Indicates whether the current object and another <see cref="T:System.TimeZoneInfo" /> object have the same adjustment rules.</summary>
	/// <returns>true if the two time zones have identical adjustment rules and an identical base offset; otherwise, false.</returns>
	/// <param name="other">A second object to compare with the current <see cref="T:System.TimeZoneInfo" /> object.   </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="other" /> parameter is null.</exception>
	public bool HasSameRules(TimeZoneInfo other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (_baseUtcOffset != other._baseUtcOffset || _supportsDaylightSavingTime != other._supportsDaylightSavingTime)
		{
			return false;
		}
		AdjustmentRule[] adjustmentRules = _adjustmentRules;
		AdjustmentRule[] adjustmentRules2 = other._adjustmentRules;
		bool flag = (adjustmentRules == null && adjustmentRules2 == null) || (adjustmentRules != null && adjustmentRules2 != null);
		if (!flag)
		{
			return false;
		}
		if (adjustmentRules != null)
		{
			if (adjustmentRules.Length != adjustmentRules2.Length)
			{
				return false;
			}
			for (int i = 0; i < adjustmentRules.Length; i++)
			{
				if (!adjustmentRules[i].Equals(adjustmentRules2[i]))
				{
					return false;
				}
			}
		}
		return flag;
	}

	/// <summary>Converts the current <see cref="T:System.TimeZoneInfo" /> object to a serialized string.</summary>
	/// <returns>A string that represents the current <see cref="T:System.TimeZoneInfo" /> object.</returns>
	public string ToSerializedString()
	{
		return StringSerializer.GetSerializedString(this);
	}

	/// <summary>Returns the current <see cref="T:System.TimeZoneInfo" /> object's display name.</summary>
	/// <returns>The value of the <see cref="P:System.TimeZoneInfo.DisplayName" /> property of the current <see cref="T:System.TimeZoneInfo" /> object.</returns>
	public override string ToString()
	{
		return DisplayName;
	}

	private TimeZoneInfo(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, AdjustmentRule[] adjustmentRules, bool disableDaylightSavingTime)
	{
		ValidateTimeZoneInfo(id, baseUtcOffset, adjustmentRules, out var adjustmentRulesSupportDst);
		_id = id;
		_baseUtcOffset = baseUtcOffset;
		_displayName = displayName;
		_standardDisplayName = standardDisplayName;
		_daylightDisplayName = (disableDaylightSavingTime ? null : daylightDisplayName);
		_supportsDaylightSavingTime = adjustmentRulesSupportDst && !disableDaylightSavingTime;
		_adjustmentRules = adjustmentRules;
	}

	/// <summary>Creates a custom time zone with a specified identifier, an offset from Coordinated Universal Time (UTC), a display name, and a standard time display name.</summary>
	/// <returns>The new time zone.</returns>
	/// <param name="id">The time zone's identifier.</param>
	/// <param name="baseUtcOffset">An object that represents the time difference between this time zone and Coordinated Universal Time (UTC).</param>
	/// <param name="displayName">The display name of the new time zone.   </param>
	/// <param name="standardDisplayName">The name of the new time zone's standard time.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="id" /> parameter is an empty string ("").-or-The <paramref name="baseUtcOffset" /> parameter does not represent a whole number of minutes.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="baseUtcOffset" /> parameter is greater than 14 hours or less than -14 hours.</exception>
	/// <filterpriority>2</filterpriority>
	public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName)
	{
		return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, standardDisplayName, null, disableDaylightSavingTime: false);
	}

	/// <summary>Creates a custom time zone with a specified identifier, an offset from Coordinated Universal Time (UTC), a display name, a standard time name, a daylight saving time name, and daylight saving time rules.</summary>
	/// <returns>A <see cref="T:System.TimeZoneInfo" /> object that represents the new time zone.</returns>
	/// <param name="id">The time zone's identifier.</param>
	/// <param name="baseUtcOffset">An object that represents the time difference between this time zone and Coordinated Universal Time (UTC).</param>
	/// <param name="displayName">The display name of the new time zone.   </param>
	/// <param name="standardDisplayName">The new time zone's standard time name.</param>
	/// <param name="daylightDisplayName">The daylight saving time name of the new time zone.   </param>
	/// <param name="adjustmentRules">An array that augments the base UTC offset for a particular period. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="id" /> parameter is an empty string ("").-or-The <paramref name="baseUtcOffset" /> parameter does not represent a whole number of minutes.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="baseUtcOffset" /> parameter is greater than 14 hours or less than -14 hours.</exception>
	/// <exception cref="T:System.InvalidTimeZoneException">The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter overlap.-or-The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter are not in chronological order.-or-One or more elements in <paramref name="adjustmentRules" /> are null.-or-A date can have multiple adjustment rules applied to it.-or-The sum of the <paramref name="baseUtcOffset" /> parameter and the <see cref="P:System.TimeZoneInfo.AdjustmentRule.DaylightDelta" /> value of one or more objects in the <paramref name="adjustmentRules" /> array is greater than 14 hours or less than -14 hours.</exception>
	/// <filterpriority>2</filterpriority>
	public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, AdjustmentRule[] adjustmentRules)
	{
		return CreateCustomTimeZone(id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, disableDaylightSavingTime: false);
	}

	/// <summary>Creates a custom time zone with a specified identifier, an offset from Coordinated Universal Time (UTC), a display name, a standard time name, a daylight saving time name, daylight saving time rules, and a value that indicates whether the returned object reflects daylight saving time information.</summary>
	/// <returns>The new time zone. If the <paramref name="disableDaylightSavingTime" /> parameter is true, the returned object has no daylight saving time data.</returns>
	/// <param name="id">The time zone's identifier.</param>
	/// <param name="baseUtcOffset">A <see cref="T:System.TimeSpan" /> object that represents the time difference between this time zone and Coordinated Universal Time (UTC).</param>
	/// <param name="displayName">The display name of the new time zone.   </param>
	/// <param name="standardDisplayName">The standard time name of the new time zone.</param>
	/// <param name="daylightDisplayName">The daylight saving time name of the new time zone.   </param>
	/// <param name="adjustmentRules">An array of <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> objects that augment the base UTC offset for a particular period.</param>
	/// <param name="disableDaylightSavingTime">true to discard any daylight saving time-related information present in <paramref name="adjustmentRules" /> with the new object; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="id" /> parameter is an empty string ("").-or-The <paramref name="baseUtcOffset" /> parameter does not represent a whole number of minutes.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="baseUtcOffset" /> parameter is greater than 14 hours or less than -14 hours.</exception>
	/// <exception cref="T:System.InvalidTimeZoneException">The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter overlap.-or-The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter are not in chronological order.-or-One or more elements in <paramref name="adjustmentRules" /> are null.-or-A date can have multiple adjustment rules applied to it.-or-The sum of the <paramref name="baseUtcOffset" /> parameter and the <see cref="P:System.TimeZoneInfo.AdjustmentRule.DaylightDelta" /> value of one or more objects in the <paramref name="adjustmentRules" /> array is greater than 14 hours or less than -14 hours.</exception>
	/// <filterpriority>2</filterpriority>
	public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, AdjustmentRule[] adjustmentRules, bool disableDaylightSavingTime)
	{
		if (!disableDaylightSavingTime && adjustmentRules != null && adjustmentRules.Length != 0)
		{
			adjustmentRules = (AdjustmentRule[])adjustmentRules.Clone();
		}
		return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, disableDaylightSavingTime);
	}

	/// <summary>Runs when the deserialization of an object has been completed.</summary>
	/// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.TimeZoneInfo" /> object contains invalid or corrupted data.</exception>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		try
		{
			ValidateTimeZoneInfo(_id, _baseUtcOffset, _adjustmentRules, out var adjustmentRulesSupportDst);
			if (adjustmentRulesSupportDst != _supportsDaylightSavingTime)
			{
				throw new SerializationException(SR.Format("The value of the field '{0}' is invalid.  The serialized data is corrupt.", "SupportsDaylightSavingTime"));
			}
		}
		catch (ArgumentException innerException)
		{
			throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException);
		}
		catch (InvalidTimeZoneException innerException2)
		{
			throw new SerializationException("An error occurred while deserializing the object.  The serialized data is corrupt.", innerException2);
		}
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data needed to serialize the current <see cref="T:System.TimeZoneInfo" /> object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object to populate with data.</param>
	/// <param name="context">The destination for this serialization (see <see cref="T:System.Runtime.Serialization.StreamingContext" />).</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null.</exception>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("Id", _id);
		info.AddValue("DisplayName", _displayName);
		info.AddValue("StandardName", _standardDisplayName);
		info.AddValue("DaylightName", _daylightDisplayName);
		info.AddValue("BaseUtcOffset", _baseUtcOffset);
		info.AddValue("AdjustmentRules", _adjustmentRules);
		info.AddValue("SupportsDaylightSavingTime", _supportsDaylightSavingTime);
	}

	private TimeZoneInfo(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		_id = (string)info.GetValue("Id", typeof(string));
		_displayName = (string)info.GetValue("DisplayName", typeof(string));
		_standardDisplayName = (string)info.GetValue("StandardName", typeof(string));
		_daylightDisplayName = (string)info.GetValue("DaylightName", typeof(string));
		_baseUtcOffset = (TimeSpan)info.GetValue("BaseUtcOffset", typeof(TimeSpan));
		_adjustmentRules = (AdjustmentRule[])info.GetValue("AdjustmentRules", typeof(AdjustmentRule[]));
		_supportsDaylightSavingTime = (bool)info.GetValue("SupportsDaylightSavingTime", typeof(bool));
	}

	private AdjustmentRule GetAdjustmentRuleForTime(DateTime dateTime, out int? ruleIndex)
	{
		return GetAdjustmentRuleForTime(dateTime, dateTimeisUtc: false, out ruleIndex);
	}

	private AdjustmentRule GetAdjustmentRuleForTime(DateTime dateTime, bool dateTimeisUtc, out int? ruleIndex)
	{
		if (_adjustmentRules == null || _adjustmentRules.Length == 0)
		{
			ruleIndex = null;
			return null;
		}
		DateTime dateOnly = (dateTimeisUtc ? (dateTime + BaseUtcOffset).Date : dateTime.Date);
		int num = 0;
		int num2 = _adjustmentRules.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			AdjustmentRule adjustmentRule = _adjustmentRules[num3];
			AdjustmentRule previousRule = ((num3 > 0) ? _adjustmentRules[num3 - 1] : adjustmentRule);
			int num4 = CompareAdjustmentRuleToDateTime(adjustmentRule, previousRule, dateTime, dateOnly, dateTimeisUtc);
			if (num4 == 0)
			{
				ruleIndex = num3;
				return adjustmentRule;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		ruleIndex = null;
		return null;
	}

	private int CompareAdjustmentRuleToDateTime(AdjustmentRule rule, AdjustmentRule previousRule, DateTime dateTime, DateTime dateOnly, bool dateTimeisUtc)
	{
		if (!((rule.DateStart.Kind != DateTimeKind.Utc) ? (dateOnly >= rule.DateStart) : ((dateTimeisUtc ? dateTime : ConvertToUtc(dateTime, previousRule.DaylightDelta, previousRule.BaseUtcOffsetDelta)) >= rule.DateStart)))
		{
			return 1;
		}
		if (!((rule.DateEnd.Kind != DateTimeKind.Utc) ? (dateOnly <= rule.DateEnd) : ((dateTimeisUtc ? dateTime : ConvertToUtc(dateTime, rule.DaylightDelta, rule.BaseUtcOffsetDelta)) <= rule.DateEnd)))
		{
			return -1;
		}
		return 0;
	}

	private DateTime ConvertToUtc(DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta)
	{
		return ConvertToFromUtc(dateTime, daylightDelta, baseUtcOffsetDelta, convertToUtc: true);
	}

	private DateTime ConvertFromUtc(DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta)
	{
		return ConvertToFromUtc(dateTime, daylightDelta, baseUtcOffsetDelta, convertToUtc: false);
	}

	private DateTime ConvertToFromUtc(DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta, bool convertToUtc)
	{
		TimeSpan timeSpan = BaseUtcOffset + daylightDelta + baseUtcOffsetDelta;
		if (convertToUtc)
		{
			timeSpan = timeSpan.Negate();
		}
		long num = dateTime.Ticks + timeSpan.Ticks;
		if (num <= DateTime.MaxValue.Ticks)
		{
			if (num >= DateTime.MinValue.Ticks)
			{
				return new DateTime(num);
			}
			return DateTime.MinValue;
		}
		return DateTime.MaxValue;
	}

	private static DateTime ConvertUtcToTimeZone(long ticks, TimeZoneInfo destinationTimeZone, out bool isAmbiguousLocalDst)
	{
		ticks += GetUtcOffsetFromUtc((ticks > DateTime.MaxValue.Ticks) ? DateTime.MaxValue : ((ticks < DateTime.MinValue.Ticks) ? DateTime.MinValue : new DateTime(ticks)), destinationTimeZone, out isAmbiguousLocalDst).Ticks;
		if (ticks <= DateTime.MaxValue.Ticks)
		{
			if (ticks >= DateTime.MinValue.Ticks)
			{
				return new DateTime(ticks);
			}
			return DateTime.MinValue;
		}
		return DateTime.MaxValue;
	}

	private DaylightTimeStruct GetDaylightTime(int year, AdjustmentRule rule, int? ruleIndex)
	{
		TimeSpan daylightDelta = rule.DaylightDelta;
		DateTime start;
		DateTime end;
		if (rule.NoDaylightTransitions)
		{
			AdjustmentRule previousAdjustmentRule = GetPreviousAdjustmentRule(rule, ruleIndex);
			start = ConvertFromUtc(rule.DateStart, previousAdjustmentRule.DaylightDelta, previousAdjustmentRule.BaseUtcOffsetDelta);
			end = ConvertFromUtc(rule.DateEnd, rule.DaylightDelta, rule.BaseUtcOffsetDelta);
		}
		else
		{
			start = TransitionTimeToDateTime(year, rule.DaylightTransitionStart);
			end = TransitionTimeToDateTime(year, rule.DaylightTransitionEnd);
		}
		return new DaylightTimeStruct(start, end, daylightDelta);
	}

	private static bool GetIsDaylightSavings(DateTime time, AdjustmentRule rule, DaylightTimeStruct daylightTime, TimeZoneInfoOptions flags)
	{
		if (rule == null)
		{
			return false;
		}
		DateTime startTime;
		DateTime endTime;
		if (time.Kind == DateTimeKind.Local)
		{
			startTime = (rule.IsStartDateMarkerForBeginningOfYear() ? new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) : (daylightTime.Start + daylightTime.Delta));
			endTime = (rule.IsEndDateMarkerForEndOfYear() ? new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1L) : daylightTime.End);
		}
		else
		{
			bool flag = rule.DaylightDelta > TimeSpan.Zero;
			startTime = (rule.IsStartDateMarkerForBeginningOfYear() ? new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) : (daylightTime.Start + (flag ? rule.DaylightDelta : TimeSpan.Zero)));
			endTime = (rule.IsEndDateMarkerForEndOfYear() ? new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1L) : (daylightTime.End + (flag ? (-rule.DaylightDelta) : TimeSpan.Zero)));
		}
		bool flag2 = CheckIsDst(startTime, time, endTime, ignoreYearAdjustment: false, rule);
		if (flag2 && time.Kind == DateTimeKind.Local && GetIsAmbiguousTime(time, rule, daylightTime))
		{
			flag2 = time.IsAmbiguousDaylightSavingTime();
		}
		return flag2;
	}

	private TimeSpan GetDaylightSavingsStartOffsetFromUtc(TimeSpan baseUtcOffset, AdjustmentRule rule, int? ruleIndex)
	{
		if (rule.NoDaylightTransitions)
		{
			AdjustmentRule previousAdjustmentRule = GetPreviousAdjustmentRule(rule, ruleIndex);
			return baseUtcOffset + previousAdjustmentRule.BaseUtcOffsetDelta + previousAdjustmentRule.DaylightDelta;
		}
		return baseUtcOffset + rule.BaseUtcOffsetDelta;
	}

	private TimeSpan GetDaylightSavingsEndOffsetFromUtc(TimeSpan baseUtcOffset, AdjustmentRule rule)
	{
		return baseUtcOffset + rule.BaseUtcOffsetDelta + rule.DaylightDelta;
	}

	private static bool GetIsDaylightSavingsFromUtc(DateTime time, int year, TimeSpan utc, AdjustmentRule rule, int? ruleIndex, out bool isAmbiguousLocalDst, TimeZoneInfo zone)
	{
		isAmbiguousLocalDst = false;
		if (rule == null)
		{
			return false;
		}
		DaylightTimeStruct daylightTime = zone.GetDaylightTime(year, rule, ruleIndex);
		bool ignoreYearAdjustment = false;
		TimeSpan daylightSavingsStartOffsetFromUtc = zone.GetDaylightSavingsStartOffsetFromUtc(utc, rule, ruleIndex);
		DateTime dateTime;
		if (rule.IsStartDateMarkerForBeginningOfYear() && daylightTime.Start.Year > DateTime.MinValue.Year)
		{
			int? ruleIndex2;
			AdjustmentRule adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(new DateTime(daylightTime.Start.Year - 1, 12, 31), out ruleIndex2);
			if (adjustmentRuleForTime != null && adjustmentRuleForTime.IsEndDateMarkerForEndOfYear())
			{
				dateTime = zone.GetDaylightTime(daylightTime.Start.Year - 1, adjustmentRuleForTime, ruleIndex2).Start - utc - adjustmentRuleForTime.BaseUtcOffsetDelta;
				ignoreYearAdjustment = true;
			}
			else
			{
				dateTime = new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) - daylightSavingsStartOffsetFromUtc;
			}
		}
		else
		{
			dateTime = daylightTime.Start - daylightSavingsStartOffsetFromUtc;
		}
		TimeSpan daylightSavingsEndOffsetFromUtc = zone.GetDaylightSavingsEndOffsetFromUtc(utc, rule);
		DateTime dateTime2;
		if (rule.IsEndDateMarkerForEndOfYear() && daylightTime.End.Year < DateTime.MaxValue.Year)
		{
			int? ruleIndex3;
			AdjustmentRule adjustmentRuleForTime2 = zone.GetAdjustmentRuleForTime(new DateTime(daylightTime.End.Year + 1, 1, 1), out ruleIndex3);
			if (adjustmentRuleForTime2 != null && adjustmentRuleForTime2.IsStartDateMarkerForBeginningOfYear())
			{
				dateTime2 = ((!adjustmentRuleForTime2.IsEndDateMarkerForEndOfYear()) ? (zone.GetDaylightTime(daylightTime.End.Year + 1, adjustmentRuleForTime2, ruleIndex3).End - utc - adjustmentRuleForTime2.BaseUtcOffsetDelta - adjustmentRuleForTime2.DaylightDelta) : (new DateTime(daylightTime.End.Year + 1, 12, 31) - utc - adjustmentRuleForTime2.BaseUtcOffsetDelta - adjustmentRuleForTime2.DaylightDelta));
				ignoreYearAdjustment = true;
			}
			else
			{
				dateTime2 = new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1L) - daylightSavingsEndOffsetFromUtc;
			}
		}
		else
		{
			dateTime2 = daylightTime.End - daylightSavingsEndOffsetFromUtc;
		}
		DateTime dateTime3;
		DateTime dateTime4;
		if (daylightTime.Delta.Ticks > 0)
		{
			dateTime3 = dateTime2 - daylightTime.Delta;
			dateTime4 = dateTime2;
		}
		else
		{
			dateTime3 = dateTime;
			dateTime4 = dateTime - daylightTime.Delta;
		}
		bool flag = CheckIsDst(dateTime, time, dateTime2, ignoreYearAdjustment, rule);
		if (flag)
		{
			isAmbiguousLocalDst = time >= dateTime3 && time < dateTime4;
			if (!isAmbiguousLocalDst && dateTime3.Year != dateTime4.Year)
			{
				try
				{
					dateTime3.AddYears(1);
					dateTime4.AddYears(1);
					isAmbiguousLocalDst = time >= dateTime3 && time < dateTime4;
				}
				catch (ArgumentOutOfRangeException)
				{
				}
				if (!isAmbiguousLocalDst)
				{
					try
					{
						dateTime3.AddYears(-1);
						dateTime4.AddYears(-1);
						isAmbiguousLocalDst = time >= dateTime3 && time < dateTime4;
					}
					catch (ArgumentOutOfRangeException)
					{
					}
				}
			}
		}
		return flag;
	}

	private static bool CheckIsDst(DateTime startTime, DateTime time, DateTime endTime, bool ignoreYearAdjustment, AdjustmentRule rule)
	{
		if (!ignoreYearAdjustment && !rule.NoDaylightTransitions)
		{
			int year = startTime.Year;
			int year2 = endTime.Year;
			if (year != year2)
			{
				endTime = endTime.AddYears(year - year2);
			}
			int year3 = time.Year;
			if (year != year3)
			{
				time = time.AddYears(year - year3);
			}
		}
		if (startTime > endTime)
		{
			if (!(time < endTime))
			{
				return time >= startTime;
			}
			return true;
		}
		if (rule.NoDaylightTransitions)
		{
			if (time >= startTime)
			{
				return time <= endTime;
			}
			return false;
		}
		if (time >= startTime)
		{
			return time < endTime;
		}
		return false;
	}

	private static bool GetIsAmbiguousTime(DateTime time, AdjustmentRule rule, DaylightTimeStruct daylightTime)
	{
		bool result = false;
		if (rule == null || rule.DaylightDelta == TimeSpan.Zero)
		{
			return result;
		}
		DateTime dateTime;
		DateTime dateTime2;
		if (rule.DaylightDelta > TimeSpan.Zero)
		{
			if (rule.IsEndDateMarkerForEndOfYear())
			{
				return false;
			}
			dateTime = daylightTime.End;
			dateTime2 = daylightTime.End - rule.DaylightDelta;
		}
		else
		{
			if (rule.IsStartDateMarkerForBeginningOfYear())
			{
				return false;
			}
			dateTime = daylightTime.Start;
			dateTime2 = daylightTime.Start + rule.DaylightDelta;
		}
		result = time >= dateTime2 && time < dateTime;
		if (!result && dateTime.Year != dateTime2.Year)
		{
			try
			{
				DateTime dateTime3 = dateTime.AddYears(1);
				DateTime dateTime4 = dateTime2.AddYears(1);
				result = time >= dateTime4 && time < dateTime3;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			if (!result)
			{
				try
				{
					DateTime dateTime3 = dateTime.AddYears(-1);
					DateTime dateTime4 = dateTime2.AddYears(-1);
					result = time >= dateTime4 && time < dateTime3;
				}
				catch (ArgumentOutOfRangeException)
				{
				}
			}
		}
		return result;
	}

	private static bool GetIsInvalidTime(DateTime time, AdjustmentRule rule, DaylightTimeStruct daylightTime)
	{
		bool result = false;
		if (rule == null || rule.DaylightDelta == TimeSpan.Zero)
		{
			return result;
		}
		DateTime dateTime;
		DateTime dateTime2;
		if (rule.DaylightDelta < TimeSpan.Zero)
		{
			if (rule.IsEndDateMarkerForEndOfYear())
			{
				return false;
			}
			dateTime = daylightTime.End;
			dateTime2 = daylightTime.End - rule.DaylightDelta;
		}
		else
		{
			if (rule.IsStartDateMarkerForBeginningOfYear())
			{
				return false;
			}
			dateTime = daylightTime.Start;
			dateTime2 = daylightTime.Start + rule.DaylightDelta;
		}
		result = time >= dateTime && time < dateTime2;
		if (!result && dateTime.Year != dateTime2.Year)
		{
			try
			{
				DateTime dateTime3 = dateTime.AddYears(1);
				DateTime dateTime4 = dateTime2.AddYears(1);
				result = time >= dateTime3 && time < dateTime4;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			if (!result)
			{
				try
				{
					DateTime dateTime3 = dateTime.AddYears(-1);
					DateTime dateTime4 = dateTime2.AddYears(-1);
					result = time >= dateTime3 && time < dateTime4;
				}
				catch (ArgumentOutOfRangeException)
				{
				}
			}
		}
		return result;
	}

	private static TimeSpan GetUtcOffset(DateTime time, TimeZoneInfo zone, TimeZoneInfoOptions flags)
	{
		TimeSpan baseUtcOffset = zone.BaseUtcOffset;
		int? ruleIndex;
		AdjustmentRule adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(time, out ruleIndex);
		if (adjustmentRuleForTime != null)
		{
			baseUtcOffset += adjustmentRuleForTime.BaseUtcOffsetDelta;
			if (adjustmentRuleForTime.HasDaylightSaving)
			{
				DaylightTimeStruct daylightTime = zone.GetDaylightTime(time.Year, adjustmentRuleForTime, ruleIndex);
				bool isDaylightSavings = GetIsDaylightSavings(time, adjustmentRuleForTime, daylightTime, flags);
				baseUtcOffset += (isDaylightSavings ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero);
			}
		}
		return baseUtcOffset;
	}

	private static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone)
	{
		bool isDaylightSavings;
		return GetUtcOffsetFromUtc(time, zone, out isDaylightSavings);
	}

	private static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out bool isDaylightSavings)
	{
		bool isAmbiguousLocalDst;
		return GetUtcOffsetFromUtc(time, zone, out isDaylightSavings, out isAmbiguousLocalDst);
	}

	internal static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out bool isDaylightSavings, out bool isAmbiguousLocalDst)
	{
		isDaylightSavings = false;
		isAmbiguousLocalDst = false;
		TimeSpan baseUtcOffset = zone.BaseUtcOffset;
		AdjustmentRule adjustmentRuleForTime;
		int? ruleIndex;
		int year;
		if (time > s_maxDateOnly)
		{
			adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(DateTime.MaxValue, out ruleIndex);
			year = 9999;
		}
		else if (time < s_minDateOnly)
		{
			adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(DateTime.MinValue, out ruleIndex);
			year = 1;
		}
		else
		{
			adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(time, dateTimeisUtc: true, out ruleIndex);
			year = (time + baseUtcOffset).Year;
		}
		if (adjustmentRuleForTime != null)
		{
			baseUtcOffset += adjustmentRuleForTime.BaseUtcOffsetDelta;
			if (adjustmentRuleForTime.HasDaylightSaving)
			{
				isDaylightSavings = GetIsDaylightSavingsFromUtc(time, year, zone._baseUtcOffset, adjustmentRuleForTime, ruleIndex, out isAmbiguousLocalDst, zone);
				baseUtcOffset += (isDaylightSavings ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero);
			}
		}
		return baseUtcOffset;
	}

	internal static DateTime TransitionTimeToDateTime(int year, TransitionTime transitionTime)
	{
		DateTime timeOfDay = transitionTime.TimeOfDay;
		DateTime result;
		if (transitionTime.IsFixedDateRule)
		{
			int num = DateTime.DaysInMonth(year, transitionTime.Month);
			result = new DateTime(year, transitionTime.Month, (num < transitionTime.Day) ? num : transitionTime.Day, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
		}
		else if (transitionTime.Week <= 4)
		{
			result = new DateTime(year, transitionTime.Month, 1, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
			int dayOfWeek = (int)result.DayOfWeek;
			int num2 = (int)(transitionTime.DayOfWeek - dayOfWeek);
			if (num2 < 0)
			{
				num2 += 7;
			}
			num2 += 7 * (transitionTime.Week - 1);
			if (num2 > 0)
			{
				return result.AddDays(num2);
			}
		}
		else
		{
			int day = DateTime.DaysInMonth(year, transitionTime.Month);
			result = new DateTime(year, transitionTime.Month, day, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
			int num3 = result.DayOfWeek - transitionTime.DayOfWeek;
			if (num3 < 0)
			{
				num3 += 7;
			}
			if (num3 > 0)
			{
				return result.AddDays(-num3);
			}
		}
		return result;
	}

	private static TimeZoneInfoResult TryGetTimeZone(string id, bool dstDisabled, out TimeZoneInfo value, out Exception e, CachedData cachedData, bool alwaysFallbackToLocalMachine = false)
	{
		TimeZoneInfoResult result = TimeZoneInfoResult.Success;
		e = null;
		TimeZoneInfo value2 = null;
		if (cachedData._systemTimeZones != null && cachedData._systemTimeZones.TryGetValue(id, out value2))
		{
			if (dstDisabled && value2._supportsDaylightSavingTime)
			{
				value = CreateCustomTimeZone(value2._id, value2._baseUtcOffset, value2._displayName, value2._standardDisplayName);
			}
			else
			{
				value = new TimeZoneInfo(value2._id, value2._baseUtcOffset, value2._displayName, value2._standardDisplayName, value2._daylightDisplayName, value2._adjustmentRules, disableDaylightSavingTime: false);
			}
			return result;
		}
		if (!cachedData._allSystemTimeZonesRead || alwaysFallbackToLocalMachine)
		{
			result = TryGetTimeZoneFromLocalMachine(id, dstDisabled, out value, out e, cachedData);
		}
		else
		{
			result = TimeZoneInfoResult.TimeZoneNotFoundException;
			value = null;
		}
		return result;
	}

	private static TimeZoneInfoResult TryGetTimeZoneFromLocalMachine(string id, bool dstDisabled, out TimeZoneInfo value, out Exception e, CachedData cachedData)
	{
		TimeZoneInfo value2;
		TimeZoneInfoResult num = TryGetTimeZoneFromLocalMachine(id, out value2, out e);
		if (num == TimeZoneInfoResult.Success)
		{
			if (cachedData._systemTimeZones == null)
			{
				cachedData._systemTimeZones = new Dictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase);
			}
			if (!cachedData._systemTimeZones.ContainsKey(id))
			{
				cachedData._systemTimeZones.Add(id, value2);
			}
			if (dstDisabled && value2._supportsDaylightSavingTime)
			{
				value = CreateCustomTimeZone(value2._id, value2._baseUtcOffset, value2._displayName, value2._standardDisplayName);
				return num;
			}
			value = new TimeZoneInfo(value2._id, value2._baseUtcOffset, value2._displayName, value2._standardDisplayName, value2._daylightDisplayName, value2._adjustmentRules, disableDaylightSavingTime: false);
			return num;
		}
		value = null;
		return num;
	}

	private static void ValidateTimeZoneInfo(string id, TimeSpan baseUtcOffset, AdjustmentRule[] adjustmentRules, out bool adjustmentRulesSupportDst)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (id.Length == 0)
		{
			throw new ArgumentException(SR.Format("The specified ID parameter '{0}' is not supported.", id), "id");
		}
		if (UtcOffsetOutOfRange(baseUtcOffset))
		{
			throw new ArgumentOutOfRangeException("baseUtcOffset", "The TimeSpan parameter must be within plus or minus 14.0 hours.");
		}
		if (baseUtcOffset.Ticks % 600000000 != 0L)
		{
			throw new ArgumentException("The TimeSpan parameter cannot be specified more precisely than whole minutes.", "baseUtcOffset");
		}
		adjustmentRulesSupportDst = false;
		if (adjustmentRules == null || adjustmentRules.Length == 0)
		{
			return;
		}
		adjustmentRulesSupportDst = true;
		AdjustmentRule adjustmentRule = null;
		AdjustmentRule adjustmentRule2 = null;
		for (int i = 0; i < adjustmentRules.Length; i++)
		{
			adjustmentRule = adjustmentRule2;
			adjustmentRule2 = adjustmentRules[i];
			if (adjustmentRule2 == null)
			{
				throw new InvalidTimeZoneException("The AdjustmentRule array cannot contain null elements.");
			}
			if (!IsValidAdjustmentRuleOffest(baseUtcOffset, adjustmentRule2))
			{
				throw new InvalidTimeZoneException("The sum of the BaseUtcOffset and DaylightDelta properties must within plus or minus 14.0 hours.");
			}
			if (adjustmentRule != null && adjustmentRule2.DateStart <= adjustmentRule.DateEnd)
			{
				throw new InvalidTimeZoneException("The elements of the AdjustmentRule array must be in chronological order and must not overlap.");
			}
		}
	}

	internal static bool UtcOffsetOutOfRange(TimeSpan offset)
	{
		if (!(offset < MinOffset))
		{
			return offset > MaxOffset;
		}
		return true;
	}

	private static TimeSpan GetUtcOffset(TimeSpan baseUtcOffset, AdjustmentRule adjustmentRule)
	{
		return baseUtcOffset + adjustmentRule.BaseUtcOffsetDelta + (adjustmentRule.HasDaylightSaving ? adjustmentRule.DaylightDelta : TimeSpan.Zero);
	}

	private static bool IsValidAdjustmentRuleOffest(TimeSpan baseUtcOffset, AdjustmentRule adjustmentRule)
	{
		return !UtcOffsetOutOfRange(GetUtcOffset(baseUtcOffset, adjustmentRule));
	}

	private static void NormalizeAdjustmentRuleOffset(TimeSpan baseUtcOffset, ref AdjustmentRule adjustmentRule)
	{
		TimeSpan utcOffset = GetUtcOffset(baseUtcOffset, adjustmentRule);
		TimeSpan timeSpan = TimeSpan.Zero;
		if (utcOffset > MaxOffset)
		{
			timeSpan = MaxOffset - utcOffset;
		}
		else if (utcOffset < MinOffset)
		{
			timeSpan = MinOffset - utcOffset;
		}
		if (timeSpan != TimeSpan.Zero)
		{
			adjustmentRule = AdjustmentRule.CreateAdjustmentRule(adjustmentRule.DateStart, adjustmentRule.DateEnd, adjustmentRule.DaylightDelta, adjustmentRule.DaylightTransitionStart, adjustmentRule.DaylightTransitionEnd, adjustmentRule.BaseUtcOffsetDelta + timeSpan, adjustmentRule.NoDaylightTransitions);
		}
	}

	internal TimeZoneInfo()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
