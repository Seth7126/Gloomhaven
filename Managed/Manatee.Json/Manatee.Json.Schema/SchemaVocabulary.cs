using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Manatee.Json.Schema;

[DebuggerDisplay("{Id}")]
public class SchemaVocabulary : IEquatable<SchemaVocabulary>
{
	public string Id { get; }

	public string MetaSchemaId { get; }

	internal List<Type> DefinedKeywords { get; } = new List<Type>();

	public SchemaVocabulary(string id, string metaSchemaId)
	{
		Id = id;
		MetaSchemaId = metaSchemaId;
	}

	internal SchemaVocabulary(string id)
	{
		Id = id;
	}

	public bool Equals(SchemaVocabulary? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (string.Equals(Id, other.Id))
		{
			return string.Equals(MetaSchemaId, other.MetaSchemaId);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SchemaVocabulary);
	}

	public override int GetHashCode()
	{
		return (((Id != null) ? Id.GetHashCode() : 0) * 397) ^ ((MetaSchemaId != null) ? MetaSchemaId.GetHashCode() : 0);
	}
}
