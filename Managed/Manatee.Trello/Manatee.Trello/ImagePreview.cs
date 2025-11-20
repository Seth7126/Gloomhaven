using System;
using Manatee.Trello.Internal;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ImagePreview : IImagePreview, ICacheable
{
	private DateTime? _creation;

	public DateTime CreationDate
	{
		get
		{
			if (!_creation.HasValue)
			{
				_creation = Id.ExtractCreationDate();
			}
			return _creation.Value;
		}
	}

	public int? Height { get; }

	public string Id { get; }

	public bool? IsScaled { get; set; }

	public string Url { get; }

	public int? Width { get; }

	internal ImagePreview(IJsonImagePreview json)
	{
		Id = json.Id;
		Height = json.Height;
		IsScaled = json.Scaled;
		Url = json.Url;
		Width = json.Width;
	}
}
