using System.Threading.Tasks;

namespace Platforms.Steam;

public interface IHydraSettingsProviderSteam
{
	Task<byte[]> GetAuthTicket();

	void DisposeTicket();
}
