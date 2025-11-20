namespace Manatee.Trello;

public interface ICanWebhook : ICacheable
{
	void ApplyAction(IAction action);
}
