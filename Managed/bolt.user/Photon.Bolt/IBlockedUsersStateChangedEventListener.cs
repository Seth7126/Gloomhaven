namespace Photon.Bolt;

public interface IBlockedUsersStateChangedEventListener
{
	void OnEvent(BlockedUsersStateChangedEvent ev);
}
