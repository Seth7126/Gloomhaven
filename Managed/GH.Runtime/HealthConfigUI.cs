using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Health Level", fileName = "HealthConfig")]
public class HealthConfigUI : ScriptableObject
{
	public int health;

	public List<HealthZoomConfigUI> zoomConfigs;

	public HealthZoomConfigUI GetZoomConfig(float zoom)
	{
		int num = -1;
		float num2 = 0f;
		for (int i = 0; i < zoomConfigs.Count; i++)
		{
			if (zoomConfigs[i].zoomMax >= zoom && (num < 0 || zoomConfigs[i].zoomMax < num2))
			{
				num = i;
				num2 = zoomConfigs[i].zoomMax;
			}
		}
		if (num >= 0)
		{
			return zoomConfigs[num];
		}
		return null;
	}
}
