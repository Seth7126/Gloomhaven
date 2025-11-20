using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class UnityGameEditorSpawnerProp : MonoBehaviour, IHoverable
{
	private CSpawner m_Spawner;

	private void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Hovering");
	}

	public void OnCursorEnter()
	{
		if (GetComponent<UnityGameEditorObject>() == null)
		{
			Debug.LogError("No UnityGameEditorObject script found on Trap");
			return;
		}
		if (m_Spawner == null)
		{
			m_Spawner = ScenarioManager.CurrentScenarioState.Spawners.OfType<CSpawner>().SingleOrDefault((CSpawner s) => s.Prop?.InstanceName == base.gameObject.name);
		}
		if (m_Spawner != null && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<ActorStatPanel>.Instance.Show(m_Spawner);
		}
	}

	public void OnCursorExit()
	{
		if (m_Spawner == null)
		{
			m_Spawner = ScenarioManager.CurrentScenarioState.Spawners.OfType<CSpawner>().SingleOrDefault((CSpawner s) => s.Prop?.InstanceName == base.gameObject.name);
		}
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<ActorStatPanel>.Instance.HideForSpawner(m_Spawner);
		}
	}
}
