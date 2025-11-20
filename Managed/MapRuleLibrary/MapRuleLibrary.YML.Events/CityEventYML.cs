using System.Collections.Generic;

namespace MapRuleLibrary.YML.Events;

public class CityEventYML : RoadEventYML
{
	public CityEventYML()
	{
		base.LoadedYML = new List<CRoadEvent>();
	}
}
