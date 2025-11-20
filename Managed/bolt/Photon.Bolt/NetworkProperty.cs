#define DEBUG
using System;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal abstract class NetworkProperty
{
	public int OffsetStorage;

	public int OffsetProperties;

	public int PropertyFilters;

	public int PropertyNameHash;

	public string PropertyName;

	public int PropertyPriority;

	public NetworkObj_Meta PropertyMeta;

	public bool ToProxies;

	public bool ToController;

	public PropertyInterpolationSettings Interpolation;

	public virtual bool AllowCallbacks => true;

	public virtual bool WantsOnRender => false;

	public virtual bool WantsOnSimulateAfter => false;

	public virtual bool WantsOnSimulateBefore => false;

	public virtual bool WantsOnControlGainedLost => false;

	public virtual bool WantsOnFrameCloned => false;

	public void Settings_Property(string name, int priority, int filters)
	{
		PropertyName = name;
		PropertyNameHash = name.GetHashCode();
		PropertyFilters = filters;
		PropertyPriority = Mathf.Clamp(priority, 1, 100);
		ToProxies = (filters & 0x40000000) == 1073741824;
		ToController = (filters & int.MinValue) == int.MinValue;
	}

	public void Settings_Offsets(int properties, int storage)
	{
		OffsetStorage = storage;
		OffsetProperties = properties;
	}

	public void Settings_Interpolation(float snapMagnitude, bool enabled)
	{
		Interpolation.Enabled = enabled;
		Interpolation.SnapMagnitude = snapMagnitude;
	}

	public abstract bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet);

	public abstract void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet);

	public abstract void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other);

	public abstract bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2);

	public virtual object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return "NONE";
	}

	public virtual int BitCount(NetworkObj obj)
	{
		return -1;
	}

	public virtual void SetDynamic(NetworkObj obj, object value)
	{
		throw new NotSupportedException();
	}

	public virtual object GetDynamic(NetworkObj obj)
	{
		throw new NotSupportedException();
	}

	public virtual bool SupportsDeltaCompression()
	{
		return false;
	}

	public virtual void OnInit(NetworkObj obj)
	{
	}

	public virtual void OnRender(NetworkObj obj)
	{
	}

	public virtual void OnSimulateBefore(NetworkObj obj)
	{
	}

	public virtual void OnSimulateAfter(NetworkObj obj)
	{
	}

	public virtual void OnParentChanged(NetworkObj obj, Entity newParent, Entity oldParent)
	{
	}

	public virtual void OnFrameCloned(NetworkObj obj, NetworkStorage storage)
	{
	}

	public virtual void OnControlGained(NetworkObj obj)
	{
	}

	public virtual void OnControlLost(NetworkObj obj)
	{
	}

	public virtual void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t)
	{
		BoltLog.Warn("SmoothCommandCorrection called without implementation");
	}
}
