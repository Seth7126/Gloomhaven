using Epic.OnlineServices;
using UnityEngine;

internal class DummyInvitePlayer : IInvitePlayer
{
	public string Username { get; }

	public Texture2D Avatar { get; }

	public bool IsInvited { get; set; }

	public bool IsOnline { get; }

	public ProductUserId ProductUserID { get; }

	public string CurrentlyPlayingGameName { get; }

	public DummyInvitePlayer(string username, Texture2D avatar, bool isInvited, bool isOnline, ProductUserId productUserID = null, string currentlyPlayingGameName = null)
	{
		Username = username;
		Avatar = avatar;
		IsInvited = isInvited;
		IsOnline = isOnline;
		ProductUserID = productUserID;
		CurrentlyPlayingGameName = currentlyPlayingGameName;
	}
}
