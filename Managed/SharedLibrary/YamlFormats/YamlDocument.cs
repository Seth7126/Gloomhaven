using System.Collections.Generic;

namespace YamlFormats;

public class YamlDocument
{
	public Dictionary<string, DataItem> AnchoredItems = new Dictionary<string, DataItem>();

	public DataItem Root;

	public List<Directive> Directives = new List<Directive>();
}
