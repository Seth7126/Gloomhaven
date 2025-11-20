using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello.Internal;

internal class Field<T>
{
	private readonly Manatee.Trello.Internal.Synchronization.SynchronizationContext _context;

	private readonly List<IValidationRule<T>> _rules;

	private readonly string _property;

	public T Value
	{
		get
		{
			return CurrentValue;
		}
		set
		{
			if (!object.Equals(CurrentValue, value))
			{
				Validate(value);
				_context.SetValue(_property, value, CancellationToken.None);
			}
		}
	}

	private T CurrentValue => _context.GetValue<T>(_property);

	public Field(Manatee.Trello.Internal.Synchronization.SynchronizationContext context, string propertyName)
	{
		_context = context;
		_property = propertyName;
		_rules = new List<IValidationRule<T>>();
	}

	public void AddRule(IValidationRule<T> rule)
	{
		_rules.Add(rule);
	}

	private void Validate(T value)
	{
		List<string> list = (from r in _rules
			select r.Validate(CurrentValue, value) into s
			where s != null
			select s).ToList();
		if (list.Any())
		{
			throw new ValidationException<T>(value, list);
		}
	}
}
