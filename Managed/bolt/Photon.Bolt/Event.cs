using Photon.Bolt.Internal;
using UdpKit;

namespace Photon.Bolt;

[Documentation]
public abstract class Event : NetworkObj_Root
{
	internal const byte ENTITY_EVERYONE = 1;

	internal const byte ENTITY_EVERYONE_EXCEPT_OWNER = 3;

	internal const byte ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER = 13;

	internal const byte ENTITY_EVERYONE_EXCEPT_CONTROLLER = 5;

	internal const byte ENTITY_ONLY_CONTROLLER = 7;

	internal const byte ENTITY_ONLY_CONTROLLER_AND_OWNER = 15;

	internal const byte ENTITY_ONLY_OWNER = 9;

	internal const byte ENTITY_ONLY_SELF = 11;

	internal const byte GLOBAL_EVERYONE = 2;

	internal const byte GLOBAL_OTHERS = 4;

	internal const byte GLOBAL_ONLY_SERVER = 6;

	internal const byte GLOBAL_ALL_CLIENTS = 8;

	internal const byte GLOBAL_SPECIFIC_CONNECTION = 10;

	internal const byte GLOBAL_ONLY_SELF = 12;

	internal const int RELIABLE_WINDOW_BITS = 10;

	internal const int RELIABLE_SEQUENCE_BITS = 12;

	internal uint Sequence;

	internal ReliabilityModes Reliability;

	internal int Targets;

	internal bool Reliable;

	internal Entity TargetEntity;

	internal BoltConnection TargetConnection;

	internal BoltConnection SourceConnection;

	internal new Event_Meta Meta;

	private NetworkStorage storage;

	internal volatile int RefCount = 0;

	internal bool IsPooled = true;

	public bool FromSelf => SourceConnection == null;

	public BoltConnection RaisedBy => SourceConnection;

	public bool IsGlobalEvent => !IsEntityEvent;

	public byte[] BinaryData { get; set; }

	internal override NetworkStorage Storage => storage;

	internal bool IsEntityEvent => Targets == 1 || Targets == 3 || Targets == 5 || Targets == 13 || Targets == 7 || Targets == 11 || Targets == 15 || Targets == 9;

	internal Event(Event_Meta meta)
		: base(meta)
	{
		Meta = meta;
	}

	internal void InitNetworkStorage()
	{
		storage = AllocateStorage();
		IsPooled = false;
	}

	internal void FreeStorage()
	{
		if (!IsPooled)
		{
			PrepareRelease();
			if (storage != null)
			{
				Meta.FreeStorage(storage);
			}
			Reliable = false;
			TargetEntity = null;
			TargetConnection = null;
			SourceConnection = null;
			(Meta as IFactory).Return(this);
			IsPooled = true;
		}
	}

	internal bool Pack(BoltConnection connection, UdpPacket packet)
	{
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			if (!Meta.Properties[i].Property.Write(connection, this, storage, packet))
			{
				return false;
			}
		}
		return true;
	}

	internal void Read(BoltConnection connection, UdpPacket packet)
	{
		for (int i = 0; i < Meta.Properties.Length; i++)
		{
			Meta.Properties[i].Property.Read(connection, this, storage, packet);
		}
	}

	public void Send()
	{
		EventDispatcher.Enqueue(this);
	}

	protected virtual void PrepareRelease()
	{
	}

	internal void IncrementRefs()
	{
		RefCount++;
	}

	internal void DecrementRefs()
	{
		if (RefCount > 0)
		{
			RefCount--;
		}
		CheckRefs();
	}

	internal void CheckRefs()
	{
		if (RefCount == 0)
		{
			FreeStorage();
		}
	}
}
