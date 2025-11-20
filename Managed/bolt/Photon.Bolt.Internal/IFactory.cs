using System;

namespace Photon.Bolt.Internal;

internal interface IFactory
{
	Type TypeObject { get; }

	TypeId TypeId { get; }

	UniqueId TypeKey { get; }

	object Create();

	void Return(object obj);
}
