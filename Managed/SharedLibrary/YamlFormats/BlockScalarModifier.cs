namespace YamlFormats;

public class BlockScalarModifier
{
	public char Indent;

	public char Chomp;

	public int GetIndent()
	{
		if (Indent > '0' && Indent <= '9')
		{
			return Indent - 48;
		}
		return 1;
	}

	public ChompingMethod GetChompingMethod()
	{
		return Chomp switch
		{
			'-' => ChompingMethod.Strip, 
			'+' => ChompingMethod.Keep, 
			_ => ChompingMethod.Clip, 
		};
	}
}
