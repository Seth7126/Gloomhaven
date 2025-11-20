using AStar;
using ScenarioRuleLibrary;
using UnityEngine;

public class CInteractableActor : CInteractable
{
	[SerializeField]
	private bool triggerMouseOver;

	private CActor m_Actor;

	public void Awake()
	{
	}

	public void Start()
	{
		try
		{
			if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
			{
				Object.Destroy(this);
			}
			else
			{
				m_Actor = GetComponentInParent<CharacterManager>()?.CharacterActor;
			}
		}
		catch
		{
			Object.Destroy(this);
		}
	}

	public virtual void Update()
	{
	}

	public override void ShowNormalInterface(bool disabled)
	{
		if (m_Actor != null && TileBehaviour.s_Callback != null)
		{
			Point arrayIndex = m_Actor.ArrayIndex;
			CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[arrayIndex.X, arrayIndex.Y];
			TileBehaviour.s_Callback(clientTile, null, networkActionIfOnline: true, isUserClick: true, SaveData.Instance.Global.EnableSecondClickHexToConfirm);
		}
	}

	public override void OnDoubleClicked()
	{
		if (FFSNetwork.IsOnline)
		{
			_ = m_Actor;
		}
	}
}
