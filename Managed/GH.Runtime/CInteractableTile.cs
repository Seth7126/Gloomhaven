using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;

public class CInteractableTile : CInteractable
{
	private TileBehaviour m_TileBehaviour;

	public override void ShowNormalInterface(bool disabled)
	{
		if (m_TileBehaviour == null)
		{
			m_TileBehaviour = GetComponent<TileBehaviour>();
		}
		if (TileBehaviour.s_Callback != null && m_TileBehaviour != null)
		{
			if (InputManager.GamePadInUse && IsSelectState() && !NeedSkipAnimation())
			{
				Choreographer.s_Choreographer.m_selectButton.PlayAnimation(ExecuteTileCallback);
			}
			else
			{
				ExecuteTileCallback();
			}
		}
	}

	private bool IsSelectState()
	{
		if (!Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<SelectTargetState>())
		{
			return Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<UseActionScenarioState>();
		}
		return true;
	}

	private bool NeedSkipAnimation()
	{
		if (InteractabilityManager.ShouldTryPreventControl())
		{
			return InteractabilityManager.IsAllowSelectionForTile(m_TileBehaviour.m_ClientTile.m_Tile.m_ArrayIndex);
		}
		return false;
	}

	private void ExecuteTileCallback()
	{
		TileBehaviour.s_Callback(m_TileBehaviour.m_ClientTile, null, networkActionIfOnline: true, isUserClick: true, SaveData.Instance.Global.EnableSecondClickHexToConfirm);
	}

	public override void OnDoubleClicked()
	{
	}
}
