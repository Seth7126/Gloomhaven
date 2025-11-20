#define ENABLE_LOGS
using System.Diagnostics;
using System.Linq;
using Photon.Bolt;

namespace FFSNet;

[DebuggerDisplay("ID: {ID} Type: {GetControllableType} Character: {GetCharacter}")]
public class NetworkControllable
{
	public string GetControllableType
	{
		get
		{
			if (ControllableObject is BenchedCharacter)
			{
				return "Benched";
			}
			if (ControllableObject is CharacterManager)
			{
				return "CharacterManager";
			}
			if (ControllableObject is NewPartyCharacterUI)
			{
				return "NewPartyCharacterUI";
			}
			return "None";
		}
	}

	public string GetCharacter
	{
		get
		{
			if (ControllableObject is BenchedCharacter benchedCharacter && benchedCharacter != null && benchedCharacter.CharacterData != null)
			{
				return benchedCharacter.CharacterData.CharacterID;
			}
			if (ControllableObject is CharacterManager characterManager && characterManager != null && characterManager.CharacterActor != null)
			{
				return characterManager.CharacterActor.Class.ID;
			}
			if (ControllableObject is NewPartyCharacterUI newPartyCharacterUI && newPartyCharacterUI != null && newPartyCharacterUI.Data != null)
			{
				return newPartyCharacterUI.Data.CharacterID;
			}
			return "None";
		}
	}

	public int ID { get; private set; }

	public NetworkPlayer Controller { get; private set; }

	public IControllable ControllableObject { get; private set; }

	public BoltEntity NetworkEntity { get; set; }

	public bool IsParticipant
	{
		get
		{
			if (ControllableObject != null)
			{
				return ControllableObject.IsParticipant;
			}
			return false;
		}
	}

	public bool IsAlive
	{
		get
		{
			if (ControllableObject != null)
			{
				return ControllableObject.IsAlive;
			}
			return false;
		}
	}

	public NetworkControllable(int id, IControllable controllableObject, NetworkPlayer initialController = null)
	{
		Console.LogInfo("Creating new NetworkControllable. ID: " + id);
		ID = id;
		ControllableObject = controllableObject;
		if (!FFSNetwork.IsOnline)
		{
			return;
		}
		if (NetworkEntity == null)
		{
			NetworkEntity = BoltNetwork.Entities.FirstOrDefault((BoltEntity x) => x.TryFindState<IControllableState>(out var state) && state.ControllableID == ID);
			if (NetworkEntity == null)
			{
				if (FFSNetwork.IsHost)
				{
					NetworkEntity = BoltNetwork.Instantiate(ControllableObject.GetNetworkEntityPrefabID(), new ControllableToken(ID));
				}
				else
				{
					Debug.Log("Client was unable to find matching BoltNetwork Entity for ID: " + ID + " - it may not have been instantiated by the Host yet. It will be attached when it has.");
				}
			}
		}
		if (initialController != null)
		{
			initialController.AssignControllable(this, releaseFirst: false, syncAssignmentToClientsIfServer: false);
		}
		else if (!(controllableObject is BenchedCharacter) && PlayerRegistry.HostPlayer != null)
		{
			PlayerRegistry.HostPlayer.AssignControllable(this, releaseFirst: false, syncAssignmentToClientsIfServer: false);
		}
	}

	public void ChangeControllableObject(IControllable controllableObject, bool releaseOldControllableObject = true, NetworkPlayer newController = null)
	{
		if (controllableObject == ControllableObject)
		{
			return;
		}
		Console.LogInfo("Changing the controllable object for the controllable with ID: " + ID);
		IControllable controllableObject2 = ControllableObject;
		if (releaseOldControllableObject)
		{
			ControllableObject?.OnControlReleased();
		}
		ControllableObject = controllableObject;
		if (newController == null)
		{
			if (controllableObject is BenchedCharacter)
			{
				Release();
				ControllableRegistry.OnControllerChanged?.Invoke(this, Controller, null);
			}
			else
			{
				ControllableObject?.OnControlAssigned(Controller);
			}
		}
		else
		{
			newController.AssignControllable(this, releaseFirst: true, syncAssignmentToClientsIfServer: false);
		}
		ControllableRegistry.OnControllableObjectChanged?.Invoke(this, controllableObject2, controllableObject);
	}

	public void Release(bool syncToClientsIfServer = false)
	{
		if (Controller != null)
		{
			Controller.ReleaseControllable(this, syncToClientsIfServer);
		}
	}

	public void OnControlAssigned(NetworkPlayer controller)
	{
		Controller = controller;
		ControllableObject?.OnControlAssigned(Controller);
		if (FFSNetwork.IsHost)
		{
			NetworkEntity?.GetComponent<INetworkedState>().UpdateControllerID(Controller.PlayerID);
		}
	}

	public void OnControlReleased()
	{
		ControllableObject?.OnControlReleased();
		Controller = null;
	}

	public void UpdateState(GameAction action)
	{
		if (NetworkEntity != null)
		{
			NetworkEntity.GetComponent<INetworkedState>().UpdateState(action);
		}
		else
		{
			Debug.LogError("Attempted to run UpdateState on a null NetworkEntity!  ControllableID: " + ID);
		}
	}

	public void ApplyState()
	{
		if (NetworkEntity != null)
		{
			NetworkEntity.GetComponent<INetworkedState>().ApplyState();
		}
		else
		{
			Debug.LogError("Attempted to run ApplyState on a null NetworkEntity!  ControllableID: " + ID);
		}
	}

	public void ResetState()
	{
		if (NetworkEntity != null)
		{
			NetworkEntity.GetComponent<INetworkedState>().ResetState();
		}
		else
		{
			Debug.LogError("Attempted to run ResetState on a null NetworkEntity!  ControllableID: " + ID);
		}
	}

	public void ClearScenarioState()
	{
		if (NetworkEntity != null)
		{
			NetworkEntity.GetComponent<INetworkedState>().ClearScenarioState();
		}
		else
		{
			Debug.LogError("Attempted to run ClearScenarioState on a null NetworkEntity!  ControllableID: " + ID);
		}
	}
}
