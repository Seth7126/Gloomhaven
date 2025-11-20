using System;
using System.ComponentModel.DataAnnotations;

namespace Manatee.Json.Schema;

[Flags]
public enum JsonSchemaType
{
	NotDefined = 0,
	[Display(Description = "array")]
	Array = 1,
	[Display(Description = "boolean")]
	Boolean = 2,
	[Display(Description = "integer")]
	Integer = 4,
	[Display(Description = "null")]
	Null = 8,
	[Display(Description = "number")]
	Number = 0x10,
	[Display(Description = "object")]
	Object = 0x20,
	[Display(Description = "string")]
	String = 0x40
}
