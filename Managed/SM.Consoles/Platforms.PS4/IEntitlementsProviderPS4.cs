namespace Platforms.PS4;

public interface IEntitlementsProviderPS4 : IEntitlementPack
{
	string FromSceLabel(string sceID);
}
