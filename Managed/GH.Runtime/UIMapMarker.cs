using UnityEngine;

public class UIMapMarker : UIFollowMapLocation
{
	public virtual void SetLocation(MapLocation location, Vector3 offset)
	{
		Track(location.transform, offset);
	}

	public virtual void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public virtual void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
