#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;
using Photon.Bolt;

namespace FFSNet;

public static class ControllableRegistry
{
	public static List<NetworkControllable> AllControllables { get; private set; } = new List<NetworkControllable>();

	public static List<NetworkControllable> ParticipatingControllables => AllControllables.FindAll((NetworkControllable x) => x.IsParticipant);

	public static ControllablesChangedEvent OnControllableCreated { get; set; }

	public static ControllablesChangedEvent OnControllableDestroyed { get; set; }

	public static ControllableObjectChangedEvent OnControllableObjectChanged { get; set; }

	public static ControllerChangedEvent OnControllerChanged { get; set; }

	public static void CreateControllable(int id, IControllable controllableObject, NetworkPlayer initialController = null, bool changeObjectIfControllableExists = true, bool releaseOldControllableObject = true)
	{
		if (id == 0)
		{
			Console.LogError("ERROR_MULTIPLAYER_00019", "Error creating a new NetworkControllable. The ID #0 is reserved as the state default.");
			return;
		}
		if (controllableObject == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00018", "Error creating a new NetworkControllable. ControllableObject returns null.");
			return;
		}
		NetworkControllable controllable = GetControllable(id);
		if (controllable == null)
		{
			controllable = new NetworkControllable(id, controllableObject, initialController);
			AllControllables.Add(controllable);
			OnControllableCreated?.Invoke(controllable);
			Console.Log("Controllable (ID: " + controllable.ID + ") CREATED");
			Print();
		}
		else if (changeObjectIfControllableExists)
		{
			controllable.ChangeControllableObject(controllableObject, releaseOldControllableObject, initialController);
		}
	}

	public static void ChangeControllableObject(int id, IControllable controllableObject, bool releaseOldControllableObject = true, NetworkPlayer newController = null)
	{
		ChangeControllableObject(GetControllable(id), controllableObject, releaseOldControllableObject, newController);
	}

	public static void ChangeControllableObject(NetworkControllable controllable, IControllable controllableObject, bool releaseOldControllableObject = true, NetworkPlayer newController = null)
	{
		if (controllable != null)
		{
			controllable.ChangeControllableObject(controllableObject, releaseOldControllableObject, newController);
		}
		else
		{
			Debug.LogWarning("Tried to change the controllable object but the controllable returns null.");
		}
	}

	public static void DestroyControllable(int id, bool syncToClientsIfServer = false)
	{
		DestroyControllable(GetControllable(id), syncToClientsIfServer);
	}

	public static void DestroyControllable(NetworkControllable controllable, bool syncToClientsIfServer = false)
	{
		if (controllable != null)
		{
			if (AllControllables.Contains(controllable))
			{
				controllable.Release();
				OnControllableDestroyed?.Invoke(controllable);
				AllControllables.Remove(controllable);
				Console.Log("Controllable (ID: " + controllable.ID + ") DESTROYED");
				Print();
				if (FFSNetwork.IsHost)
				{
					BoltNetwork.Destroy(controllable.NetworkEntity);
					if (syncToClientsIfServer)
					{
						ControllableDestructionEvent controllableDestructionEvent = ControllableDestructionEvent.Create(GlobalTargets.AllClients, ReliabilityModes.ReliableOrdered);
						controllableDestructionEvent.ControllableID = controllable.ID;
						controllableDestructionEvent.Send();
					}
				}
			}
			else
			{
				Debug.LogError("Tried to remove a controllable from the registry but no such controllable resides within the list of controllables.");
			}
		}
		else
		{
			Debug.Log("Tried to remove a controllable from the registry but the controllable returns null.");
		}
	}

	public static NetworkControllable GetControllable(int controllableID)
	{
		return AllControllables.FirstOrDefault((NetworkControllable x) => x.ID == controllableID);
	}

	public static NetworkPlayer GetController(int controllableID)
	{
		return GetControllable(controllableID)?.Controller;
	}

	public static void Print()
	{
		Console.LogDebug("Printing controllable registry (" + AllControllables.Count + "):");
		foreach (NetworkControllable allControllable in AllControllables)
		{
			if (allControllable != null)
			{
				Console.LogDebug("Controllable (ID: " + allControllable.ID + ", Linked Object: " + ((allControllable.ControllableObject != null) ? allControllable.ControllableObject.GetName() : "NULL") + ")");
			}
			else
			{
				Console.LogDebug("Null controllable.");
			}
		}
	}

	public static void Reset()
	{
		OnControllableCreated = null;
		OnControllableDestroyed = null;
		OnControllableObjectChanged = null;
		OnControllerChanged = null;
		AllControllables.Clear();
		Debug.Log("Cleared Controllable Registry!");
	}
}
