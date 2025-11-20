using System;
using System.Collections.Generic;
using FFSNet;
using UnityEngine.Events;

public interface INetworkSessionService
{
	PlayersChangedEvent OnPlayerJoined { get; set; }

	PlayersChangedEvent OnPlayerLeft { get; set; }

	List<Tuple<string, NetworkPlayer>> GetCharacterAssignations();

	void EndSession();

	bool IsOnline(NetworkPlayer player);

	void StartSession(UnityAction onHostStarted, UnityAction onHostEnded);

	string GetInviteCode();

	void GenerateInviteCode();
}
