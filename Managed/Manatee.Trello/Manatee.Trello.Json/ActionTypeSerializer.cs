using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json;

internal class ActionTypeSerializer : Manatee.Json.Serialization.ISerializer
{
	public bool ShouldMaintainReferences => false;

	public bool Handles(SerializationContextBase context)
	{
		return context.InferredType == typeof(ActionType);
	}

	public JsonValue Serialize(SerializationContext context)
	{
		return context.Source?.ToString();
	}

	public object Deserialize(DeserializationContext context)
	{
		if (!ActionType.TryParse(context.LocalValue.String, out var result))
		{
			throw new TrelloInteractionException("ActionType '" + context.LocalValue.String + "' is not a recognized value.  It may be a new value that Trello introduced since this version of Manatee.Trello.  Please report it by logging an issue in GitHub or contacting me via my Slack workspace.");
		}
		return result;
	}
}
