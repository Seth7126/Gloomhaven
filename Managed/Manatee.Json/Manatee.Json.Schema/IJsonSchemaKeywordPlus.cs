using System;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

internal interface IJsonSchemaKeywordPlus : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>
{
	bool Handles(JsonValue value);
}
