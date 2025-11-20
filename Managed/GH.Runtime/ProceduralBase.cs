using System;
using UnityEngine;

public class ProceduralBase : MonoBehaviour, IContentUpdateMonitor
{
	public bool IsPlacementCompleted { get; private set; }

	public event Action<ProceduralBase> ContentPlacementStarted;

	public event Action<ProceduralBase> ContentPlacementCompleted;

	protected virtual void Awake()
	{
		GetComponent<ApparanceEntity>().IsPopulated = false;
		IsPlacementCompleted = false;
	}

	protected ProceduralMapTile GetRoom()
	{
		return GloomUtility.FindAncestorComponent<ProceduralMapTile>(this);
	}

	protected ProceduralStyle GetRoomStyle()
	{
		ProceduralMapTile room = GetRoom();
		if (room != null)
		{
			return GloomUtility.EnsureComponent<ProceduralStyle>(room.gameObject);
		}
		return null;
	}

	protected int GetRoomSeed()
	{
		ProceduralStyle roomStyle = GetRoomStyle();
		if (roomStyle != null)
		{
			return roomStyle.Seed;
		}
		return 0;
	}

	internal ProceduralStyle GetParentStyle()
	{
		ProceduralStyle roomStyle = GetRoomStyle();
		if (roomStyle != null)
		{
			return roomStyle;
		}
		return GetScenarioStyle();
	}

	internal ProceduralScenario GetScenario()
	{
		ProceduralScenario proceduralScenario = GloomUtility.FindAncestorComponent<ProceduralScenario>(this);
		if (proceduralScenario != null)
		{
			return proceduralScenario;
		}
		_ = base.gameObject.scene;
		if (base.gameObject.scene.isLoaded)
		{
			GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				ProceduralScenario component = rootGameObjects[i].GetComponent<ProceduralScenario>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	internal ProceduralStyle GetScenarioStyle()
	{
		ProceduralScenario scenario = GetScenario();
		if (scenario != null)
		{
			return GloomUtility.EnsureComponent<ProceduralStyle>(scenario.gameObject);
		}
		return null;
	}

	protected int GetScenarioSeed()
	{
		ProceduralStyle scenarioStyle = GetScenarioStyle();
		if (scenarioStyle != null)
		{
			return scenarioStyle.Seed;
		}
		return 0;
	}

	public virtual void NotifyContentPlacementStarted()
	{
		IsPlacementCompleted = false;
		this.ContentPlacementStarted?.Invoke(this);
	}

	public virtual void NotifyContentPlacementComplete()
	{
		IsPlacementCompleted = true;
		this.ContentPlacementCompleted?.Invoke(this);
		if (!(base.transform.parent != null))
		{
			return;
		}
		GameObject gameObject = base.transform.parent.gameObject;
		while (gameObject != null)
		{
			if (gameObject.GetComponent<IProceduralContentMonitor>() != null)
			{
				IProceduralContentMonitor[] components = gameObject.GetComponents<IProceduralContentMonitor>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].ProceduralContentChanged(base.gameObject);
				}
			}
			gameObject = gameObject.transform.parent?.gameObject;
		}
	}

	public virtual void NotifyContentRemovalStarted()
	{
		IsPlacementCompleted = false;
	}

	public virtual void NotifyContentRemovalComplete()
	{
		IsPlacementCompleted = false;
	}
}
