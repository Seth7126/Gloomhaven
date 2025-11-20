using System.Collections;
using FFSNet;
using UnityEngine;

namespace Script.PlatformLayer;

public class PlayerOnlineProvider
{
	private volatile bool _isPlayerOnline;

	private bool _menuLoading;

	public bool IsPlayerOnline => _isPlayerOnline;

	public PlayerOnlineProvider()
	{
		CoroutineHelper.RunCoroutine(UpdateOnlineState());
	}

	private IEnumerator UpdateOnlineState()
	{
		while (true)
		{
			UpdatePlayerOnlineValue();
			yield return new WaitForSeconds(1f);
		}
	}

	private void UpdatePlayerOnlineValue()
	{
		_isPlayerOnline = UpdatePlayerOnlineValueInternal();
	}

	private bool UpdatePlayerOnlineValueInternal()
	{
		if (FFSNetwork.IsOnline)
		{
			if (_menuLoading)
			{
				return false;
			}
			int num = 0;
			foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
			{
				if (allPlayer.IsSlotAssigned)
				{
					num++;
				}
			}
			if (num > 1)
			{
				return true;
			}
		}
		return false;
	}

	public void ResetMenuLoading()
	{
		_menuLoading = false;
		UpdatePlayerOnlineValue();
	}

	public void SetMenuLoading()
	{
		_menuLoading = true;
		UpdatePlayerOnlineValue();
	}
}
