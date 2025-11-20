using System;

namespace Photon.Bolt.Internal;

[Flags]
public enum BoltBuildType
{
	DEBUG = 1,
	RELEASE = 2,
	CLOUD = 4
}
