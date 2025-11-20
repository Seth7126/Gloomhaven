using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class CustomFieldDisplayInfo : ICustomFieldDisplayInfo
{
	private readonly Field<bool?> _cardFront;

	private readonly CustomFieldDisplayInfoContext _context;

	public bool? CardFront
	{
		get
		{
			return _cardFront.Value;
		}
		set
		{
			_cardFront.Value = value;
		}
	}

	internal CustomFieldDisplayInfo(CustomFieldDisplayInfoContext context)
	{
		_context = context;
		_cardFront = new Field<bool?>(_context, "CardFront");
		_cardFront.AddRule(NullableHasValueRule<bool>.Instance);
	}
}
