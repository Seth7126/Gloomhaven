namespace Manatee.Trello.Internal.Eventing;

internal interface IHandle
{
}
internal interface IHandle<TMessage> : IHandle
{
	void Handle(TMessage message);
}
