using System.Collections.Generic;

namespace YamlFormats;

public class Mapping : DataItem
{
	public List<MappingEntry> Entries = new List<MappingEntry>();
}
