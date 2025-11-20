using Mono.Cecil;

internal static class MultiTargetShims
{
	private static readonly object[] _NoArgs = new object[0];

	public static TypeReference GetConstraintType(this TypeReference type)
	{
		return type;
	}
}
