using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FFSNet;
using UnityEngine;

public class PingManager : Singleton<PingManager>
{
	private class Ping
	{
		public NetworkPlayer player;

		public GameObject element;

		public UIPingTooltip tooltip;

		public GameObject highlight;

		public Ping(NetworkPlayer player, GameObject element, UIPingTooltip tooltip, GameObject highlight)
		{
			this.player = player;
			this.element = element;
			this.tooltip = tooltip;
			this.highlight = highlight;
		}
	}

	[SerializeField]
	private UIPingTooltip pingTooltipPrefab;

	[SerializeField]
	private GameObject ping3DHighlightPrefab;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private float lifetimePing = 2f;

	private Stack<UIPingTooltip> pingTooltipPool = new Stack<UIPingTooltip>();

	private Stack<GameObject> pingHighlightPool = new Stack<GameObject>();

	private List<Ping> assigned = new List<Ping>();

	private UIPingTooltip GetPingTooltip()
	{
		if (pingTooltipPool.Count > 0)
		{
			return pingTooltipPool.Pop();
		}
		return Object.Instantiate(pingTooltipPrefab, base.transform);
	}

	private GameObject GetPingHighlight()
	{
		GameObject gameObject = ((pingTooltipPool.Count <= 0) ? Object.Instantiate(ping3DHighlightPrefab, Choreographer.s_Choreographer.transform) : pingHighlightPool.Pop());
		gameObject.gameObject.SetActive(value: true);
		return gameObject;
	}

	public void Ping3DElementSinglePlayer(GameObject element)
	{
		Ping3DElement(element);
	}

	public void Ping3DElementMultiPlayer(GameObject element, NetworkPlayer player)
	{
		Ping3DElement(element, player);
	}

	private void Ping3DElement(GameObject element, NetworkPlayer player = null)
	{
		if (!IsPingShown(element, player))
		{
			HidePing(player, element, instant: true);
			GameObject pingHighlight = GetPingHighlight();
			pingHighlight.transform.position = element.transform.position;
			UIPingTooltip pingTooltip = GetPingTooltip();
			string text = player?.Username ?? PlatformLayer.UserData.UserName;
			string description = PlatformTextSpriteProvider.GetPlatformIcon(player?.PlatformName ?? PlatformLayer.Instance.PlatformID) + " " + text;
			pingTooltip.Show(description, element, canvas.transform as RectTransform);
			Ping ping = new Ping(player, element, pingTooltip, pingHighlight);
			assigned.Add(ping);
			StartCoroutine(Wait(ping));
		}
	}

	private bool IsPingShown(GameObject element, NetworkPlayer player)
	{
		return assigned.FirstOrDefault((Ping x) => x.element == element && x.player == player) != null;
	}

	private IEnumerator Wait(Ping element)
	{
		yield return Timekeeper.instance.WaitForSeconds(lifetimePing);
		HidePing(element);
	}

	private void HidePing(Ping element, bool instant = false)
	{
		if (assigned.Remove(element))
		{
			element.tooltip.Hide(instant);
			element.highlight.SetActive(value: false);
			pingTooltipPool.Push(element.tooltip);
			pingHighlightPool.Push(element.highlight);
		}
	}

	public void HidePing(NetworkPlayer player, GameObject element, bool instant = false)
	{
		foreach (Ping item in assigned.FindAll((Ping it) => it.player == player || it.element == element))
		{
			HidePing(item, instant);
		}
	}

	public void ProxyPingHex(GameAction gameAction)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(gameAction.PlayerID);
		if (player != null)
		{
			TileToken tileToken = (TileToken)gameAction.SupplementaryDataToken;
			CClientTile[,] array = ClientScenarioManager.s_ClientScenarioManager?.ClientTileArray;
			if (!array.IsTNull())
			{
				if (tileToken.TileX >= 0 && tileToken.TileX < array.GetLength(0) && tileToken.TileY >= 0 && tileToken.TileY < array.GetLength(1))
				{
					CClientTile cClientTile = array[tileToken.TileX, tileToken.TileY];
					try
					{
						Console.Log(player.Username + " pinged a tile (TileX: " + tileToken.TileX + ", TileY: " + tileToken.TileY + ").");
						Ping3DElementMultiPlayer(cClientTile.m_TileBehaviour.gameObject, player);
						return;
					}
					catch
					{
						Console.LogWarning("Cannot ping the hex. The tile returns null (TileX: " + tileToken.TileX + ", TileY: " + tileToken.TileY + ").");
						return;
					}
				}
				Console.LogWarning("Cannot ping the hex. The target tile is out of bounds of the tile map.");
			}
			else
			{
				Console.LogWarning("Cannot ping the hex. The tile map is not initialized.");
			}
		}
		else
		{
			Console.LogWarning("Cannot ping the hex. The pinging player could not be found (PlayerID: " + player.PlayerID + ").");
		}
	}
}
