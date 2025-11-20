using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class IdKeywordDraft04 : IdKeyword, IEquatable<IdKeywordDraft04>
{
	public override string Name => "id";

	public override JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft04;

	public override SchemaVocabulary Vocabulary => SchemaVocabularies.None;

	[DeserializationUseOnly]
	[UsedImplicitly]
	public IdKeywordDraft04()
	{
	}

	public IdKeywordDraft04(string value)
		: base(value)
	{
	}

	public bool Equals(IdKeywordDraft04? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return base.Value == other.Value;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as IdKeywordDraft04);
	}

	public override int GetHashCode()
	{
		return base.Value?.GetHashCode() ?? 0;
	}
}
