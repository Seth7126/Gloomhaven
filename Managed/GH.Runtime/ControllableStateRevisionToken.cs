using System;
using FFSNet;
using Photon.Bolt;
using UdpKit;

public sealed class ControllableStateRevisionToken : IProtocolToken
{
	public struct ControllableStateRevisionData
	{
		public int ControllableID;

		public int ControllerID;

		public ulong RevisionHash;
	}

	private int controllablesCount;

	public ControllableStateRevisionData[] Data { get; set; }

	public ControllableStateRevisionToken(NetworkPlayer controller)
	{
		controllablesCount = controller.MyControllables.Count;
		Data = new ControllableStateRevisionData[controllablesCount];
		FFSNet.Console.LogInfo("Creating a ControllableStateRevisionsToken. " + controller.Username + "'s controllable count: " + controllablesCount);
		for (int i = 0; i < controllablesCount; i++)
		{
			GHNetworkControllable component = controller.MyControllables[i].NetworkEntity.GetComponent<GHNetworkControllable>();
			Data[i].ControllableID = component.state.ControllableID;
			Data[i].ControllerID = component.state.ControllerID;
			Data[i].RevisionHash = component.GetRevisionHash();
		}
	}

	public ControllableStateRevisionToken()
	{
		controllablesCount = 0;
		Data = new ControllableStateRevisionData[controllablesCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(controllablesCount);
		for (int i = 0; i < controllablesCount; i++)
		{
			packet.WriteInt(Data[i].ControllableID);
			packet.WriteInt(Data[i].ControllerID);
			packet.WriteULong(Data[i].RevisionHash);
		}
	}

	public void Read(UdpPacket packet)
	{
		controllablesCount = packet.ReadInt();
		Data = new ControllableStateRevisionData[controllablesCount];
		for (int i = 0; i < controllablesCount; i++)
		{
			Data[i].ControllableID = packet.ReadInt();
			Data[i].ControllerID = packet.ReadInt();
			Data[i].RevisionHash = packet.ReadULong();
		}
	}

	public bool IsSameRevision(NetworkPlayer targetController)
	{
		for (int i = 0; i < controllablesCount; i++)
		{
			if (targetController != PlayerRegistry.GetPlayer(Data[i].ControllerID))
			{
				FFSNet.Console.LogWarning("State revisions do not match. Incorrect controller detected.");
				return false;
			}
			GHNetworkControllable gHNetworkControllable;
			try
			{
				NetworkControllable controllable = ControllableRegistry.GetControllable(Data[i].ControllableID);
				if (controllable == null)
				{
					FFSNet.Console.LogWarning("Unable to find controllable for ControllableID = " + Data[i].ControllableID);
					ControllableRegistry.Print();
					return false;
				}
				if (!(controllable.NetworkEntity != null))
				{
					FFSNet.Console.LogWarning("NetworkEntity for controllable is null.  ControllableID = " + Data[i].ControllableID);
					return false;
				}
				gHNetworkControllable = ControllableRegistry.GetControllable(Data[i].ControllableID)?.NetworkEntity.GetComponent<GHNetworkControllable>();
			}
			catch (Exception ex)
			{
				FFSNet.Console.LogWarning("Exception getting NetworkEntity from ControllableID: " + Data[i].ControllableID + ".\n" + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
			if (gHNetworkControllable != null)
			{
				if (Data[i].RevisionHash != gHNetworkControllable.GetRevisionHash())
				{
					FFSNet.Console.LogInfo("State revisions do not match. State properties unsynchronized.");
					return false;
				}
				continue;
			}
			FFSNet.Console.LogWarning("State revisions do not match. Incorrect controllable detected.");
			return false;
		}
		FFSNet.Console.LogInfo("Comparison check for state revisions successfully completed. State revisions match!");
		return true;
	}

	public void PrintControllableStateRevisionData()
	{
		for (int i = 0; i < controllablesCount; i++)
		{
			FFSNet.Console.LogInfo("Controllable State Revision Hash (ControllableID: " + Data[i].ControllableID + ", ControllerID: " + Data[i].ControllerID + "): " + Data[i].RevisionHash);
		}
	}
}
