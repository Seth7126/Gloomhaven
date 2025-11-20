using System;

namespace Manatee.Json.Schema;

public class DefinitionsKeyword : DefsKeyword, IEquatable<DefinitionsKeyword>
{
	public override string Name => "definitions";

	public override JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft04 | JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07;

	public override SchemaVocabulary Vocabulary => SchemaVocabularies.None;

	public bool Equals(DefinitionsKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Equals((DefsKeyword?)other);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as DefinitionsKeyword);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
