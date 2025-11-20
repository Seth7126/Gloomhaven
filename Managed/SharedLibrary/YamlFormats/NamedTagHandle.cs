using System.Collections.Generic;

namespace YamlFormats;

public class NamedTagHandle : TagHandle
{
	public List<char> Name = new List<char>();
}
