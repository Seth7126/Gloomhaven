using UnityEngine;
using VolumetricFogAndMist;

public class FogOfWarController : MonoBehaviour
{
	[SerializeField]
	private VolumetricFog m_Fog;

	private void Start()
	{
		if (!(m_Fog == null))
		{
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Campaign:
				m_Fog.enabled = false;
				break;
			case EGameMode.Guildmaster:
				m_Fog.enabled = !PlatformLayer.Setting.DisableFogOfWar;
				break;
			}
		}
	}
}
