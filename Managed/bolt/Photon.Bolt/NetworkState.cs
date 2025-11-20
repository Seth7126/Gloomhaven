#define DEBUG
using System;
using System.Collections.Generic;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

public abstract class NetworkState : NetworkObj_Root, IEntitySerializer, IState, IDisposable
{
	private class PropertyField : IDebugDrawerObjectArray
	{
		public string name;

		public string value;

		public bool show;

		private Dictionary<int, IDebugDrawerObjectArray> propertyFields;

		private readonly int hash;

		public bool IsVisible
		{
			get
			{
				return show;
			}
			set
			{
				show = value;
			}
		}

		public PropertyField(string name)
		{
			this.name = name;
			show = false;
			value = null;
			propertyFields = new Dictionary<int, IDebugDrawerObjectArray>();
			hash = ((this.name != null) ? this.name.GetHashCode() : 0);
		}

		public void AddField(PropertyField field)
		{
			propertyFields.Add(field.GetHashCode(), field);
		}

		public bool TryGet(int id, out PropertyField field)
		{
			if (propertyFields.TryGetValue(id, out var debugDrawerObjectArray))
			{
				field = debugDrawerObjectArray as PropertyField;
				return true;
			}
			field = null;
			return false;
		}

		public override int GetHashCode()
		{
			return hash;
		}

		public override bool Equals(object obj)
		{
			return obj is PropertyField && ((PropertyField)obj).name.Equals(name);
		}

		public string GetName()
		{
			return name;
		}

		public string GetValue()
		{
			return value;
		}

		public IDebugDrawerObjectArray[] GetChildren()
		{
			IDebugDrawerObjectArray[] array = new IDebugDrawerObjectArray[propertyFields.Count];
			propertyFields.Values.CopyTo(array, 0);
			return array;
		}
	}

	internal float MecanimWarningTimeout = 0f;

	internal Entity Entity;

	internal List<Animator> Animators = new List<Animator>();

	internal new NetworkState_Meta Meta;

	internal BitSet PropertyDefaultMask = new BitSet();

	internal Priority[] PropertyPriorityTemp;

	internal BoltDoubleList<NetworkStorage> Frames = new BoltDoubleList<NetworkStorage>();

	private PropertyField propertyFieldRoot;

	private readonly Dictionary<string, List<PropertyCallback>> _callbacks = new Dictionary<string, List<PropertyCallback>>();

	private readonly Dictionary<string, List<PropertyCallbackSimple>> _callbacksSimple = new Dictionary<string, List<PropertyCallbackSimple>>();

	private readonly BitSet _changedProperties = new BitSet();

	public Animator Animator => (Animators.Count > 0) ? Animators[0] : null;

	internal sealed override NetworkStorage Storage => Frames.first;

	TypeId IEntitySerializer.TypeId => Meta.TypeId;

	Animator IState.Animator => Animators[0];

	IEnumerable<Animator> IState.AllAnimators => Animators;

	internal NetworkState(NetworkState_Meta meta)
		: base(meta)
	{
		Meta = meta;
	}

	public void SetAnimator(Animator animator)
	{
		Animators.Clear();
		if ((bool)animator)
		{
			Animators.Add(animator);
		}
	}

	public void AddAnimator(Animator animator)
	{
		if ((bool)animator)
		{
			Animators.Add(animator);
		}
	}

	internal void WriteInitialPosition(Vector3 position, UdpPacket packet)
	{
		Meta.InstantiationPositionCompression.Pack(packet, position);
	}

	internal Vector3 ReadInitialPosition(UdpPacket packet)
	{
		return Meta.InstantiationPositionCompression.Read(packet);
	}

	internal void WriteInitialRotation(Quaternion rotation, UdpPacket packet)
	{
		Meta.InstantiationRotationCompression.Pack(packet, rotation);
	}

	internal Quaternion ReadInitialRotation(UdpPacket packet)
	{
		return Meta.InstantiationRotationCompression.Read(packet);
	}

