using System.Collections.Generic;

namespace YamlFormats;

public class Sequence : DataItem
{
	public List<DataItem> Entries = new List<DataItem>();
}
