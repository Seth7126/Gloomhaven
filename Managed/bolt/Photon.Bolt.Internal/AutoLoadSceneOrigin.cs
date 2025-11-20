using System;

namespace Photon.Bolt.Internal;

[Flags]
internal enum AutoLoadSceneOrigin
{
	NONE = 0,
	STARTUP = 1,
	SESSION_CREATION = 2
}
