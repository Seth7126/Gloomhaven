using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class UnityGameEditorDoorProp : MonoBehaviour, IHoverable
{
	public CObjectDoor.EDoorType m_DoorType;

	public bool m_IsDungeonEntrance;

	public bool m_IsDungeonExit;

	public bool m_InitiallyVisable;

	public CObjectDoor.ELockType m_LockType;

	private CapsuleCollider _hoverCapsule;

	public CObjectDoor DoorInCurrentState { get; private set; }

	public bool IsDoorOpen { get; private set; }

	private void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Hovering");
	}

	private void OnValidate()
	{
	}

	public Vector3 ForwardVector()
	{
		if (!ApparanceLayer.IsThinDoor(m_DoorType))
		{
			return base.transform.forward;
		}
		return base.transform.right;
	}

	public Vector3 RightVector()
	{
		if (!ApparanceLayer.IsThinDoor(m_DoorType))
		{
			return base.transform.right;
		}
		return -base.transform.forward;
	}

	public CObjectDoor.ELockType LockType()
	{
		return m_LockType;
	}

	public void CreateHoverCapsule(GameObject door)
	{
		float num = 2f;
		float radius = 0.8f;
		ProceduralTile component = door.GetComponent<ProceduralTile>();
		if (component != null)
		{
			num = component.ContentHeight;
		}
		_hoverCapsule = base.gameObject.AddComponent<CapsuleCollider>();
		_hoverCapsule.height = num;
		_hoverCapsule.radius = radius;
		_hoverCapsule.center = new Vector3(0f, num * 0.5f, 0f);
		_hoverCapsule.isTrigger = true;
	}

	public void OnCursorEnter()
	{
		if (IsDoorOpen)
		{
			return;
		}
		IsDoorOpen = MF.GameObjectAnimatorControllerIsCurrentState(base.gameObject, "Open");
		if (_hoverCapsule != null)
		{
			_hoverCapsule.enabled = !IsDoorOpen;
		}
		if (DoorInCurrentState == null)
		{
			DoorInCurrentState = ScenarioManager.CurrentScenarioState.DoorProps.OfType<CObjectDoor>().SingleOrDefault((CObjectDoor s) => s?.InstanceName == base.gameObject.name);
		}
		if (DoorInCurrentState != null && DoorInCurrentState.RuntimeAttachedActor != null && !DoorInCurrentState.RuntimeAttachedActor.IsDead && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<ActorStatPanel>.Instance.Show(DoorInCurrentState.RuntimeAttachedActor);
		}
	}

	public void OnCursorExit()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<UIDoorInfoPanel>.Instance?.Hide();
			if (DoorInCurrentState != null && DoorInCurrentState.RuntimeAttachedActor != null && !DoorInCurrentState.RuntimeAttachedActor.IsDead)
			{
				Singleton<ActorStatPanel>.Instance.HideForActor(DoorInCurrentState.RuntimeAttachedActor);
			}
		}
	}
}
