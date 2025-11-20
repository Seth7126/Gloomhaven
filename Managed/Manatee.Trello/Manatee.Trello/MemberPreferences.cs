using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class MemberPreferences : IMemberPreferences
{
	private readonly Field<bool?> _enableColorBlindMode;

	private readonly Field<int?> _minutesBetweenSummaries;

	private MemberPreferencesContext _context;

	public bool? EnableColorBlindMode
	{
		get
		{
			return _enableColorBlindMode.Value;
		}
		set
		{
			_enableColorBlindMode.Value = value;
		}
	}

	public int? MinutesBetweenSummaries
	{
		get
		{
			return _minutesBetweenSummaries.Value;
		}
		set
		{
			_minutesBetweenSummaries.Value = value;
		}
	}

	internal MemberPreferences(MemberPreferencesContext context)
	{
		_context = context;
		_enableColorBlindMode = new Field<bool?>(_context, "EnableColorBlindMode");
		_enableColorBlindMode.AddRule(NullableHasValueRule<bool>.Instance);
		_minutesBetweenSummaries = new Field<int?>(_context, "MinutesBetweenSummaries");
		_minutesBetweenSummaries.AddRule(NullableHasValueRule<int>.Instance);
	}
}
