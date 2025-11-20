using System;
using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

[Flags]
public enum BoardFilter
{
	[Display(Description = "members")]
	Members = 1,
	[Display(Description = "organization")]
	Organization = 2,
	[Display(Description = "public")]
	Public = 4,
	[Display(Description = "open")]
	Open = 8,
	[Display(Description = "closed")]
	Closed = 0x10,
	[Display(Description = "pinned")]
	Pinned = 0x20,
	[Display(Description = "unpinned")]
	Unpinned = 0x40,
	[Display(Description = "starred")]
	Starred = 0x80,
	[Display(Description = "all")]
	All = 0x80
}