	void IEntitySerializer.DebugInfo()
	{
		if (BoltNetworkInternal.DebugDrawer == null)
		{
			return;
		}
		BoltNetworkInternal.DebugDrawer.LabelBold("");
		BoltNetworkInternal.DebugDrawer.LabelBold("State Info");
		BoltNetworkInternal.DebugDrawer.LabelField("Type", Factory.GetFactory(Meta.TypeId).TypeObject);
		BoltNetworkInternal.DebugDrawer.LabelField("Type Id", Meta.TypeId);
		BoltNetworkInternal.DebugDrawer.LabelBold("");
		BoltNetworkInternal.DebugDrawer.LabelBold("State Properties");
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			NetworkPropertyInfo property = Meta.Properties[i];
			string[] paths = property.Paths;
			object value = property.Property.DebugValue(base.Objects[property.OffsetObjects], Storage);
			int[] indices = property.Indices;
			if (paths.NullOrEmpty() || indices.Length == 0)
			{
				DebugInfoProperty(i, property, value);
			}
			else
			{
				DebugInfoArrayProperty(property, value, indices);
			}
		}
		BoltNetworkInternal.DebugDrawer.DrawObjectArray(propertyFieldRoot);
	}

	private void DebugInfoArrayProperty(NetworkPropertyInfo property, object value, int[] indices)
	{
		PropertyField innerBaseField = propertyFieldRoot;
		string text = property.Paths[property.Paths.Length - 1];
		string[] array = text.Split(new char[1] { '.' });
		innerBaseField = GetOrCreateField(innerBaseField, array[0]);
		for (int i = 0; i < indices.Length; i++)
		{
			string name = array[i].Replace("[]", "[" + indices[i] + "]");
			innerBaseField = GetOrCreateField(innerBaseField, name);
		}
		PropertyField propertyField = GetOrCreateField(innerBaseField, array[^1]);
		propertyField.value = ((value != null) ? value.ToString() : "N/A");
		static PropertyField GetOrCreateField(PropertyField propertyField2, string text2)
		{
			if (!propertyField2.TryGet(text2.GetHashCode(), out var field))
			{
				field = new PropertyField(text2);
				propertyField2.AddField(field);
			}
			return field;
		}
	}

	private void DebugInfoProperty(int i, NetworkPropertyInfo property, object value)
	{
		string text = property.Property.PropertyName;
		if (!Entity.IsOwner && Entity.Source._entityChannel.TryFindProxy(Entity, out var proxy))
		{
			text = "(" + proxy.PropertyPriority[i].PropertyUpdated + ") " + text;
		}
		BoltNetworkInternal.DebugDrawer.LabelField(text, (value != null) ? value.ToString() : "N/A");
	}

	void IEntitySerializer.OnRender()
	{
		for (int i = 0; i < Meta.OnRender.Count; i++)
		{
			NetworkPropertyInfo networkPropertyInfo = Meta.OnRender[i];
			networkPropertyInfo.Property.OnRender(base.Objects[networkPropertyInfo.OffsetObjects]);
		}
	}

	void IEntitySerializer.OnInitialized()
	{
		NetworkStorage networkStorage = AllocateStorage();
		networkStorage.Frame = (Entity.IsOwner ? BoltCore.frame : (-1));
		Frames.AddLast(networkStorage);
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			NetworkPropertyInfo networkPropertyInfo = Meta.Properties[i];
			networkPropertyInfo.Property.OnInit(base.Objects[networkPropertyInfo.OffsetObjects]);
		}
	}

	void IEntitySerializer.OnCreated(Entity entity)
	{
		Entity = entity;
		propertyFieldRoot = new PropertyField(null);
	}

	void IEntitySerializer.OnParentChanging(Entity newParent, Entity oldParent)
	{
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			NetworkPropertyInfo networkPropertyInfo = Meta.Properties[i];
			networkPropertyInfo.Property.OnParentChanged(base.Objects[networkPropertyInfo.OffsetObjects], newParent, oldParent);
		}
	}

	void IEntitySerializer.OnSimulateBefore()
	{
		if (Entity.IsOwner || Entity.HasPredictedControl)
		{
			Frames.first.Frame = BoltCore.frame;
		}
		else
		{
			while (Frames.count > 1 && Entity.Frame >= Frames.Next(Frames.first).Frame)
			{
				Frames.Next(Frames.first).Combine(Frames.first);
				FreeStorage(Frames.RemoveFirst());
			}
		}
		int count = Meta.OnSimulateBefore.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				NetworkPropertyInfo networkPropertyInfo = Meta.OnSimulateBefore[i];
				networkPropertyInfo.Property.OnSimulateBefore(base.Objects[networkPropertyInfo.OffsetObjects]);
			}
		}
		InvokeCallbacks();
	}

	void IEntitySerializer.OnSimulateAfter()
	{
		int count = Meta.OnSimulateAfter.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				NetworkPropertyInfo networkPropertyInfo = Meta.OnSimulateAfter[i];
				networkPropertyInfo.Property.OnSimulateAfter(base.Objects[networkPropertyInfo.OffsetObjects]);
			}
		}
		InvokeCallbacks();
	}

	void IEntitySerializer.OnControlGained()
	{
		while (Frames.count > 1)
		{
			Frames.last.Combine(Frames.first);
			FreeStorage(Frames.RemoveFirst());
		}
	}

	void IEntitySerializer.OnControlLost()
	{
		Frames.first.Frame = Entity.Frame;
	}

	BitSet IEntitySerializer.GetDefaultMask()
	{
		return PropertyDefaultMask;
	}

	BitSet IEntitySerializer.GetFilter(BoltConnection connection, EntityProxy proxy)
	{
		if (Entity.IsController(connection))
		{
			return Meta.Filters[31];
		}
		return Meta.Filters[30];
	}

	void IEntitySerializer.InitProxy(EntityProxy p)
	{
		p.PropertyPriority = new Priority[Meta.CountProperties];
		for (int i = 0; i < p.PropertyPriority.Length; i++)
		{
			p.PropertyPriority[i].PropertyIndex = i;
		}
	}

	int IEntitySerializer.Pack(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope env)
	{
		int num = 0;
		BitSet filter = ((IEntitySerializer)this).GetFilter(connection, env.Proxy);
		Priority[] propertiesTempPriority = Meta.PropertiesTempPriority;
		Priority[] propertyPriority = env.Proxy.PropertyPriority;
		for (int i = 0; i < propertyPriority.Length; i++)
		{
			Assert.True(propertyPriority[i].PropertyIndex == i);
			if (filter.IsSet(i) && env.Proxy.IsSet(i))
			{
				propertyPriority[i].PropertyPriority += Meta.Properties[i].Property.PropertyPriority;
				propertyPriority[i].PropertyPriority = Mathf.Clamp(propertyPriority[i].PropertyPriority, 0, BoltCore._config.maxPropertyPriority);
				propertiesTempPriority[num] = propertyPriority[i];
				num++;
			}
		}
		Array.Sort(propertiesTempPriority, 0, num, Priority.Comparer.Instance);
		PackProperties(connection, stream, env, propertiesTempPriority, num);
		for (int j = 0; j < env.Written.Count; j++)
		{
			Priority priority = env.Written[j];
			env.Proxy.PropertyPriority[priority.PropertyIndex].PropertyPriority = 0;
			env.Proxy.Clear(priority.PropertyIndex);
		}
		return env.Written.Count;
	}

	private void PackProperties(BoltConnection connection, UdpPacket packet, EntityProxyEnvelope env, Priority[] priority, int count)
	{
		int position = packet.Position;
		packet.WriteByte(0, Meta.PacketMaxPropertiesBits);
		int num = System.Math.Min(Meta.PacketMaxBits, packet.Size - packet.Position);
		for (int i = 0; i < count; i++)
		{
			if (num <= Meta.PropertyIdBits)
			{
				break;
			}
			if (env.Written.Count == Meta.PacketMaxProperties)
			{
				break;
			}
			Priority item = priority[i];
			NetworkPropertyInfo networkPropertyInfo = Meta.Properties[item.PropertyIndex];
			if (item.PropertyPriority == 0)
			{
				break;
			}
			int num2 = Meta.PropertyIdBits + networkPropertyInfo.Property.BitCount(base.Objects[networkPropertyInfo.OffsetObjects]);
			int position2 = packet.Position;
			if (num < num2)
			{
				continue;
			}
			packet.WriteInt(item.PropertyIndex, Meta.PropertyIdBits);
			if (networkPropertyInfo.Property.Write(connection, base.Objects[networkPropertyInfo.OffsetObjects], Storage, packet))
			{
				int num3 = packet.Position - position2;
				if (num3 != num2)
				{
				}
				if (packet.Overflowing)
				{
					packet.Ptr = position2;
					break;
				}
				num -= num2;
				env.Written.Add(item);
			}
			else
			{
				packet.Ptr = position2;
			}
		}
		Assert.True(env.Written.Count <= Meta.PacketMaxProperties);
		UdpPacket.WriteByteAt(packet.Data, position, Meta.PacketMaxPropertiesBits, (byte)env.Written.Count);
	}

	void IEntitySerializer.Read(BoltConnection connection, UdpPacket packet, int frame)
	{
		int num = packet.ReadByte(Meta.PacketMaxPropertiesBits);
		NetworkStorage networkStorage = null;
		if (Entity.HasPredictedControl)
		{
			Assert.True(Frames.count == 1);
			networkStorage = Frames.first;
			networkStorage.Frame = BoltCore.frame;
		}
		else if (Frames.first.Frame == -1)
		{
			Assert.True(Frames.count == 1);
			networkStorage = Frames.first;
			networkStorage.Frame = frame;
		}
		else
		{
			networkStorage = DuplicateStorage(Frames.last);
			networkStorage.Frame = frame;
			networkStorage.ClearAll();
			for (int i = 0; i < Meta.OnFrameCloned.Count; i++)
			{
				NetworkPropertyInfo networkPropertyInfo = Meta.OnFrameCloned[i];
				networkPropertyInfo.Property.OnFrameCloned(base.Objects[networkPropertyInfo.OffsetObjects], networkStorage);
			}
			Frames.AddLast(networkStorage);
		}
		if (Entity.HasControl && !Entity.HasPredictedControl && !Entity.IsOwner)
		{
			for (int j = 0; j < Meta.Properties.Length; j++)
			{
				NetworkPropertyInfo networkPropertyInfo2 = Meta.Properties[j];
				if (!networkPropertyInfo2.Property.ToController)
				{
					int num2 = base.Objects[networkPropertyInfo2.OffsetObjects][networkPropertyInfo2.Property];
					networkStorage.Values[num2] = Frames.first.Values[num2];
				}
			}
		}
		while (--num >= 0)
		{
			int num3 = packet.ReadInt(Meta.PropertyIdBits);
			NetworkPropertyInfo networkPropertyInfo3 = Meta.Properties[num3];
			if (!Entity.IsOwner && Entity.Source._entityChannel.TryFindProxy(Entity, out var proxy))
			{
				proxy.PropertyPriority[num3].PropertyUpdated = frame;
			}
			Assert.True(num3 == base.Objects[networkPropertyInfo3.OffsetObjects].OffsetProperties + networkPropertyInfo3.Property.OffsetProperties);
			networkPropertyInfo3.Property.Read(connection, base.Objects[networkPropertyInfo3.OffsetObjects], networkStorage, packet);
			networkStorage.Set(num3);
		}
	}

	private bool VerifyCallbackPath(string path)
	{
		if (Meta.CallbackPaths.Contains(path))
		{
			return true;
		}
		BoltLog.Error("No callback path '{0}' available on {1}", path, this);
		return false;
	}

	void IState.AddAnimator(Animator animator)
	{
		Animators.Add(animator);
	}

	void IState.SetAnimator(Animator animator)
	{
		Animators.Clear();
		Animators.Add(animator);
	}

	void IState.AddCallback(string path, PropertyCallback callback)
	{
		if (VerifyCallbackPath(path))
		{
			if (!_callbacks.TryGetValue(path, out var value))
			{
				value = (_callbacks[path] = new List<PropertyCallback>(32));
			}
			value.Add(callback);
		}
	}

	void IState.AddCallback(string path, PropertyCallbackSimple callback)
	{
		if (VerifyCallbackPath(path))
		{
			if (!_callbacksSimple.TryGetValue(path, out var value))
			{
				value = (_callbacksSimple[path] = new List<PropertyCallbackSimple>(32));
			}
			value.Add(callback);
		}
	}

	void IState.RemoveCallback(string path, PropertyCallback callback)
	{
		if (VerifyCallbackPath(path) && _callbacks.TryGetValue(path, out var value))
		{
			value.Remove(callback);
		}
	}

	void IState.RemoveCallback(string path, PropertyCallbackSimple callback)
	{
		if (VerifyCallbackPath(path) && _callbacksSimple.TryGetValue(path, out var value))
		{
			value.Remove(callback);
		}
	}

	void IState.RemoveAllCallbacks()
	{
		_callbacks.Clear();
		_callbacksSimple.Clear();
	}

	void IState.SetDynamic(string property, object value)
	{
		int hashCode = property.GetHashCode();
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			if (Meta.Properties[i].OffsetObjects == 0 && Meta.Properties[i].Property.PropertyNameHash == hashCode && Meta.Properties[i].Property.PropertyName == property)
			{
				Meta.Properties[i].Property.SetDynamic(this, value);
				return;
			}
		}
		throw new ArgumentException($"unknown property {property}");
	}

	object IState.GetDynamic(string property)
	{
		int hashCode = property.GetHashCode();
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			if (Meta.Properties[i].OffsetObjects == 0 && Meta.Properties[i].Property.PropertyNameHash == hashCode && Meta.Properties[i].Property.PropertyName == property)
			{
				return Meta.Properties[i].Property.GetDynamic(this);
			}
		}
		throw new ArgumentException($"unknown property {property}");
	}

	void IState.SetTeleport(NetworkTransform transform)
	{
		transform.SetTeleportInternal(teleport: true);
	}

	void IState.SetTransforms(NetworkTransform transform, Transform simulate)
	{
		((IState)this).SetTransforms(transform, simulate, (Transform)null);
	}

	void IState.SetTransforms(NetworkTransform transform, Transform simulate, Transform render)
	{
		transform.SetTransformsInternal(simulate, render);
		if (Entity.AttachIsRunning && (bool)simulate)
		{
			Assert.Same(transform, Storage.Values[transform.PropertyIndex].Transform);
			if (transform.space == TransformSpaces.World)
			{
				Storage.Values[transform.PropertyIndex].Vector3 = simulate.position;
			}
			else if (transform.space == TransformSpaces.Local)
			{
				Storage.Values[transform.PropertyIndex].Vector3 = simulate.localPosition;
			}
			Storage.Values[transform.PropertyIndex + 1].Quaternion = simulate.rotation;
		}
	}

	void IState.ForceTransform(NetworkTransform transform, Vector3 position)
	{
		((IState)this).ForceTransform(transform, position, Quaternion.identity);
	}

	void IState.ForceTransform(NetworkTransform transform, Vector3 position, Quaternion rotation)
	{
		if (!Entity.IsOwner)
		{
			BoltIterator<NetworkStorage> iterator = Frames.GetIterator();
			while (iterator.Next())
			{
				iterator.val.Values[transform.PropertyIndex].Vector3 = position;
				iterator.val.Values[transform.PropertyIndex + 1].Quaternion = rotation;
			}
		}
	}

	bool IState.TrySetDynamic(string property, object value)
	{
		int hashCode = property.GetHashCode();
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			if (Meta.Properties[i].OffsetObjects == 0 && Meta.Properties[i].Property.PropertyNameHash == hashCode && Meta.Properties[i].Property.PropertyName == property)
			{
				Meta.Properties[i].Property.SetDynamic(this, value);
				return true;
			}
		}
		return false;
	}

	bool IState.TryGetDynamic(string property, out object value)
	{
		int hashCode = property.GetHashCode();
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			if (Meta.Properties[i].OffsetObjects == 0 && Meta.Properties[i].Property.PropertyNameHash == hashCode && Meta.Properties[i].Property.PropertyName == property)
			{
				value = Meta.Properties[i].Property.GetDynamic(this);
				return true;
			}
		}
		value = null;
		return false;
	}

	void IDisposable.Dispose()
	{
	}

	private void InvokeCallbacks()
	{
		_changedProperties.ClearAll();
		while (!Frames.first.IsZero)
		{
			PropertyDefaultMask.Combine(Frames.first);
			PropertyDefaultMask.Combine(_changedProperties);
			if (Entity.Proxies.count > 0)
			{
				BoltIterator<EntityProxy> iterator = Entity.Proxies.GetIterator();
				while (iterator.Next())
				{
					iterator.val.Combine(Frames.first);
					iterator.val.Combine(_changedProperties);
				}
			}
			for (int i = 0; i < 16; i++)
			{
				ulong num = Frames.first[i];
				if (num == 0)
				{
					continue;
				}
				Frames.first[i] = 0uL;
				for (int j = 0; j < 64; j++)
				{
					if ((num & (ulong)(1L << j)) != 0)
					{
						num &= (ulong)(~(1L << j));
						InvokeCallbacksForProperty(i * 64 + j);
						_changedProperties.Combine(Frames.first);
					}
				}
			}
		}
	}

	private void InvokeCallbacksForProperty(int propertyIndex)
	{
		try
		{
			NetworkPropertyInfo networkPropertyInfo = Meta.Properties[propertyIndex];
			for (int i = 0; i < networkPropertyInfo.Paths.Length; i++)
			{
				if (_callbacks.TryGetValue(networkPropertyInfo.Paths[i], out var value))
				{
					for (int j = 0; j < value.Count; j++)
					{
						value[j](this, networkPropertyInfo.Paths[networkPropertyInfo.Paths.Length - 1], new ArrayIndices(networkPropertyInfo.Indices));
					}
				}
				if (_callbacksSimple.TryGetValue(networkPropertyInfo.Paths[i], out var value2))
				{
					for (int k = 0; k < value2.Count; k++)
					{
						value2[k]();
					}
				}
			}
		}
		catch (Exception exception)
		{
			BoltLog.Exception(exception);
		}
	}
}
