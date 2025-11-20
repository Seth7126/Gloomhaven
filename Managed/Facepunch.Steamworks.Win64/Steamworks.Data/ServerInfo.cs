using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Steamworks.Data;

public struct ServerInfo : IEquatable<ServerInfo>
{
	private string[] _tags;

	internal const uint k_unFavoriteFlagNone = 0u;

	internal const uint k_unFavoriteFlagFavorite = 1u;

	internal const uint k_unFavoriteFlagHistory = 2u;

	public string Name { get; set; }

	public int Ping { get; set; }

	public string GameDir { get; set; }

	public string Map { get; set; }

	public string Description { get; set; }

	public uint AppId { get; set; }

	public int Players { get; set; }

	public int MaxPlayers { get; set; }

	public int BotPlayers { get; set; }

	public bool Passworded { get; set; }

	public bool Secure { get; set; }

	public uint LastTimePlayed { get; set; }

	public int Version { get; set; }

	public string TagString { get; set; }

	public ulong SteamId { get; set; }

	public uint AddressRaw { get; set; }

	public IPAddress Address { get; set; }

	public int ConnectionPort { get; set; }

	public int QueryPort { get; set; }

	public string[] Tags
	{
		get
		{
			if (_tags == null && !string.IsNullOrEmpty(TagString))
			{
				_tags = TagString.Split(new char[1] { ',' });
			}
			return _tags;
		}
	}

	internal static ServerInfo From(gameserveritem_t item)
	{
		return new ServerInfo
		{
			AddressRaw = item.NetAdr.IP,
			Address = Utility.Int32ToIp(item.NetAdr.IP),
			ConnectionPort = item.NetAdr.ConnectionPort,
			QueryPort = item.NetAdr.QueryPort,
			Name = item.ServerNameUTF8(),
			Ping = item.Ping,
			GameDir = item.GameDirUTF8(),
			Map = item.MapUTF8(),
			Description = item.GameDescriptionUTF8(),
			AppId = item.AppID,
			Players = item.Players,
			MaxPlayers = item.MaxPlayers,
			BotPlayers = item.BotPlayers,
			Passworded = item.Password,
			Secure = item.Secure,
			LastTimePlayed = item.TimeLastPlayed,
			Version = item.ServerVersion,
			TagString = item.GameTagsUTF8(),
			SteamId = item.SteamID
		};
	}

	public ServerInfo(uint ip, ushort cport, ushort qport, uint timeplayed)
	{
		this = default(ServerInfo);
		AddressRaw = ip;
		Address = Utility.Int32ToIp(ip);
		ConnectionPort = cport;
		QueryPort = qport;
		LastTimePlayed = timeplayed;
	}

	public void AddToHistory()
	{
		SteamMatchmaking.Internal.AddFavoriteGame(SteamClient.AppId, AddressRaw, (ushort)ConnectionPort, (ushort)QueryPort, 2u, (uint)Epoch.Current);
	}

	public async Task<Dictionary<string, string>> QueryRulesAsync()
	{
		return await SourceServerQuery.GetRules(this);
	}

	public void RemoveFromHistory()
	{
		SteamMatchmaking.Internal.RemoveFavoriteGame(SteamClient.AppId, AddressRaw, (ushort)ConnectionPort, (ushort)QueryPort, 2u);
	}

	public void AddToFavourites()
	{
		SteamMatchmaking.Internal.AddFavoriteGame(SteamClient.AppId, AddressRaw, (ushort)ConnectionPort, (ushort)QueryPort, 1u, (uint)Epoch.Current);
	}

	public void RemoveFromFavourites()
	{
		SteamMatchmaking.Internal.RemoveFavoriteGame(SteamClient.AppId, AddressRaw, (ushort)ConnectionPort, (ushort)QueryPort, 1u);
	}

	public bool Equals(ServerInfo other)
	{
		return GetHashCode() == other.GetHashCode();
	}

	public override int GetHashCode()
	{
		return Address.GetHashCode() + SteamId.GetHashCode() + ConnectionPort.GetHashCode() + QueryPort.GetHashCode();
	}
}
