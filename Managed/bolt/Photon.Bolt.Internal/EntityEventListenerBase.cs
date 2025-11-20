namespace Photon.Bolt.Internal;

[Documentation(Alias = "Photon.Bolt.EntityEventListener")]
public abstract class EntityEventListenerBase : EntityBehaviour
{
	public sealed override void Initialized()
	{
		base.entity.Entity.AddEventListener(this);
	}
}
[Documentation(Alias = "Photon.Bolt.EntityEventListener<TState>")]
public abstract class EntityEventListenerBase<TState> : EntityEventListenerBase
{
	public TState state => base.entity.GetState<TState>();
}
