namespace Platforms.PS5;

public interface IEntitlementsProviderPS5 : IEntitlementPack
{
	string FromSceLabel(string sceID);
}
