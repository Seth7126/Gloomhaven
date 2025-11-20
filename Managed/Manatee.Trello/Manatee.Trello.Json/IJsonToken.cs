using System;
using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonToken : IJsonCacheable
{
	[JsonSpecialSerialization]
	string TokenValue { get; set; }

	[JsonDeserialize]
	string Identifier { get; set; }

	[JsonDeserialize]
	IJsonMember Member { get; set; }

	[JsonDeserialize]
	DateTime? DateCreated { get; set; }

	[JsonDeserialize]
	DateTime? DateExpires { get; set; }

	[JsonDeserialize]
	List<IJsonTokenPermission> Permissions { get; set; }
}
