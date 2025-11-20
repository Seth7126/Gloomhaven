using System.Collections.Generic;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class CustomFieldDisplayInfoContext : LinkedSynchronizationContext<IJsonCustomFieldDisplayInfo>
{
	static CustomFieldDisplayInfoContext()
	{
		SynchronizationContext<IJsonCustomFieldDisplayInfo>.Properties = new Dictionary<string, Property<IJsonCustomFieldDisplayInfo>> { 
		{
			"CardFront",
			new Property<IJsonCustomFieldDisplayInfo, bool?>((IJsonCustomFieldDisplayInfo d, TrelloAuthorization a) => d.CardFront, delegate(IJsonCustomFieldDisplayInfo d, bool? o)
			{
				d.CardFront = o;
			})
		} };
	}

	public CustomFieldDisplayInfoContext(TrelloAuthorization auth)
		: base(auth)
	{
	}
}
