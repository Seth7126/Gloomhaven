using System;

namespace Manatee.Trello.Json;

public interface IJsonLabel : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	IJsonBoard Board { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	LabelColor? Color { get; set; }

	bool ForceNullColor { set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	[Obsolete("Trello no longer supports this feature.")]
	int? Uses { get; set; }
}
