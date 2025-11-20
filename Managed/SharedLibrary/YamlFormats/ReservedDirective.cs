using System.Collections.Generic;

namespace YamlFormats;

public class ReservedDirective : Directive
{
	public string Name;

	public List<string> Parameters = new List<string>();
}
