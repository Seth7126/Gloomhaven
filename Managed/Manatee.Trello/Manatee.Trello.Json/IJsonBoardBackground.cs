using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonBoardBackground : IJsonCacheable, IAcceptId
{
	string BottomColor { get; set; }

	BoardBackgroundBrightness? Brightness { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Color { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Image { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	List<IJsonImagePreview> ImageScaled { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Tile { get; set; }

	string TopColor { get; set; }

	BoardBackgroundType? Type { get; set; }
}
