namespace Steamworks.Data;

public struct GameId
{
	public ulong Value;

	public static implicit operator GameId(ulong value)
	{
		return new GameId
		{
			Value = value
		};
	}

	public static implicit operator ulong(GameId value)
	{
		return value.Value;
	}
}
