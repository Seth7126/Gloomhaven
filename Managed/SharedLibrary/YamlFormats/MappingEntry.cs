namespace YamlFormats;

public class MappingEntry
{
	public DataItem Key;

	public DataItem Value;

	public override string ToString()
	{
		return $"{{Key:{Key}, Value:{Value}}}";
	}
}
