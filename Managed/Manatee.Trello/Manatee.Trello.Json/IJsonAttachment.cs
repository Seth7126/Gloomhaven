using System;
using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonAttachment : IJsonCacheable
{
	[JsonDeserialize]
	int? Bytes { get; set; }

	[JsonDeserialize]
	DateTime? Date { get; set; }

	[JsonSerialize]
	[JsonDeserialize]
	string EdgeColor { get; set; }

	[JsonDeserialize]
	IJsonMember Member { get; set; }

	[JsonDeserialize]
	bool? IsUpload { get; set; }

	[JsonDeserialize]
	string MimeType { get; set; }

	[JsonDeserialize]
	string Name { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonPosition Pos { get; set; }

	[JsonDeserialize]
	List<IJsonImagePreview> Previews { get; set; }

	[JsonDeserialize]
	string Url { get; set; }
}
