using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonSticker : IJsonCacheable, IAcceptId
{
	[JsonSerialize]
	[JsonDeserialize]
	double? Left { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	List<IJsonImagePreview> Previews { get; set; }

	[JsonSerialize]
	[JsonDeserialize]
	int? Rotation { get; set; }

	[JsonSerialize]
	[JsonDeserialize]
	double? Top { get; set; }

	[JsonDeserialize]
	string Url { get; set; }

	[JsonSerialize]
	[JsonDeserialize]
	int? ZIndex { get; set; }
}
