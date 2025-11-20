using System;
using System.Collections.Generic;

namespace Manatee.Trello;

public class ValidationException<T> : Exception
{
	public IEnumerable<string> Errors { get; }

	public ValidationException(T value, IEnumerable<string> errors)
		: base($"'{value}' is not a valid value.")
	{
		Errors = errors;
	}
}
