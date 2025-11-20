using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Json.Parsing;

internal interface IJsonParser
{
	bool Handles(char c);

	bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage, bool allowExtraChars);

	bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage);

	Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token);
}
