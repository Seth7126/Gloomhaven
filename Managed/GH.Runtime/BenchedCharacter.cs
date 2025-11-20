using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using ScenarioRuleLibrary;

public sealed class BenchedCharacter : IControllable
{
	public CMapCharacter CharacterData { get; set; }

	public bool IsParticipant => false;

	public bool IsAlive => true;

	public BenchedCharacter(CMapCharacter characterData)
	{
		CharacterData = characterData;
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			ControllableRegistry.CreateControllable(characterData.CharacterName.GetHashCode(), this, PlayerRegistry.HostPlayer);
		}
		else
		{
			ControllableRegistry.CreateControllable(CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID), this, PlayerRegistry.HostPlayer);
		}
	}

	public void Destroy()
	{
		ControllableRegistry.DestroyControllable(AdventureState.MapState.IsCampaign ? CharacterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(CharacterData.CharacterID));
	}

	public void OnControlAssigned(NetworkPlayer controller)
	{
		if (!(controller == null) && !(PlayerRegistry.MyPlayer == null) && PlayerRegistry.MyPlayer.PlayerID == controller.PlayerID)
		{
			CharacterData.IsUnderMyControl = true;
			Console.LogInfo(CharacterData.CharacterID + "(BENCHED) is now under my control.");
		}
	}

	public void OnControlReleased()
	{
		if (!(PlayerRegistry.MyPlayer == null) && CharacterData != null && CharacterData.IsUnderMyControl)
		{
			CharacterData.IsUnderMyControl = false;
			Console.LogInfo(CharacterData.CharacterID + "(BENCHED) is no longer under my control.");
		}
	}

	public PrefabId GetNetworkEntityPrefabID()
	{
		return BoltPrefabs.GHControllableState;
	}

	public string GetName()
	{
		if (CharacterData == null)
		{
			return "UNINITIALIZED";
		}
		return CharacterData.CharacterID;
	}
}
