namespace Manatee.Json.Pointer;

public class PointerEvaluationResults
{
	public JsonValue? Result { get; }

	public string? Error { get; }

	internal PointerEvaluationResults(JsonValue found)
	{
		Result = found;
	}

	internal PointerEvaluationResults(string error)
	{
		Error = error;
	}
}
