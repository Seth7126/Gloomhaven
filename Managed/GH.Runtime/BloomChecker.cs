using UnityEngine;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Bloom))]
public class BloomChecker : MonoBehaviour
{
	private void Awake()
	{
		if (PlatformLayer.Setting.TurnOffUICameraBloom)
		{
			GetComponent<Bloom>().enabled = false;
		}
	}
}
