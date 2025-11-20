namespace Manatee.Trello;

public interface IPowerUpData : ICacheable, IRefreshable
{
	string PluginId { get; }

	string Value { get; }
}
