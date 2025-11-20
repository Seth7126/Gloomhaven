using System;

namespace VoiceChat;

public interface IUserVoice
{
	bool IsMuted { get; }

	bool HasRecordDevice { get; }

	float Volume { get; set; }

	string Name { get; }

	string PlatformAccountID { get; }

	string PlatformName { get; }

	bool IsSpeaking { get; }

	bool IsHost { get; }

	bool IsPCUser
	{
		get
		{
			string platformName = PlatformName;
			int num;
			switch (platformName)
			{
			default:
				num = ((!(platformName == "EpicGamesStore")) ? 1 : 0);
				break;
			case "Standalone":
			case "Steam":
			case "GoGGalaxy":
				num = 0;
				break;
			}
			return num == 0;
		}
	}

	event Action EventMuteChange;

	void Mute();

	void UnMute();
}
