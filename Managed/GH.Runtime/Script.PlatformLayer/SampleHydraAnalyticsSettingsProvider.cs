using System.Threading.Tasks;
using Platforms.Epic;
using Platforms.GameCore;
using Platforms.Generic;
using Platforms.PS4;
using Platforms.PS5;
using Platforms.ProsOrHydra;
using Platforms.Steam;
using Pros.Sdk;
using Steamworks;

namespace Script.PlatformLayer;

public class SampleHydraAnalyticsSettingsProvider : IProsHydraSettingsProviderGeneric, IHydraProsSettingsProviderPS4, IHydraProsSettingsProviderPS5, IProsHydraSettingsProviderGameCore, IProsSettingsProvider, IHydraSettingsProvider, IHydraSettingsProviderSteam, IHydraSettingsProviderEpic
{
	private string _sceClientIdForTokenVerification;

	private string _sceClientSecret;

	private string _sceClientIdForTokenVerification1;

	private string _sceClientSecret1;

	private AuthTicket _ticket;

	public string HydraEndpoint => "https://prod.hydrapi.net:11701";

	public string HydraTitleId => "PDE8F";

	public string HydraSecretKey => "OwPU4ZgOm1vzH4vAWOA8eXHR++cwHuVGVmVeWtCivNifab9JKODccvTaKNtzWJZxRr4KC+ligVGvKRMJyCW9Sg==";

	public SdkEnvironment Environment => SdkEnvironment.DEVELOPMENT;

	public string ProsTitleId => "RVS0830";

	public string ProsSecretKey => "j552PGfWVyBxNQ1j5KCBPsfczkhaXy2g2PpReY872i8ySUT6TNdD429DXvtWiX1IiwopV3m/H1/VLCbfZ/RLTA==";

	public string ClientVersion => "0.0.1.0";

	public int ConnectionCheckCooldown => 60;

	public string SingleSignOnUrl => "https://hydrapigloomhaven.net:11700";

	string IHydraProsSettingsProviderPS4.SceClientIdForTokenVerification => "53858eff-6787-4616-820a-75423c65fc47";

	string IHydraProsSettingsProviderPS5.SceClientIdForTokenVerification => "d25214fc-36d6-4a4f-9868-51392fc004e5";

	string IHydraProsSettingsProviderPS4.SceClientSecret => "KimUG89YI2Uo8n98";

	string IHydraProsSettingsProviderPS5.SceClientSecret => "WintMJCBL44arNSY";

	string IProsHydraSettingsProviderGeneric.Login => "editor_or_pc_login";

	public string Login => "editor_or_pc_login";

	async Task<byte[]> IHydraSettingsProviderSteam.GetAuthTicket()
	{
		if (!SteamClient.IsValid)
		{
			return null;
		}
		_ticket = await SteamUser.GetAuthSessionTicketAsync();
		return _ticket?.Data;
	}

	public void DisposeTicket()
	{
		_ticket?.Cancel();
	}

	public async Task<string> GetToken()
	{
		return null;
	}
}
