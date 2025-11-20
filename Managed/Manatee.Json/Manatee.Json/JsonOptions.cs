namespace Manatee.Json;

public static class JsonOptions
{
	public static string PrettyPrintIndent { get; set; } = "\t";

	public static DuplicateKeyBehavior DuplicateKeyBehavior { get; set; }

	public static ArrayEquality DefaultArrayEquality { get; set; }

	public static bool ThrowOnIncorrectTypeAccess { get; set; } = true;

	public static ILog? Log { get; set; }

	public static LogCategory LogCategory { get; set; } = LogCategory.All;
}
