using System;

namespace Manatee.Trello;

public interface IImagePreview : ICacheable
{
	DateTime CreationDate { get; }

	int? Height { get; }

	bool? IsScaled { get; set; }

	string Url { get; }

	int? Width { get; }
}
