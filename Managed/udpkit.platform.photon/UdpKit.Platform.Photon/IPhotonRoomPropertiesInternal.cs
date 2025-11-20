using System.Collections.Generic;

namespace UdpKit.Platform.Photon;

internal interface IPhotonRoomPropertiesInternal
{
	bool IsOpen { get; set; }

	bool IsVisible { get; set; }

	Dictionary<object, object> CustomRoomProperties { get; }

	HashSet<string> CustomRoomPropertiesInLobby { get; }
}
