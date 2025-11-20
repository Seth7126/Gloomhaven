namespace Platforms.ProsOrHydra;

public struct HydraProsSignInResponse
{
	public ISignInResponse Response { get; set; }

	public bool Failed { get; set; }

	public bool SuppressReconnect { get; set; }
}
