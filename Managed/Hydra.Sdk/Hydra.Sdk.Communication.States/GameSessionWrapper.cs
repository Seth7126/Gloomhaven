using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class GameSessionWrapper : IHydraSdkStateWrapper
{
	public string GameSessionId { get; private set; }

	public GameSessionWrapper(string gameSessionId)
	{
		GameSessionId = gameSessionId;
	}
}
