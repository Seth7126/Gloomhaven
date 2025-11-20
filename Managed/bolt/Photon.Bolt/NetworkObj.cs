#define DEBUG
using System;
using System.Collections.Generic;
using Photon.Bolt.Exceptions;

namespace Photon.Bolt;

public abstract class NetworkObj
{
	internal string Path;

	internal NetworkObj Root;

	internal List<NetworkObj> RootObjects;

	internal readonly NetworkObj_Meta Meta;

	internal int OffsetObjects;

	internal int OffsetStorage;

	internal int OffsetProperties;

	internal NetworkState RootState => (NetworkState)Root;

	internal List<NetworkObj> Objects => Root.RootObjects;

	internal virtual NetworkStorage Storage => Root.Storage;

	internal int this[NetworkProperty property]
	{
		get
		{
			Assert.NotNull(property);
			Assert.True(OffsetObjects >= 0);
			Assert.True(OffsetObjects < Root.Meta.CountObjects);
			Assert.Same(Root.Objects[OffsetObjects], this);
			Assert.Same(Root.Objects[OffsetObjects].Meta, property.PropertyMeta);
			Assert.Same(Root.Meta.Properties[Root.Objects[OffsetObjects].OffsetProperties + property.OffsetProperties].Property, property);
			return OffsetStorage + property.OffsetStorage;
		}
	}

	internal void Add()
	{
		Assert.True(OffsetObjects == Objects.Count);
		Objects.Add(this);
	}

	internal NetworkObj(NetworkObj_Meta meta)
	{
		Meta = meta;
	}

	internal void InitRoot()
	{
		RootObjects = new List<NetworkObj>(Meta.CountObjects);
		Path = null;
		Meta.InitObject(this, this, default(NetworkObj_Meta.Offsets));
		Assert.True(RootObjects.Count == Meta.CountObjects, "RootObjects.Count == Meta.CountObjects");
	}

	internal void Init(string path, NetworkObj parent, NetworkObj_Meta.Offsets offsets)
	{
		Path = path;
		Meta.InitObject(this, parent, offsets);
	}

	internal NetworkStorage AllocateStorage()
	{
		return Meta.AllocateStorage();
	}

	internal NetworkStorage DuplicateStorage(NetworkStorage srcStorage)
	{
		NetworkStorage networkStorage = AllocateStorage();
		networkStorage.Root = srcStorage.Root;
		networkStorage.Frame = srcStorage.Frame;
		Array.Copy(srcStorage.Values, 0, networkStorage.Values, 0, srcStorage.Values.Length);
		return networkStorage;
	}

	internal void FreeStorage(NetworkStorage storage)
	{
		Meta.FreeStorage(storage);
	}
}
