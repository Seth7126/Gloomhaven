namespace Photon.Bolt.Internal;

internal interface IEventFactory : IFactory
{
	void Dispatch(Event ev, object target);
}
