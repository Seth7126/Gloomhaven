using System.Collections.Generic;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class TokenPermissionContext : LinkedSynchronizationContext<IJsonTokenPermission>
{
	static TokenPermissionContext()
	{
		SynchronizationContext<IJsonTokenPermission>.Properties = new Dictionary<string, Property<IJsonTokenPermission>>
		{
			{
				"ModelType",
				new Property<IJsonTokenPermission, TokenModelType?>((IJsonTokenPermission d, TrelloAuthorization a) => d.ModelType, delegate(IJsonTokenPermission d, TokenModelType? o)
				{
					d.ModelType = o;
				})
			},
			{
				"CanRead",
				new Property<IJsonTokenPermission, bool?>((IJsonTokenPermission d, TrelloAuthorization a) => d.Read, delegate(IJsonTokenPermission d, bool? o)
				{
					d.Read = o;
				})
			},
			{
				"CanWrite",
				new Property<IJsonTokenPermission, bool?>((IJsonTokenPermission d, TrelloAuthorization a) => d.Write, delegate(IJsonTokenPermission d, bool? o)
				{
					d.Write = o;
				})
			}
		};
	}

	public TokenPermissionContext(TrelloAuthorization auth)
		: base(auth)
	{
	}
}
