using System.Linq;
using UnityEngine;

public class UIPlatformControllerActivator : MonoBehaviour
{
	[SerializeField]
	private DeviceType[] _platforms;

	public void CheckPlatform()
	{
		if (!_platforms.Any((DeviceType x) => x == PlatformLayer.Instance.GetCurrentPlatform()))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
