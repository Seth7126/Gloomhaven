using System.Collections.Generic;
using Photon.Bolt.Collections;

namespace Photon.Bolt.Internal;

public class EntityProxy : BitSet, IBoltListNode<EntityProxy>
{
	public class PriorityComparer : IComparer<EntityProxy>
	{
		public static readonly PriorityComparer Instance = new PriorityComparer();

		private PriorityComparer()
		{
		}

		int IComparer<EntityProxy>.Compare(EntityProxy x, EntityProxy y)
		{
			return y.Priority.CompareTo(x.Priority);
		}
	}

	public NetworkState State;

	public NetworkId NetworkId;

	public ProxyFlags Flags;

	public Priority[] PropertyPriority;

	public Entity Entity;

	public BoltConnection Connection;

	public Queue<EntityProxyEnvelope> Envelopes;

	public IProtocolToken ControlTokenLost;

	public IProtocolToken ControlTokenGained;

	public int Skipped;

	public float Priority;

	EntityProxy IBoltListNode<EntityProxy>.prev { get; set; }

	EntityProxy IBoltListNode<EntityProxy>.next { get; set; }

	object IBoltListNode<EntityProxy>.list { get; set; }

	public EntityProxy()
	{
		Envelopes = new Queue<EntityProxyEnvelope>();
	}

	public EntityProxyEnvelope CreateEnvelope()
	{
		EntityProxyEnvelope entityProxyEnvelope = EntityProxyEnvelopePool.Acquire();
		entityProxyEnvelope.Proxy = this;
		entityProxyEnvelope.Flags = Flags;
		entityProxyEnvelope.ControlTokenLost = ControlTokenLost;
		entityProxyEnvelope.ControlTokenGained = ControlTokenGained;
		return entityProxyEnvelope;
	}

	public override string ToString()
	{
		return string.Format("[Proxy {0} {1}]", NetworkId, ((object)Entity) ?? ((object)"NULL"));
	}
}
