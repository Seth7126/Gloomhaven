using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public class GameToken : CustomDataToken
{
	public int GameModeID { get; set; }

	public string SaveName { get; private set; }

	public string SaveHash { get; private set; }

	public string HostPlayerID { get; private set; }

	public string HostAccountID { get; private set; }

	public string HostNetworkAccountID { get; private set; }

	public string HostUsername { get; private set; }

	public string HostPlatformName { get; private set; }

	public string CustomRulesetName { get; private set; }

	public string CustomLevelWorkshopID { get; private set; }

	public bool WaitUntilSavePoint { get; set; }

	public bool IsModded { get; private set; }

	public string RulesetHash { get; private set; }

	public uint DLCFlag { get; private set; }

	public HashSet<string> CurrentPlatformUsersInSession { get; private set; }

	public bool IsCrossplaySession { get; private set; }

	public GameToken(int gameModeID, string saveName, string saveHash, string hostPlayerID, string hostAccountID, string hostNetworkAccountID, string hostUsername, string hostPlatformName, string customRulesetName, bool isModded, uint dlcFlag, HashSet<string> currentPlatformUsersInSession, bool isCrossplaySession, string rulesetHash = "", string customLevelWorkshopID = "", bool waitUntilSavePoint = false, byte[] customData = null)
		: base(customData)
	{
		GameModeID = gameModeID;
		SaveName = saveName;
		SaveHash = saveHash;
		HostPlayerID = hostPlayerID;
		HostAccountID = hostAccountID;
		HostNetworkAccountID = hostNetworkAccountID;
		HostUsername = hostUsername;
		HostPlatformName = hostPlatformName;
		CustomRulesetName = customRulesetName;
		IsModded = isModded;
		RulesetHash = rulesetHash;
		DLCFlag = dlcFlag;
		CurrentPlatformUsersInSession = currentPlatformUsersInSession;
		CustomLevelWorkshopID = customLevelWorkshopID;
		WaitUntilSavePoint = waitUntilSavePoint;
		IsCrossplaySession = isCrossplaySession;
		string text = "--------------------------------------------------------------------------------------------\n";
		text += "Creating the game data token.\n";
		text = text + "Game mode ID: " + gameModeID + "\n";
		text = text + "Save name: " + saveName + "\n";
		text = text + "Save hash: " + saveHash + "\n";
		text = text + "Host Player ID: " + HostPlayerID + "\n";
		text = text + "Host Account ID: " + HostAccountID + "\n";
		text = text + "Host Network Account ID: " + HostNetworkAccountID + "\n";
		text = text + "Host Username: " + HostUsername + "\n";
		text = text + "Host PlatformName: " + HostPlatformName + "\n";
		text = text + "Custom Ruleset Name: " + CustomRulesetName + "\n";
		text = text + "Custom Level Workshop ID: " + customLevelWorkshopID + "\n";
		text = text + "Is Modded: " + isModded + "\n";
		text = text + "DLC Flag: " + DLCRegistry.GetDLCListAsStringFromFlag((DLCRegistry.EDLCKey)dlcFlag) + "\n";
		text = text + "Is Crossplay Session: " + isCrossplaySession + "\n";
		text += "Users in session: ";
		foreach (string item in CurrentPlatformUsersInSession)
		{
			text = text + item + "; ";
		}
		text += "\n";
		text = text + "Wait Until Save Point: " + WaitUntilSavePoint;
		text += "--------------------------------------------------------------------------------------------";
		FFSNet.Console.LogInfo(text);
	}

	public void MaskBadWordsInUsername(Action onCallback)
	{
		HostUsername.GetCensoredStringAsync(delegate(string censoredUsername)
		{
			HostUsername = censoredUsername;
			onCallback?.Invoke();
		});
	}

	public GameToken()
	{
		GameModeID = 0;
		SaveName = string.Empty;
		SaveHash = string.Empty;
		HostPlayerID = string.Empty;
		HostAccountID = string.Empty;
		HostNetworkAccountID = string.Empty;
		HostUsername = string.Empty;
		HostPlatformName = string.Empty;
		CustomRulesetName = string.Empty;
		CustomLevelWorkshopID = string.Empty;
		IsModded = false;
		RulesetHash = string.Empty;
		DLCFlag = 0u;
		CurrentPlatformUsersInSession = new HashSet<string>();
		IsCrossplaySession = false;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(GameModeID);
		packet.WriteString(SaveName, Encoding.UTF8);
		packet.WriteString(SaveHash, Encoding.UTF8);
		packet.WriteString(HostPlayerID, Encoding.UTF8);
		packet.WriteString(HostAccountID, Encoding.UTF8);
		packet.WriteString(HostNetworkAccountID, Encoding.UTF8);
		packet.WriteString(HostUsername, Encoding.UTF8);
		packet.WriteString(HostPlatformName, Encoding.UTF8);
		packet.WriteString(CustomRulesetName, Encoding.UTF8);
		packet.WriteString(CustomLevelWorkshopID, Encoding.ASCII);
		packet.WriteBool(WaitUntilSavePoint);
		packet.WriteBool(IsModded);
		packet.WriteBool(IsCrossplaySession);
		packet.WriteString(RulesetHash, Encoding.UTF8);
		packet.WriteUInt(DLCFlag);
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, CurrentPlatformUsersInSession);
		byte[] array = memoryStream.ToArray();
		packet.WriteInt(array.Length);
		packet.WriteByteArray(array);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		GameModeID = packet.ReadInt();
		SaveName = packet.ReadString(Encoding.UTF8);
		SaveHash = packet.ReadString(Encoding.UTF8);
		HostPlayerID = packet.ReadString(Encoding.UTF8);
		HostAccountID = packet.ReadString(Encoding.UTF8);
		HostNetworkAccountID = packet.ReadString(Encoding.UTF8);
		HostUsername = packet.ReadString(Encoding.UTF8);
		HostPlatformName = packet.ReadString(Encoding.UTF8);
		CustomRulesetName = packet.ReadString(Encoding.UTF8);
		CustomLevelWorkshopID = packet.ReadString(Encoding.ASCII);
		WaitUntilSavePoint = packet.ReadBool();
		IsModded = packet.ReadBool();
		IsCrossplaySession = packet.ReadBool();
		RulesetHash = packet.ReadString(Encoding.UTF8);
		DLCFlag = packet.ReadUInt();
		byte[] array = new byte[packet.ReadInt()];
		packet.ReadByteArray(array);
		using MemoryStream serializationStream = new MemoryStream(array);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		SerializationBinding binder = new SerializationBinding();
		binaryFormatter.Binder = binder;
		CurrentPlatformUsersInSession = binaryFormatter.Deserialize(serializationStream) as HashSet<string>;
	}
}
