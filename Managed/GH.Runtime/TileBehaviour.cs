using System.Collections.Generic;
using AStar;
using AsmodeeNet.Utils.Extensions;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using Script.Controller;
using UnityEngine;
using UnityEngine.UI;

public class TileBehaviour : MonoBehaviour
{
	public delegate void CallbackType(CClientTile clientTile, List<CTile> optionalTileList, bool networkActionIfOnline = false, bool isUserClick = false, bool actingPlayerHasSecondClickConfirmationEnabled = false);

	public CClientTile m_ClientTile;

	[SerializeField]
	private float m_TooltipWidth = 300f;

	[SerializeField]
	private Vector2 m_TooltipOffset = new Vector2(-20f, 10f);

	private bool isShowingTooltip;

	private WorldspaceTileBehaviourUI m_WorldspaceTileBehaviourUI;

	public static CallbackType s_Callback;

	public static void SetCallback(CallbackType callback)
	{
		s_Callback = callback;
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		Singleton<ObjectCacheService>.Instance.AddTileBehaviour(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (Singleton<ObjectCacheService>.Instance != null)
		{
			Singleton<ObjectCacheService>.Instance.RemoveTileBehaviour(this);
		}
		HideTooltip();
	}

	public void ShowTooltip(string tooltipTitle, string tooltipText = null)
	{
		isShowingTooltip = true;
		UITooltip.ResetContent();
		UITooltip.ShowBackgroundImage(show: true);
		UITooltip.SetScreenBound(enable: true, 20f);
		UITooltip.SetWidth(m_TooltipWidth);
		UITooltip.SetVerticalControls(autoAdjusted: true);
		if (tooltipTitle != null)
		{
			UITooltip.AddTitle(string.Format("<color=#{1}>{0}</color>", tooltipTitle, UIInfoTools.Instance.basicTextColor.ToHex()));
		}
		if (tooltipText != null)
		{
			UITooltip.AddDescription(tooltipText);
		}
		UITooltip.SetOffset(m_TooltipOffset);
		UITooltip.AnchorToRect(null, UITooltip.Corner.Auto);
		UITooltip.Show();
	}

	public void HideTooltip()
	{
		if (isShowingTooltip)
		{
			isShowingTooltip = false;
			UITooltip.Hide();
		}
	}

	public void CreateWorldspaceTileBehaviourUI()
	{
		if (m_WorldspaceTileBehaviourUI == null && m_ClientTile != null && m_ClientTile.m_Tile != null)
		{
			GameObject gameObject = ObjectPool.Spawn(WorldspaceUITools.Instance.WorldspaceTileBehaviourUIPrefab, null, base.transform.position, Quaternion.identity);
			gameObject.transform.SetParent(WorldspaceUITools.Instance.WorldspaceGUIPrefabLevel.transform, worldPositionStays: false);
			m_WorldspaceTileBehaviourUI = gameObject.GetComponent<WorldspaceTileBehaviourUI>();
			CNode cNode = ScenarioManager.PathFinder.Nodes[m_ClientTile.m_Tile.m_ArrayIndex.X, m_ClientTile.m_Tile.m_ArrayIndex.Y];
			m_WorldspaceTileBehaviourUI.Init(this, m_ClientTile.m_Tile.m_ArrayIndex.X + ":" + m_ClientTile.m_Tile.m_ArrayIndex.Y + "(" + ((!cNode.IsBridge) ? "" : (cNode.IsBridgeOpen ? "BO" : "B")) + (cNode.Walkable ? "W" : "") + (cNode.Blocked ? "BL" : "") + ")");
		}
	}

	public void RefreshWorldSpaceIcon()
	{
		if (DebugMenu.DebugMenuNotNull)
		{
			m_WorldspaceTileBehaviourUI.RefreshWorldIcon();
		}
	}

	public void HideWorldSpaceIcon()
	{
		if (DebugMenu.DebugMenuNotNull)
		{
			m_WorldspaceTileBehaviourUI.HideIcon();
		}
	}
}
