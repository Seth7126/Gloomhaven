using System.Threading.Tasks;

namespace Platforms.Epic;

public interface IHydraSettingsProviderEpic
{
	Task<string> GetToken();
}
