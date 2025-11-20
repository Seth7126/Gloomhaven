namespace Platforms.Social;

public class RecentPlayer
{
	private readonly string _id;

	private readonly byte[] _key;

	private readonly string _name;

	public string ID => _id;

	public byte[] Key => _key;

	public string Name => _name;

	public RecentPlayer(string id, byte[] key, string name)
	{
		_id = id;
		_key = key;
		_name = name;
	}
}
